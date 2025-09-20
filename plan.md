### Actions taken / Reasoning

My initial thought was that the PaymentService.MakePayment method would be untestible, inspite of that I started by scaffolding out all the tests I wanted in place. While the vast majority of the tests passed, this was not for the correct reason.

Once these tests were in-place, I added an interface for the datastores, and then added a constructor and injected in the datastore. Injecting in the dependencies decouples the PaymentService from the two different datastores. It allowed me to control the behaviour for testing purposes by using mocks. Depending on the requirements the datastore could ever be configured during application startup, or if the datastore needs to be configurable at run time, a factory method could be used to instantiate the correct implementation on the fly.

At this point I revisited the tests, and added mocking behaviour and call verification to all the tests, at this point all the tests correctly failed (because I asserted that calls to GetAccount and UpdateAccount must be made).

Next I updated MakePayment to use the injected datastore rather than instantiating copies. One this was finished the tests started passing.

With working tests, I moved on tidying up the paymentScheme verifcation logic. I started out by pulling out all the null account checks, and adding one before the switch statement (I added in an early return to avoid any further checks). This reduces the amount of code to maintain, and reduces the risk of null reference exceptions.
Next I pulled each section from the switch into private methods. While this doesn't really change much, it does tidy the method body up, making it more readable. I did conider adding paymentValidators, and a PaymentSchemeValidatorFactory. This would have allowed pulling all of this logic out of the PaymentService class at the expense of a little more architectural complexity. I didn't carry this out purely for timing reasons. 

Below you'll see my initial thoughts, along with a short work log (how I was ensuring I was sticking to the test timeframe).


### Plan

This may be carried out over multiple sessions. At the end start of each session an entry will be added to this plan to mark I have started, I will then push that change.
I will do the same to mark the end of a session (evidence that i am keeping to the specified timeframe).


- [x] Add tests to ensure functionality is not broken
- [x] Introduce interface to datastores
- [x] Add constructor to payment service and accept the datastore interface - the payment service shouldn't care or have knowledge of the datastore. If live changes are not needed than the datastore that's injected in can be determined at app startup. If the ability to switch is needed, a factory method can be used in the DI.
- [x] Tidy up/ abstract out the payment type validation. Fastest improvement would be to deduplicate the null account check. Another option involves payment type specific validators.
- [ ] There is currently no guarding for null. Would ideally deal with this



### Work log 
Starting Test at 19:50
Pausing at 20:40 - Outlines of tests written. Will need to move on to interface abstraction to make the tests actually useful and valid though, as I need a way to mock the behaviour.
Resuming 19:25
Pausing at 20:30 - abstracted out datastores to allow testability.
Resuming at 19:30
Stopping at 19:59 - De-duplicated the account logic, and pulled out the payment scheme checks to private methods. For the time left this felt like a pragmatic improvement




### Notes

Lack of checks allows for inexpected null reference exceptions and integer over/underflow exceptions
