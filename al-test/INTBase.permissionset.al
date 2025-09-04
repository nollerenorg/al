permissionset 55000 "INT Base"
{
    Assignable = true;
    Caption = 'Integration Base Permissions';
    Permissions =
        tabledata Customer = RIMD,
        tabledata "Gen. Journal Batch" = R,
        tabledata "Gen. Journal Line" = RIMD,
        tabledata "Ext Attachment" = RIMD,
        tabledata Currency = R,
        tabledata "G/L Account" = R,
        tabledata "Bank Account" = RIMD,
        tabledata Dimension = R,
        tabledata "Dimension Value" = R,
        tabledata "VAT Product Posting Group" = R,
        tabledata "Tax Area" = R;
}
