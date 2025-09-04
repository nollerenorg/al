codeunit 55070 "INT Batch Journal Service"
{
    Subtype = Normal;

    // Batch creation of General Journal Lines via web service.
    // Invoke through OData: POST .../api/custom/integration/v1.0/codeunits/INTBatchJournalService/ProcessJournalLines
    // Request body example:
    // {
    //   "lines": [
    //     {"journalTemplateName":"GENERAL","journalBatchName":"DEFAULT","accountType":"G/L Account","accountNo":"1000","documentNo":"BATCH1","postingDate":"2024-01-31","amount":123.45,"description":"Test line"},
    //     {"journalTemplateName":"GENERAL","journalBatchName":"DEFAULT","accountType":"G/L Account","accountNo":"1000","documentNo":"BATCH1","postingDate":"2024-01-31","amount":0,"description":"Will fail"}
    //   ]
    // }
    // Response body example:
    // {
    //   "lines": [
    //     {"index":0,"status":"Success","id":"<systemId>","lineNo":10000},
    //     {"index":1,"status":"Error","error":{"code":"INT2002","json":{"code":"INT2002","correlationId":"<guid>","field":"Amount","message":"Amount must not be zero"}}}
    //   ]
    // }

    [ServiceEnabled]
    procedure ProcessJournalLines(RequestBody: Text): Text
    var
        JObject: JsonObject;
        LinesToken: JsonToken;
        LinesArray: JsonArray;
        LineToken: JsonToken;
        LineObj: JsonObject;
        ResponseObj: JsonObject;
        ResponseLines: JsonArray;
        LineResult: JsonObject;
        IntVal: Codeunit "INT Validation Mgt";
        GJL: Record "Gen. Journal Line";
        CorrId: Guid;
        i: Integer;
        ParseOk: Boolean;
        RequestErr: Text;
        ResultTxt: Text;
        LastErr: Text;
        ErrCode: Code[10];
        ErrJson: Text;
    begin
        if (RequestBody = '') then
            exit('{"error":{"code":"INT9000","message":"Empty request body"}}');

        ParseOk := JObject.ReadFrom(RequestBody);
        if not ParseOk then
            exit('{"error":{"code":"INT9000","message":"Invalid JSON"}}');

        if not JObject.Get('lines', LinesToken) then
            exit('{"error":{"code":"INT9001","message":"Missing ' + 'lines' + ' array"}}');

        if not LinesToken.IsArray() then
            exit('{"error":{"code":"INT9001","message":"' + 'lines' + ' must be an array"}}');

        LinesArray := LinesToken.AsArray();

        for i := 0 to LinesArray.Count() - 1 do begin
            LinesArray.Get(i, LineToken);
            if not LineToken.IsObject() then begin
                AddMalformedLine(ResponseLines, i, 'Line entry must be an object');
                continue;
            end;
            LineObj := LineToken.AsObject();
            Clear(GJL);
            CorrId := CreateGuid();

            if not MapJournalLine(LineObj, GJL, RequestErr) then begin
                AddMapError(ResponseLines, i, CorrId, RequestErr);
                continue;
            end;

            // Validation & insert (wrapped in TryFunction to continue batch)
            if not TryCreateLine(GJL, CorrId, IntVal) then begin
                LastErr := GetLastErrorText();
                ParseErrorString(LastErr, ErrCode, ErrJson);
                AddErrorLine(ResponseLines, i, ErrCode, ErrJson);
                continue;
            end;

            // Success
            LineResult.Add('index', i);
            LineResult.Add('status', 'Success');
            LineResult.Add('id', Format(GJL.SystemId));
            LineResult.Add('lineNo', GJL."Line No.");
            ResponseLines.Add(LineResult);
            Clear(LineResult);
        end;

        ResponseObj.Add('lines', ResponseLines);
        ResponseObj.WriteTo(ResultTxt);
        exit(ResultTxt);
    end;

    local procedure MapJournalLine(LineObj: JsonObject; var GJL: Record "Gen. Journal Line"; var Err: Text): Boolean
    var
        Token: JsonToken;
        PostingDateTxt: Text;
        PostingDate: Date;
        JTemplate: Text;
        JBatch: Text;
        AccTypeTxt: Text;
        AccNoTxt: Text;
        DocNoTxt: Text;
        DescTxt: Text;
        BalAccNoTxt: Text;
    begin
        if not GetText(LineObj, 'journalTemplateName', JTemplate) then begin
            Err := 'journalTemplateName is required';
            exit(false);
        end;
        if not GetText(LineObj, 'journalBatchName', JBatch) then begin
            Err := 'journalBatchName is required';
            exit(false);
        end;
        GetText(LineObj, 'accountType', AccTypeTxt);
        GetText(LineObj, 'accountNo', AccNoTxt);
        GetText(LineObj, 'documentNo', DocNoTxt);
        if GetText(LineObj, 'postingDate', PostingDateTxt) then begin
            if not Evaluate(PostingDate, PostingDateTxt) then begin
                Err := 'Invalid postingDate';
                exit(false);
            end;
            GJL."Posting Date" := PostingDate;
        end;
        if LineObj.Get('amount', Token) and Token.IsValue() then
            GJL.Amount := Token.AsValue().AsDecimal();
        GetText(LineObj, 'description', DescTxt);
        GetText(LineObj, 'balAccountNo', BalAccNoTxt);

        // Assign to record after raw extraction (type conversions)
        GJL."Journal Template Name" := CopyStr(JTemplate, 1, MaxStrLen(GJL."Journal Template Name"));
        GJL."Journal Batch Name" := CopyStr(JBatch, 1, MaxStrLen(GJL."Journal Batch Name"));
        if AccTypeTxt <> '' then
            Evaluate(GJL."Account Type", AccTypeTxt); // rely on enum value names
        GJL."Account No." := CopyStr(AccNoTxt, 1, MaxStrLen(GJL."Account No."));
        GJL."Document No." := CopyStr(DocNoTxt, 1, MaxStrLen(GJL."Document No."));
        GJL.Description := CopyStr(DescTxt, 1, MaxStrLen(GJL.Description));
        GJL."Bal. Account No." := CopyStr(BalAccNoTxt, 1, MaxStrLen(GJL."Bal. Account No."));
        exit(true);
    end;

    [TryFunction]
    local procedure TryCreateLine(var GJL: Record "Gen. Journal Line"; CorrId: Guid; var IntVal: Codeunit "INT Validation Mgt")
    begin
        IntVal.PrepareJournalLineForInsert(GJL, CorrId);
        GJL.Insert(true);
    end;

    local procedure GetText(JObj: JsonObject; Name: Text; var Dest: Text): Boolean
    var
        T: JsonToken;
    begin
        if JObj.Get(Name, T) and T.IsValue() then begin
            Dest := T.AsValue().AsText();
            exit(true);
        end;
        exit(false);
    end;

    local procedure AddMalformedLine(var Arr: JsonArray; Index: Integer; Msg: Text)
    var
        O: JsonObject;
        ErrObj: JsonObject;
    begin
        O.Add('index', Index);
        O.Add('status', 'Error');
        ErrObj.Add('code', 'INT9002');
        ErrObj.Add('json', CreateSimpleErrorJson('INT9002', Msg));
        O.Add('error', ErrObj);
        Arr.Add(O);
    end;

    local procedure AddMapError(var Arr: JsonArray; Index: Integer; CorrId: Guid; Msg: Text)
    var
        O: JsonObject;
        ErrObj: JsonObject;
    begin
        O.Add('index', Index);
        O.Add('status', 'Error');
        ErrObj.Add('code', 'INT9003');
        ErrObj.Add('json', CreateDetailedErrorJson('INT9003', CorrId, Msg));
        O.Add('error', ErrObj);
        Arr.Add(O);
    end;

    local procedure AddErrorLine(var Arr: JsonArray; Index: Integer; Code: Code[10]; JsonTxt: Text)
    var
        O: JsonObject;
        ErrObj: JsonObject;
    begin
        O.Add('index', Index);
        O.Add('status', 'Error');
        ErrObj.Add('code', Code);
        ErrObj.Add('json', JsonTxt);
        O.Add('error', ErrObj);
        Arr.Add(O);
    end;

    local procedure ParseErrorString(Raw: Text; var Code: Code[10]; var JsonTxt: Text)
    var
        SepPos: Integer;
    begin
        SepPos := StrPos(Raw, '|');
        if SepPos > 0 then begin
            Code := CopyStr(Raw, 1, SepPos - 1);
            JsonTxt := CopyStr(Raw, SepPos + 1);
        end else begin
            Code := 'INT9999';
            JsonTxt := CreateSimpleErrorJson(Code, Raw);
        end;
    end;

    local procedure CreateSimpleErrorJson(Code: Code[10]; Msg: Text): Text
    begin
        exit('{"code":"' + Format(Code) + '","message":"' + ConvertStr(Msg, '"', '''') + '"}');
    end;

    local procedure CreateDetailedErrorJson(Code: Code[10]; CorrId: Guid; Msg: Text): Text
    begin
        exit('{"code":"' + Format(Code) + '","correlationId":"' + Format(CorrId) + '","message":"' + ConvertStr(Msg, '"', '''') + '"}');
    end;
}
