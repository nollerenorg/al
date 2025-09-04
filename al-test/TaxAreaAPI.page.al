page 55046 "Tax Area API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'taxArea'; EntitySetName = 'taxAreas';
    SourceTable = "Tax Area"; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(code; Rec.Code) { Caption = 'Code'; }
        field(description; Rec.Description) { Caption = 'Description'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
