using ClearBank.DeveloperTest.Data;

namespace ClearBank.DeveloperTest.Tests.Data;

public class AccountDataStoreTests
{
    [Fact]
    public void ShouldBeAssignableToIDataStore()
    {
        typeof(AccountDataStore).Should().BeAssignableTo<IDataStore>();
    }
}