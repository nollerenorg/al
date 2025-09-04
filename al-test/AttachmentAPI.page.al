page 55051 "Attachment API"
{
    PageType = API;
    APIPublisher = 'custom';
    APIGroup = 'integration';
    APIVersion = 'v1.0';
    EntityName = 'attachment';
    EntitySetName = 'attachments';
    SourceTable = "Ext Attachment";
    ODataKeyFields = SystemId;
    DelayedInsert = true;
    layout
    {
        area(content)
        {
            field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
            field(fileName; Rec."File Name") { Caption = 'File Name'; }
            field(mimeType; Rec."Mime Type") { Caption = 'Mime Type'; }
            field(attachmentContent; Rec.Content) { Caption = 'Attachment Content'; }
        }
    }
    trigger OnInsertRecord(BelowxRec: Boolean): Boolean
    var
        IntVal: Codeunit "INT Validation Mgt";
        CorrId: Guid;
    begin
        CorrId := CreateGuid();
        IntVal.ValidateAttachment(Rec, CorrId);
    end;
}
