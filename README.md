# Loan Application Take-Home

Video: _add a public Loom/Jam link here after recording the walkthrough._

Small loan application flow:

1. Next.js form
2. .NET rule engine decides approve/deny
3. Approved applications persist `Customer` + `Application` in one SQLite transaction
4. An outbox row is written in that same transaction
5. A background worker sends the payload to a mock HTTP service

## Prerequisites

- .NET 8 SDK
- Node.js 18+

## Run locally

Open three terminals from the repo root.

```bash
# 1. External mock
cd mock-service
npm install
npm start
# http://localhost:4000
```

```bash
# 2. Backend
cd backend/src/LoanApp.Api
dotnet run
# http://localhost:5080
```

```bash
# 3. Frontend
cd frontend
npm install
npm run dev
# http://localhost:3000
```

Health checks:

- Backend: http://localhost:5080/health
- Mock: http://localhost:4000/health
- Mock records: http://localhost:4000/customers

SQLite file is created at `backend/src/LoanApp.Api/loanapp.db`.

## Tests

```bash
cd backend
dotnet test
```

## Test data

Blacklisted SSNs:

- `111-11-1111`
- `000-00-0000`
- `999-99-9999`

| Scenario | What to type |
| --- | --- |
| Approved (new customer) | State `CA`, SSN `123-45-6789`, any valid name/address/company, amount `5000` |
| Denied by NY | Same as above but state `NY` |
| Denied by blacklist | State `CA`, SSN `111-11-1111` |
| Returning customer | Submit the approved example, then submit again with the same SSN and a different amount or name |

Expected results:

- NY / blacklist: denied page, no DB rows, no mock payload
- First approval: one customer + one application + mock `POST /customers`
- Same SSN again: same IDs updated in DB, mock `PUT /customers/{ssn}`

## Missing from this submission

- The walkthrough video link (record after running the four flows)
- Docker / CI (not required to prove the design)
