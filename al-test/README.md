# Business Central Integration Extension

This extension exposes a curated set of Business Central entities through custom API pages plus a batch journal line creation service.

## API Pages (OData V4)
Base URL pattern:
  https://<server>/<tenant>/<environment>/api/custom/integration/v1.0/

Entity sets:
- customers
- journals
- journalLines
- attachments
- currencies
- gLAccounts
- bankAccounts
- dimensions
- dimensionValues
- vATProductPostingGroups
- taxAreas

Example (list customers):
  GET .../api/custom/integration/v1.0/customers

Single record (journal line by systemId):
  GET .../api/custom/integration/v1.0/journalLines(<systemId>)

Insert example (customer):
  POST .../api/custom/integration/v1.0/customers
  {"number":"C-10000","displayName":"New Cust","currencyCode":"EUR"}

## Attachment Upload
POST attachments with base64 content in field `attachmentContent`.

## Error Format
All validation errors follow: CODE|{JSON}
Client splits on first '|'. JSON object fields:
  code, correlationId, field (when applicable), message

Example raw error string:
  INT2002|{"code":"INT2002","correlationId":"c0f...","field":"Amount","message":"Amount must not be zero"}

## Batch Journal Line Service
Service-enabled codeunit 55070 "INT Batch Journal Service" exposes procedure ProcessJournalLines.
Endpoint (OData):
  POST .../api/custom/integration/v1.0/codeunits/INTBatchJournalService/ProcessJournalLines

Request body:
{
  "lines": [
    {"journalTemplateName":"GENERAL","journalBatchName":"DEFAULT","accountType":"G/L Account","accountNo":"1000","documentNo":"BATCH1","postingDate":"2024-01-31","amount":123.45,"description":"Test line"},
    {"journalTemplateName":"GENERAL","journalBatchName":"DEFAULT","accountType":"G/L Account","accountNo":"1000","documentNo":"BATCH1","postingDate":"2024-01-31","amount":0,"description":"Will fail"}
  ]
}

Response body:
{
  "lines": [
    {"index":0,"status":"Success","id":"<systemId>","lineNo":10000},
    {"index":1,"status":"Error","error":{"code":"INT2002","json":{"code":"INT2002","correlationId":"<guid>","field":"Amount","message":"Amount must not be zero"}}}
  ]
}

Error codes:
- INT100x: Customer validation
- INT200x: Journal line validation
- INT300x: Attachment validation
- INT9000: Invalid / empty JSON
- INT9001: Structural JSON errors (missing arrays)
- INT9002: Malformed line (not an object)
- INT9003: Mapping / required field missing
- INT9999: Unexpected internal failure

## Correlation IDs
Each failing line in batch returns its own correlationId for traceability.

## Permissions
Permission set 55000 "INT Base" grants required access. Assign it to the integration user / service principal.

## Notes
- Journal line numbering auto-increments by 10000 when no line number is provided.
- All pages use ODataKeyFields=SystemId for stable references.
- DelayedInsert=true minimizes side effects until validation passes.

## Future Enhancements (Suggestions)
- Add customer payment / payment journal endpoints if needed.
- Persist batch execution log table with correlation grouping.
- Add pagination & filtering examples to this README.
