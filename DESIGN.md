# Design notes

A behaviour-preserving refactor of `PaymentService.MakePayment`. The signature, the `Types/` models and `IPaymentService` are unchanged.

## Structure
| Concern | Where |
|---|---|
| Orchestration | `Services/PaymentService` |
| Which data store (config-driven) | `Data/AccountDataStoreFactory` + `IDataStoreSettings` |
| Scheme rules (one FluentValidation validator per scheme) | `Validation/{Bacs,FasterPayments,Chaps}PaymentValidator` |
| Picking the validator for a scheme, and rejecting bad wiring | `Validation/PaymentSchemeValidatorFactory` |
| Structured logging and account-number masking | `Logging/` |
| Wiring | `DependencyInjection/AddPaymentServices` |

## Request flow
`PaymentService` takes `IAccountDataStoreFactory`, `IPaymentSchemeValidatorFactory` and `ILogger<PaymentService>`. For each call it:
1. Throws `ArgumentNullException` for a null request, then opens a logging scope.
2. Creates the data store and loads the debtor account. A missing account is logged and returns `Success = false`.
3. Asks the validator factory for the validator for `request.PaymentScheme` and validates the account and request. Any failure is logged (as error codes) and returns `Success = false`.
4. Otherwise debits the balance, calls `UpdateAccount` on the same store, logs success and returns `Success = true`.

An exception from steps 2 to 4 is caught once at the boundary, logged, and returns `Success = false`.

## Wiring and its safety checks
- `AddPaymentServices` registers the settings, data store factory, the three validators, the validator factory and the service. It is idempotent, and the caller supplies logging (`AddLogging()`).
- Adding a scheme means adding a validator and registering it.
- `PaymentSchemeValidatorFactory` throws `ArgumentException` unless there is exactly one validator for every defined `PaymentScheme`, and no duplicate for any other value. Without this, a scheme with no validator would be accepted with no rules at all.
- The check runs when the factory is constructed, which is when `IPaymentService` is first resolved. That is usually the first payment, not host startup (`ValidateOnBuild` does not run constructors). A host that wants startup failure should resolve `IPaymentService` eagerly.
- The check protects callers who compose the public types themselves. Tests cover the check and the real registration, including that every `PaymentScheme` value has a validator.
- `PaymentService` and `PaymentSchemeValidatorFactory` throw `ArgumentNullException` for null dependencies.

## Behaviour deliberately preserved
- Bacs has no balance or status check, so its balance may go negative.
- FasterPayments needs `Balance >= Amount`. Chaps needs `AccountStatus.Live`.
- A `PaymentScheme` value with no validator is accepted when the account exists. The factory returns an accept-everything validator for it.
- Zero and negative amounts are accepted, as before.
- The data store setting is read on every call, and `"Backup"` is matched case-sensitively.

## Deliberate deviations from the original
1. A missing account with an unknown scheme returns `Success = false` (it used to throw `NullReferenceException`).
2. One catch at the `MakePayment` boundary logs the exception and returns `Success = false`. Previously store failures propagated. A timeout after a database commit would be reported as a failure.
3. A null request throws `ArgumentNullException` instead of `NullReferenceException`.
4. `PaymentService` has no parameterless constructor.

## Logging
- Scope on every call: `DebtorAccount` and `CreditorAccount` (masked to the last 4 characters) and `PaymentScheme`.
- Events: 1001 processed, 1002 account not found, 1003 rejected (`FailedRules` as a list of stable codes from `PaymentErrorCodes`), 1004 failed unexpectedly (with the exception).
- Never logged: amounts, balances, full account numbers, or FluentValidation messages (they embed values).
- Tests assert level, EventId, event name and structured values, never message text.

## Known issues, not addressed here
- Negative amounts are accepted (see above) and would credit the debtor.
- The balance is read, changed and written without a concurrency check, so two simultaneous payments could both succeed. A real store would need optimistic concurrency.
- `Account` is mutable and is debited in memory before `UpdateAccount`.

## Testing
`dotnet test ClearBank.DeveloperTest.slnx` (xunit.v3, NSubstitute, MELT). Warnings are errors.

Mutation testing with Stryker.NET (`dotnet-stryker --test-runner mtp --project ClearBank.DeveloperTest.csproj`, from the test project folder) scored 90.0% on the current code: 54 killed, 5 survived, 1 not covered. The survivors are:
- Three equivalent mutants in `PaymentService`: `new MakePaymentResult { Success = false }` equals the default `new MakePaymentResult { }`.
- The `", "` separator in the `PaymentSchemeValidatorFactory` error message. Pinning it would only couple a test to wording.
- The unreached body of the stub `BackupAccountDataStore.GetAccount`.
- One real gap: the `"DataStoreType"` key in `ConfigurationDataStoreSettings`, which needs an `App.config` to read. It is only exercised through the data store factory with a substituted settings object.
