using ClearBank.DeveloperTest.Data;

namespace ClearBank.DeveloperTest.Tests.Data;

public class BackupAccountDataStoreTests
{
    [Fact]
    public void ShouldBeAssignableToIDataStore()
    {
        typeof(BackupAccountDataStore).Should().BeAssignableTo<IDataStore>();
    }
}