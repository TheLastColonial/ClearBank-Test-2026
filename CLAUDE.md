# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Solution is `ClearBank.DeveloperTest.slnx` (.NET 10, `net10.0` set once in `Directory.Build.props`).

- Build: `dotnet build ClearBank.DeveloperTest.slnx`
- Test all: `dotnet test ClearBank.DeveloperTest.slnx`
- Single test: `dotnet test ClearBank.DeveloperTest.Tests --filter "FullyQualifiedName~<TestName>"`

`TreatWarningsAsErrors` and `AnalysisLevel=latest` are set in `Directory.Build.props`. No formatter or CI config is present in the repo. See `DESIGN.md` for preserved behaviour, deviations and known issues. Mutation testing: `dotnet-stryker --test-runner mtp --project ClearBank.DeveloperTest.csproj` from the test project folder (the default VSTest runner fails here).

## Architecture

Small coding-exercise solution with two projects:

- `ClearBank.DeveloperTest` — the production library (`InternalsVisibleTo` the test project).
- `ClearBank.DeveloperTest.Tests` — xunit.v3 + NSubstitute + MELT tests (MELT needs `Microsoft.Extensions.Logging` at the same major version as the library's Abstractions package). Logging tests assert on level, EventId and structured values only, never on message text. `TestData/PaymentTestData.cs` holds the account/request/validator factories.

This is a behaviour-preserving refactor of the original exercise code. Do not add new validation or exceptions-for-flow, and do not change `Types/` or the `MakePayment(MakePaymentRequest)` signature. Logging only observes: it must never change results or store calls. The one deliberate behaviour change is a single catch at the `MakePayment` boundary: any exception (e.g. a store/network failure) is logged as `PaymentFailed` (Error, EventId 1004, with the exception) and returns `Success = false` instead of propagating. It is not used for flow control, and note a timeout after a DB commit would be reported as a failure.

`Services/PaymentService.cs` (`IPaymentService.MakePayment`) takes `IAccountDataStoreFactory`, `IPaymentSchemeValidatorFactory` and `ILogger<PaymentService>` through its constructor (no parameterless constructor). The constructor throws `ArgumentNullException` for null dependencies. `Validation/PaymentSchemeValidatorFactory` (internal) takes all `IPaymentSchemeValidator`s and throws `ArgumentException` unless there is exactly one per defined `PaymentScheme` (a missing validator would otherwise approve that scheme unchecked); `Create(scheme)` returns that scheme's validator, or an accept-everything `InlineValidator` for a scheme with none. `MakePayment` throws `ArgumentNullException` for a null request. `DependencyInjection/AddPaymentServices` registers everything the library needs (call `AddLogging()` too); it is idempotent. The whole call runs inside a logging scope (`DebtorAccount`, `CreditorAccount` masked to the last 4 characters, and `PaymentScheme`):

1. `IAccountDataStoreFactory.Create()` returns the store; the same store serves the read and the write.
2. Loads the debtor `Account` by `request.DebtorAccountNumber`.
3. Rejects (`Success = false`, no update) if the account is null (logs `AccountNotFound`) or the validator from `IPaymentSchemeValidatorFactory.Create(request.PaymentScheme)` reports failures (logs `PaymentRejected` with `FailedRules`, a list of stable error codes from `Validation/PaymentErrorCodes`: `SchemeNotAllowed`, `InsufficientBalance`, `AccountNotLive`). A scheme with no validator passes (the factory's accept-everything validator), which preserves the original behaviour for unknown `PaymentScheme` values.
4. On success, subtracts `request.Amount` from `Balance`, calls `UpdateAccount`, logs `PaymentProcessed`, and returns `Success = true`.

Supporting pieces:

- `Data/IAccountDataStore` is implemented by the stub `AccountDataStore` and `BackupAccountDataStore` (`GetAccount` returns `new Account()`, `UpdateAccount` does nothing).
- `Data/AccountDataStoreFactory` (internal) reads `IDataStoreSettings.DataStoreType` on every call: `"Backup"` selects `BackupAccountDataStore`, anything else `AccountDataStore`. `ConfigurationDataStoreSettings` (internal) reads `ConfigurationManager.AppSettings["DataStoreType"]`.
- `Logging/PaymentServiceLog` holds the source-generated `[LoggerMessage]` events (EventIds 1001 processed, 1002 account not found, 1003 rejected); `Logging/AccountNumberMask` masks account numbers. Never log the amount, balance, full account numbers, or FluentValidation `ErrorMessage` (it embeds values). Each validator rule declares its code with `WithErrorCode` so logs do not depend on FluentValidation internals.
- `Validation/` has one FluentValidation validator per scheme, all deriving from the internal `PaymentSchemeValidator`, which requires the matching `AllowedPaymentSchemes` flag (`HasFlag`, though the enum has no `[Flags]` attribute). `FasterPaymentsPaymentValidator` adds `Balance >= Amount`; `ChapsPaymentValidator` adds `Status == Live`; `BacsPaymentValidator` adds nothing (balance can go negative).
- `Types/` holds the request/result/account models and enums. `PaymentScheme` (request-side) and `AllowedPaymentSchemes` (account-side flags) are separate types mapped by name in each validator.
- `CreditorAccountNumber` and `PaymentDate` on `MakePaymentRequest` are not used by the current logic.

Constraints from the exercise (`README.md`): the solution must build, all tests must pass, and the `MakePayment(MakePaymentRequest)` signature must not change. The brief asks for SOLID-oriented refactoring, testability, and unit tests in the tests project.

NuGet dependencies of the library: `FluentValidation`, `Microsoft.Extensions.Logging.Abstractions` and `System.Configuration.ConfigurationManager`, versions in `Directory.Packages.props`. No `App.config` is in the repo, so `DataStoreType` is unset unless one is supplied. Known deviation from the original: a null account with an unknown scheme returns `Success = false` instead of throwing `NullReferenceException`.
