page 55044 "Dimension Value API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'dimensionValue'; EntitySetName = 'dimensionValues';
    SourceTable = "Dimension Value"; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(dimensionCode; Rec."Dimension Code") { Caption = 'Dimension Code'; }
        field(code; Rec.Code) { Caption = 'Code'; }
        field(name; Rec.Name) { Caption = 'Name'; }
        field(blocked; Rec.Blocked) { Caption = 'Blocked'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
