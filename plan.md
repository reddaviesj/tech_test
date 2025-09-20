=== Plan ===

This may be carried out over multiple sessions. At the end start of each session an entry will be added to this plan to mark I have started, I will then push that change.
I will do the same to mark the end of a session (evidence that i am keeping to the specified timeframe).


- [x] Add tests to ensure functionality is not broken
- [x] Introduce interface to datastores
- [x] Add constructor to payment service and accept the datastore interface - the payment service shouldn't care or have knowledge of the datastore. If live changes are not needed than the datastore that's injected in can be determined at app startup. If the ability to switch is needed, a factory method can be used in the DI.
- [ ] Tidy up/ abstract out the payment type validation. Fastest improvement would be to deduplicate the null account check. Another option involves payment type specific validators.
- [ ] There is currently no guarding for null. Would ideally deal with this



=== Work log ===
Starting Test at 19:50
Pausing at 20:40 - Outlines of tests written. Will need to move on to interface abstraction to make the tests actually useful and valid though, as I need a way to mock the behaviour.
Resuming 19:25
Pausing at 20:30 - abstracted out datastores to allow testability.
Resuming at 19:30




=== Notes ===
Lack of checks allows for inexpected null reference exceptions and integer over/underflow exceptions