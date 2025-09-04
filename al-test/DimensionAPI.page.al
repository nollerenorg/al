page 55043 "Dimension API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'dimension'; EntitySetName = 'dimensions';
    SourceTable = Dimension; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(code; Rec.Code) { Caption = 'Code'; }
        field(name; Rec.Name) { Caption = 'Name'; }
        field(blocked; Rec.Blocked) { Caption = 'Blocked'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
