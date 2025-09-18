=== Plan ===

This may be carried out over multiple sessions. At the end start of each session an entry will be added to this plan to mark I have started, I will then push that change.
I will do the same to mark the end of a session (evidence that i am keeping to the specified timeframe).


- [ ] Add tests to ensure functionality is not broken - Initially tests will set ConfiguationManager.Appsettings to change the configured behaviour
- [ ] Introduce interface to datastores
- [ ] Add constructor to payment service and accept the datastore interface - the payment service shouldn't care or have knowledge of the datastore. If live changes are not needed than the datastore that's injected in can be determined at app startup. If the ability to switch is needed, a factory method can be used in the DI.
- [ ] Tidy up/ abstract out the payment type validation. Fastest improvement would be to deduplicate the null account check. Another option involves payment type specific validators.



=== Work log ===
Starting Test at 19:50


