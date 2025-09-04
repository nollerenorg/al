page 55045 "VAT Prod Posting Group API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'taxGroup'; EntitySetName = 'taxGroups';
    SourceTable = "VAT Product Posting Group"; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(code; Rec.Code) { Caption = 'Code'; }
        field(description; Rec.Description) { Caption = 'Description'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
