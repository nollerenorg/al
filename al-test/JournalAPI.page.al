page 55030 "Journal API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'journal'; EntitySetName = 'journals';
    SourceTable = "Gen. Journal Batch"; ODataKeyFields = SystemId; DelayedInsert = true;
    SourceTableView = where("Template Type" = const(General));
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(code; Rec.Name) { Caption = 'Code'; }
        field(description; Rec.Description) { Caption = 'Description'; }
        field(templateName; Rec."Journal Template Name") { Caption = 'Template Name'; }
    } }
}
