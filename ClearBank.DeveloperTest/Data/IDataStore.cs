using ClearBank.DeveloperTest.Types;

namespace ClearBank.DeveloperTest.Data;

public interface IDataStore
{
    Account GetAccount(string accountNumber);
    void UpdateAccount(Account account);
}