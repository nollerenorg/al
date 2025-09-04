page 55031 "Journal Line API"
{
    PageType = API;
    APIPublisher = 'custom';
    APIGroup = 'integration';
    APIVersion = 'v1.0';
    EntityName = 'journalLine';
    EntitySetName = 'journalLines';
    SourceTable = "Gen. Journal Line";
    ODataKeyFields = SystemId;
    DelayedInsert = true;
    layout
    {
        area(content)
        {
            field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
            field(journalTemplateName; Rec."Journal Template Name") { Caption = 'Template Name'; }
            field(journalBatchName; Rec."Journal Batch Name") { Caption = 'Batch Name'; }
            field(lineNo; Rec."Line No.") { Caption = 'Line No'; Editable = false; }
            field(accountType; Rec."Account Type") { Caption = 'Account Type'; }
            field(accountNo; Rec."Account No.") { Caption = 'Account No'; }
            field(documentNo; Rec."Document No.") { Caption = 'Document No'; }
            field(postingDate; Rec."Posting Date") { Caption = 'Posting Date'; }
            field(amount; Rec.Amount) { Caption = 'Amount'; }
            field(description; Rec.Description) { Caption = 'Description'; }
            field(balAccountNo; Rec."Bal. Account No.") { Caption = 'Bal. Account No'; }
        }
    }
    trigger OnInsertRecord(BelowxRec: Boolean): Boolean
    var
        IntVal: Codeunit "INT Validation Mgt";
        CorrId: Guid;
    begin
        CorrId := CreateGuid();
        IntVal.PrepareJournalLineForInsert(Rec, CorrId);
    end;
}
