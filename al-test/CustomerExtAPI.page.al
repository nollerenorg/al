page 55020 "Customer Ext API"
{
    PageType = API;
    APIPublisher = 'custom';
    APIGroup = 'integration';
    APIVersion = 'v1.0';
    EntityName = 'customer';
    EntitySetName = 'customers';
    SourceTable = Customer;
    ODataKeyFields = SystemId;
    DelayedInsert = true;
    layout
    {
        area(content)
        {
            field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
            field(number; Rec."No.") { Caption = 'Number'; }
            field(displayName; Rec.Name) { Caption = 'Display Name'; }
            field(addressLine1; Rec.Address) { Caption = 'Address Line 1'; }
            field(addressLine2; Rec."Address 2") { Caption = 'Address Line 2'; }
            field(city; Rec.City) { Caption = 'City'; }
            field(state; Rec.County) { Caption = 'County'; }
            field(country; Rec."Country/Region Code") { Caption = 'Country'; }
            field(postalCode; Rec."Post Code") { Caption = 'Postal Code'; }
            field(phoneNumber; Rec."Phone No.") { Caption = 'Phone'; }
            field(email; Rec."E-Mail") { Caption = 'Email'; }
            field(salespersonCode; Rec."Salesperson Code") { Caption = 'Salesperson Code'; }
            field(balanceDue; Rec."Balance (LCY)") { Caption = 'Balance Due'; Editable = false; }
            field(creditLimit; Rec."Credit Limit (LCY)") { Caption = 'Credit Limit'; }
            field(taxRegistrationNumber; Rec."VAT Registration No.") { Caption = 'VAT Reg No'; }
            field(currencyCode; Rec."Currency Code") { Caption = 'Currency Code'; }
            field(paymentTermsId; Rec."Payment Terms Code") { Caption = 'Payment Terms'; }
            field(shipmentMethodId; Rec."Shipment Method Code") { Caption = 'Shipment Method'; }
            field(paymentMethodId; Rec."Payment Method Code") { Caption = 'Payment Method'; }
            field(blocked; Rec.Blocked) { Caption = 'Blocked'; }
            field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
        }
    }
    trigger OnInsertRecord(BelowxRec: Boolean): Boolean
    var
        IntVal: Codeunit "INT Validation Mgt";
        CorrId: Guid;
    begin
        CorrId := CreateGuid();
        IntVal.ValidateCustomer(Rec, CorrId);
    end;

    trigger OnModifyRecord(): Boolean
    var
        IntVal: Codeunit "INT Validation Mgt";
        CorrId: Guid;
    begin
        CorrId := CreateGuid();
        IntVal.ValidateCustomer(Rec, CorrId);
    end;
}
