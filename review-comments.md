# Review comments and how this version responds

## Original review feedback

Areas for Review

Excessive use of exceptions for control flow leading to hard-to-read and verbose logic.
“Exceptions are used to guide control flow, which leads to verbose logic in the payment service.”
“The candidate also excessively used Exceptions to control the flow of their code making it hard to read and follow.”

Changes in payment service behavior that break existing functionality and do not follow refactor rules.
“The candidate significantly changed the behaviour of the payment service and did not follow the rules of a refactor.”
“The candidate invented new use cases for the AccountStatus.InboundPaymentsOnly and implemented them in a way which broke existing behaviour.”

Test design issues including brittle assertions on log messages.
“Several tests assert on specific log messages, which feels brittle and couples tests tightly to implementation details.”

New validation rules added that cause previously accepted requests to fail and inconsistent application of validation rules.
“New validation rules were added for the request which would fail requests that were previously accepted.”
“The candidate applied the Live AccountStatus validation rule to all of the payment schemes when it originally only applied to Bacs.”

## How this version responds

### 1. Exceptions used for control flow
**Respected.** Every business outcome is an ordinary return value.
- `PaymentService.Process` is a short sequence of early returns: no account, validation failures, success. Nothing is thrown to signal a rejected payment.
- Validation failures come back as FluentValidation results (`ValidationResult.Errors`) and are logged as a list of error codes. They are never raised as exceptions.
- Exceptions remain only for things that are not business outcomes: a null request or null dependency (`ArgumentNullException`), and a validator set that is missing or duplicates a scheme (`ArgumentException`, raised when the factory is built, not per payment).
- **One deliberate exception handler:** a single `catch` at the `MakePayment` boundary logs unexpected infrastructure failures (for example a database outage) and returns `Success = false`. It is not used to steer normal flow, but it is a behaviour change from the original, where those exceptions propagated. It is listed under "Deliberate deviations" in `DESIGN.md`, including the caveat that a timeout after a database commit would be reported as a failure.

### 2. Behaviour changes and the rules of a refactor
**Respected, with three small, documented deviations.** The work was done test-first. Characterization tests pinned the original behaviour that was reachable through the stub data store (only a few cases are, because the stub returns an account that allows no schemes). The per-scheme rules were then written test-first, case by case, against the original `switch` logic.
- Bacs: account must allow the scheme. No balance or status check, so its balance can still go negative.
- FasterPayments: account must allow the scheme, and `Balance >= Amount`.
- Chaps: account must allow the scheme, and `Status == Live`.
- A `PaymentScheme` value with no rules is still accepted when the account exists. The validator factory returns an accept-everything validator for it.
- The data store is still chosen by `DataStoreType == "Backup"` (case-sensitive), read on every call. The same store serves the read and the write.
- **`AccountStatus.InboundPaymentsOnly` has no new meaning.** It appears in no production rule. It behaves as it always did: not `Live`, so it is rejected by Chaps and irrelevant to Bacs and FasterPayments. Tests (`Account_status_does_not_affect_validity`, `Is_invalid_when_the_account_is_not_live`) pin this for every status.
- `MakePayment(MakePaymentRequest)`, `IPaymentService`, and everything in `Types/` are unchanged.
- **Deviations (all in `DESIGN.md`):** the boundary catch above; a null request now throws `ArgumentNullException` instead of `NullReferenceException`; a missing account combined with an unknown scheme returns `Success = false` instead of throwing `NullReferenceException`. `PaymentService` also no longer has a parameterless constructor, because its dependencies are injected.
- Logging is new, but it only observes. It cannot change a result or a store call.

### 3. Brittle assertions on log messages
**Respected.** No test asserts on the text of a log message.
- Logging tests assert the log level, the EventId and event name, and structured values: the scope (masked accounts and scheme) and the `FailedRules` list of error codes from `PaymentErrorCodes`.
- Error codes are declared explicitly on each rule (`SchemeNotAllowed`, `InsufficientBalance`, `AccountNotLive`), so tests do not depend on FluentValidation internals or on wording.
- The one place the rendered message is read is a negative privacy check: it asserts that full account numbers, amounts and balances never appear anywhere in the log output. Rewording a message cannot break it.
- Behaviour tests use a null logger and are fully decoupled from logging.

### 4. New validation rules and inconsistent application
**Respected.** No validation was added to the request.
- No new checks on amount, account numbers, dates or the creditor. Zero and negative amounts are accepted exactly as before (and are listed as a known issue in `DESIGN.md` rather than silently "fixed").
- Rules are applied per scheme and only to that scheme. The `Live` status rule is part of the Chaps validator only, as in the code supplied in this repository; Bacs and FasterPayments have no status check. Tests pin each scheme's rules individually, and a service test confirms that only the requested scheme's rules run.
- The reviewer's remark described the `Live` rule as originally belonging to Bacs. In the original `PaymentService` supplied here, the `AccountStatus.Live` check is in the Chaps case and Bacs has none, so this version follows that code.
- The only construction-time checks (a validator for every defined scheme, no duplicates) validate the wiring, not requests, so they cannot cause a previously accepted payment to be rejected.
