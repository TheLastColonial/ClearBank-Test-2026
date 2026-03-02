# Test Description

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

## Notes

AI was **not used** in this project.
I would have in modern computing due to the force multipler effect that is has.
For this, I would include the plan prompts and the agents/skills used.
However this project was to demonstrate my technical skill.

Given more time;

- I could have included better BDD testing with ReqNRoll
- Talked with Product to give more precice rules or investigated how the contracts were used to improve response richness
- Include metrics and runbook links to error logs for dashboards to help debug production incidents
- Create a NuGet out of the module to help portablity
- Create CI/CD workflows to ensure standards
- Document with UML diagrams how the application is intended to work.
