### Test Description
In the 'PaymentService.cs' file you will find a method for making a payment. At a high level the steps for making a payment are:

 - Lookup the account the payment is being made from
 - Check the account is in a valid state to make the payment
 - Deduct the payment amount from the account's balance and update the account in the database
 
What we’d like you to do is refactor the code with the following things in mind:  
 - Adherence to SOLID principals
 - Testability  
 - Readability 

We’d also like you to add some unit tests to the ClearBank.DeveloperTest.Tests project to show how you would test the code that you’ve produced. The only specific ‘rules’ are:  

 - The solution should build.
 - The tests should all pass.
 - You should not change the method signature of the MakePayment method.

You are free to use any frameworks/NuGet packages that you see fit.  
 
You should plan to spend around 1 to 3 hours to complete the exercise.

---

### Submission notes
- Requires the .NET 10 SDK. Build and run all tests with `dotnet test ClearBank.DeveloperTest.slnx` (`global.json` selects the Microsoft Testing Platform runner).
- `MakePayment(MakePaymentRequest)` keeps its signature. `PaymentService` is now constructed with its dependencies; `services.AddPaymentServices()` (plus `AddLogging()`) registers everything for dependency injection.
- [DESIGN.md](DESIGN.md) describes the structure, the behaviour deliberately preserved, the small deviations from the original, the logging approach and the known issues.
- [review-comments.md](review-comments.md) records the earlier review feedback on this exercise and how this version responds to each point.

### AI usage disclosure
- This solution was built with AI assistance: Claude Code, using Claude Sonnet 5.5.
- The assistant was used for planning, implementing the refactor and its unit tests test-first, writing the documentation, and running a mutation-testing check (Stryker.NET).
- The author directed the work, chose between the design options the assistant proposed (for example constructor injection, FluentValidation with one validator per scheme, masked account numbers in logs, the boundary exception handler), and approved each step.
- Commits made through the tool carry a `Co-Authored-By: Claude` trailer. [CLAUDE.md](CLAUDE.md) holds the project instructions given to the assistant.
