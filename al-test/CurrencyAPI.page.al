page 55040 "Currency API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'currency'; EntitySetName = 'currencies';
    SourceTable = Currency; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(code; Rec.Code) { Caption = 'Code'; }
        field(description; Rec.Description) { Caption = 'Description'; }
        field(amountRoundingPrecision; Rec."Amount Rounding Precision") { Caption = 'Amount Rounding Precision'; }
        field(invoiceRoundingPrecision; Rec."Invoice Rounding Precision") { Caption = 'Invoice Rounding Precision'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
