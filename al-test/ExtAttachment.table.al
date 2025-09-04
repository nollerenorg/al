table 55050 "Ext Attachment"
{
    DataClassification = ToBeClassified;
    fields
    {
        field(1; "File Name"; Text[250]) { DataClassification = CustomerContent; }
        field(2; "Mime Type"; Text[100]) { DataClassification = CustomerContent; }
        field(3; Content; Blob) { DataClassification = CustomerContent; }
    }
    // SystemId implicit primary key
}
