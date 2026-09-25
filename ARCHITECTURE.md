# Architecture

## Structure

```
loan-app/
  backend/
    src/LoanApp.Domain          business entities and rules
    src/LoanApp.Application     use case, ports, validation
    src/LoanApp.Infrastructure  EF Core, outbox worker, HTTP client
    src/LoanApp.Api             thin HTTP host
    tests/LoanApp.Tests
  frontend/                     Next.js form + result pages
  mock-service/                 Express stand-in for the partner API
```

Dependencies point inward: Api → Infrastructure → Application → Domain.

The controller only binds the request and returns the result. Persistence, HTTP, and messaging stay behind interfaces defined in Application.

## Rule engine

`RuleEngine` receives an ordered list of `IDenialRule`. Each rule returns a `Decision` or `null`. The first denial wins. If none match, the application is approved.

Current rules:

- `NyStateDenialRule` — state `NY`
- `BlacklistedSsnDenialRule` — SSN present in `ISsnBlacklist`

To add a rule:

1. Create a class that implements `IDenialRule`.
2. Register it as `IDenialRule` in `Application/DependencyInjection.cs`.

Existing rules do not change. The engine does not know about controllers or EF.

## Persistence and returning customers

SSN is stored as 9 digits and is unique. Each customer has at most one application (`CustomerId` unique).

On approve:

- unknown SSN → insert customer + insert application
- known SSN → update customer fields + update `requestedAmount`

Denied applications write nothing.

## Transactions and background events

The unit of work is one SQLite transaction that includes:

1. customer insert or update
2. application insert or update
3. insert of an outbox row (`CustomerUpserted`)

`SaveChanges` and `Commit` happen together. If any step throws, the transaction rolls back: no customer, no application, no event.

The HTTP call to the mock is **not** inside the request or the DB transaction. A hosted service reads unpublished outbox rows every two seconds and then:

- `POST /customers` when `isUpdate` is false
- `PUT /customers/{ssn}` when `isUpdate` is true

If the HTTP call fails, the outbox row stays pending and is retried. The local database is not rolled back after commit. That is the trade-off of processing the event after the HTTP response.

Calling the partner API inside the original request would violate the "background event" rule and would hold a DB transaction open across the network.

## External contract

Mock base URL: `http://localhost:4000`

| Method | Path | When |
| --- | --- | --- |
| POST | `/customers` | first approval for an SSN |
| PUT | `/customers/{ssn}` | returning customer |
| GET | `/customers` | inspection during the demo |

Payload (camelCase):

```json
{
  "customerId": "guid",
  "applicationId": "guid",
  "firstName": "Ada",
  "lastName": "Lovelace",
  "address": "1 Computing Lane",
  "state": "CA",
  "companyName": "Analytical Engines",
  "ssn": "123456789",
  "requestedAmount": 5000,
  "isUpdate": false
}
```

The mock always returns 200 and keeps the last payload per SSN in memory. Retries exist only on the publisher side. Idempotency is "last write wins" by SSN, which matches the one-customer/one-application model.

## Trade-offs left out

- No message bus. An in-process outbox is enough for one host.
- No EF in-memory provider. The app and the service tests use SQLite so transactions are real.
- No auth, no Docker, no extra DDD artifacts (no aggregates beyond the two records, no domain events besides the outbox row).
- No generic rules DSL. Two interfaces cover "add a rule without editing old ones."
- Frontend talks to the API directly. No BFF.
