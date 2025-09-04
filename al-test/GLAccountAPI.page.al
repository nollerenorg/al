page 55041 "GL Account API"
{
    PageType = API;
    APIPublisher = 'custom'; APIGroup = 'integration'; APIVersion = 'v1.0';
    EntityName = 'account'; EntitySetName = 'accounts';
    SourceTable = "G/L Account"; ODataKeyFields = SystemId; DelayedInsert = true;
    layout { area(content) {
        field(id; Rec.SystemId) { Caption = 'Id'; Editable = false; }
        field(number; Rec."No.") { Caption = 'Number'; }
        field(name; Rec.Name) { Caption = 'Name'; }
        field(incomeBalance; Rec."Income/Balance") { Caption = 'Income/Balance'; }
        field(accountType; Rec."Account Type") { Caption = 'Account Type'; }
        field(directPosting; Rec."Direct Posting") { Caption = 'Direct Posting'; }
        field(blocked; Rec.Blocked) { Caption = 'Blocked'; }
        field(lastModifiedDateTime; Rec.SystemModifiedAt) { Caption = 'Last Modified'; Editable = false; }
    } }
}
