using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using FakeItEasy;

namespace ClearBank.DeveloperTest.Tests.Services;

public class PaymentServiceTests
{
    private readonly PaymentService _paymentService;
    private readonly IDataStore _dataStore;

    public PaymentServiceTests()
    {
        _dataStore = A.Fake<IDataStore>();
        _paymentService = new PaymentService(_dataStore);
    }
    
    [Fact]
    public void PaymentServiceShouldBeAssignableToIPaymentService()
    {
        typeof(PaymentService).Should().BeAssignableTo<IPaymentService>();
    }

    [Fact]
    public void Constructor_GivenNullDataStore_ThrowsArgumentNullException()
    {
        Action ctor = () => new PaymentService(default(IDataStore));
        
        ctor.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dataStore");
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.Chaps)]
    [InlineData(PaymentScheme.FasterPayments)]
    public void MakePayment_GivenNullAccount_ReturnsUnsuccessfulResult(PaymentScheme paymentScheme)
    {
        const string debtorAccountNumber = "non-existent-account";
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(null);

        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber,  PaymentScheme = paymentScheme});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public void MakePayment_GivenBacsRequest_WhenBacsDisallowed_ReturnsUnsuccessfulResult()
    {
        const string debtorAccountNumber = "bacs-unsupported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Bacs });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.Ignored)).MustNotHaveHappened();
    }
    
    [Fact]
    public void MakePayment_GivenFasterPaymentsRequest_WhenFasterPaymentsDisallowed_ReturnsUnsuccessfulResult()
    {
        const string debtorAccountNumber = "fasterpayments-unsupported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.Chaps
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.FasterPayments });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.Ignored)).MustNotHaveHappened();
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(int.MaxValue - 1, int.MaxValue)]
    [InlineData(int.MinValue, int.MinValue + 1)]
    public void MakePayment_GivenFasterPaymentsRequest_WhenAccountBalanceToLow_ReturnsUnsuccessfulResult(int accountBalance, int debitAmount)
    {
        const string debtorAccountNumber = "fasterpayments-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = accountBalance
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "fasterpayments-supported-account", PaymentScheme = PaymentScheme.FasterPayments, Amount = debitAmount});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.Ignored)).MustNotHaveHappened();
    }
    
    [Fact]
    public void MakePayment_GivenChapsRequest_WhenChapsDisallowed_ReturnsUnsuccessfulResult()
    {
        const string debtorAccountNumber = "chaps-unsupported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments,
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Chaps });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.Ignored)).MustNotHaveHappened();
    }
    
    [Theory]
    [InlineData(AccountStatus.Disabled)]
    [InlineData(AccountStatus.InboundPaymentsOnly)]
    public void MakePayment_GivenChapsRequest_WhenAccountNotLive_ReturnsUnsuccessfulResult(AccountStatus accountStatus)
    {
        const string debtorAccountNumber = "account-not-live";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Status = accountStatus
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Chaps });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.Ignored)).MustNotHaveHappened();
    }

    [Fact]
    public void MakePayment_GivenBacsRequest_WhenRequestIsValid_ReturnsSuccessfulResult()
    {
        const string debtorAccountNumber = "bacs-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = 7
        };
                
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Bacs, Amount = 5});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeTrue();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.That.Matches(a =>
            a.AccountNumber == debtorAccountNumber && account.Balance == 2))).MustHaveHappenedOnceExactly();
    }
    
    [Theory]
    [InlineData(10, 5, 5)]
    public void MakePayment_GivenBacsRequest_WhenRequestIsValid_AccountIsDebited(int accountBalance, int debitAmount, int expectedBalance)
    {
        const string debtorAccountNumber = "bacs-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = accountBalance,
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Bacs,  Amount = debitAmount });
        
        account.Balance.Should().Be(expectedBalance);
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.That.Matches(a =>
            a.AccountNumber == debtorAccountNumber && account.Balance == expectedBalance))).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public void MakePayment_GivenFasterPaymentsRequest_WhenRequestIsValid_ReturnsSuccessfulResult()
    {
        const string debtorAccountNumber = "fasterpayments-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = 7
        };
                
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.FasterPayments, Amount = 5});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeTrue();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.That.Matches(a =>
            a.AccountNumber == debtorAccountNumber && account.Balance == 2))).MustHaveHappenedOnceExactly();
    }
    
    [Theory]
    [InlineData(11, 5, 6)]
    public void MakePayment_GivenFasterPaymentsRequest_WhenRequestIsValid_AccountIsDebited(decimal accountBalance, decimal debitAmount, decimal expectedBalance)
    {
        const string debtorAccountNumber = "fasterpayments-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = accountBalance,
        };
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.FasterPayments, Amount = debitAmount });
        
        account.Balance.Should().Be(expectedBalance);
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.That.Matches(a =>
            a.AccountNumber == debtorAccountNumber && account.Balance == expectedBalance))).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public void MakePayment_GivenChapsPaymentsRequest_WhenRequestIsValid_ReturnsSuccessfulResult()
    {
        const string debtorAccountNumber = "chaps-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = 7
        };
                
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Chaps, Amount = 5});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeTrue();
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.That.Matches(a =>
            a.AccountNumber == debtorAccountNumber && account.Balance == 2))).MustHaveHappenedOnceExactly();
    }
    
    [Theory]
    [InlineData(10, 5, 5)]
    public void MakePayment_GivenChapsPaymentsRequest_WhenRequestIsValid_AccountIsDebited(decimal accountBalance, decimal debitAmount, decimal expectedBalance)
    {
        const string debtorAccountNumber = "chaps-supported-account";
        
        var account = new Account
        {
            AccountNumber = debtorAccountNumber,
            AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs | AllowedPaymentSchemes.FasterPayments | AllowedPaymentSchemes.Chaps,
            Balance = accountBalance,
            Status = AccountStatus.Live
        };
                
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).Returns(account);
        
        _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = debtorAccountNumber, PaymentScheme = PaymentScheme.Chaps, Amount = debitAmount });
        
        account.Balance.Should().Be(expectedBalance);
        
        A.CallTo(() => _dataStore.GetAccount(debtorAccountNumber)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dataStore.UpdateAccount(A<Account>.That.Matches(a =>
            a.AccountNumber == debtorAccountNumber && account.Balance == expectedBalance))).MustHaveHappenedOnceExactly();
    }
}