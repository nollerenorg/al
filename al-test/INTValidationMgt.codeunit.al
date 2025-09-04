codeunit 55060 "INT Validation Mgt"
{
    SingleInstance = false;
    Subtype = Normal;

    procedure ValidateCustomer(var Cust: Record Customer; CorrId: Guid)
    var
        Currency: Record Currency;
    begin
        if Cust."No." = '' then
            Fail('INT1001', CorrId, 'No.', 'Customer Number (No.) must not be blank');
        if Cust.Name = '' then
            Fail('INT1002', CorrId, 'Name', 'Customer Name must not be blank');
        if (Cust."Currency Code" <> '') and (not Currency.Get(Cust."Currency Code")) then
            Fail('INT1003', CorrId, 'Currency Code', StrSubstNo('Currency Code %1 does not exist', Cust."Currency Code"));
    end;

    procedure PrepareJournalLineForInsert(var GJL: Record "Gen. Journal Line"; CorrId: Guid)
    var
        LastLine: Record "Gen. Journal Line";
    begin
        if GJL."Line No." = 0 then begin
            LastLine.SetRange("Journal Template Name", GJL."Journal Template Name");
            LastLine.SetRange("Journal Batch Name", GJL."Journal Batch Name");
            if LastLine.FindLast() then
                GJL."Line No." := LastLine."Line No." + 10000
            else
                GJL."Line No." := 10000;
        end;

        if GJL."Account No." = '' then
            Fail('INT2001', CorrId, 'Account No.', 'Account No. must not be blank');
        if GJL.Amount = 0 then
            Fail('INT2002', CorrId, 'Amount', 'Amount must not be zero');
    end;

    procedure ValidateAttachment(var Att: Record "Ext Attachment"; CorrId: Guid)
    begin
        if Att."File Name" = '' then
            Fail('INT3001', CorrId, 'File Name', 'File Name must not be blank');
        if Att."Mime Type" = '' then
            Fail('INT3002', CorrId, 'Mime Type', 'Mime Type must not be blank');
    end;

    local procedure Fail(Code: Code[10]; CorrId: Guid; FieldName: Text; Msg: Text)
    var
        JsonTxt: Text;
    begin
        JsonTxt := BuildJson(Code, CorrId, FieldName, Msg);
        Error('%1|%2', Code, JsonTxt);
    end;

    local procedure BuildJson(Code: Code[10]; CorrId: Guid; FieldName: Text; Msg: Text): Text
    var
        SafeMsg: Text;
        SafeField: Text;
    begin
        SafeMsg := EscapeQuotes(Msg);
        SafeField := EscapeQuotes(FieldName);
        exit(StrSubstNo('{"code":"%1","correlationId":"%2","field":"%3","message":"%4"}', Code, Format(CorrId), SafeField, SafeMsg));
    end;

    local procedure EscapeQuotes(Input: Text): Text
    begin
        exit(ConvertStr(Input, '"', ''''));
    end;
}
