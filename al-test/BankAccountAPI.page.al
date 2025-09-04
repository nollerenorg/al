page 55042 "Bank Account API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'bankAccount'; EntitySetName = 'bankAccounts';
    SourceTable = "Bank Account"; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(number; Rec."No.") { Caption = 'Number'; }
        field(name; Rec.Name) { Caption = 'Name'; }
        field(currencyCode; Rec."Currency Code") { Caption = 'Currency Code'; }
        field(iban; Rec.IBAN) { Caption = 'IBAN'; }
        field(swiftCode; Rec."SWIFT Code") { Caption = 'SWIFT'; }
        field(blocked; Rec.Blocked) { Caption = 'Blocked'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
