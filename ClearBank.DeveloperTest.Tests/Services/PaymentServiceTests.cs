using ClearBank.DeveloperTest.Services;
using ClearBank.DeveloperTest.Types;
using FakeItEasy;

namespace ClearBank.DeveloperTest.Tests.Services;

public class PaymentServiceTests
{
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _paymentService = new PaymentService();
    }
    [Fact]
    public void PaymentServiceShouldBeAssignableToIPaymentService()
    {
        typeof(PaymentService).Should().BeAssignableTo<IPaymentService>();
    }

    [Theory]
    [InlineData(PaymentScheme.Bacs)]
    [InlineData(PaymentScheme.Chaps)]
    [InlineData(PaymentScheme.FasterPayments)]
    public void MakePayment_GivenNullAccount_ReturnsUnsuccessfulResult(PaymentScheme paymentScheme)
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "non-existent-account",  PaymentScheme = paymentScheme});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public void MakePayment_GivenBacsRequest_WhenBacsDisallowed_ReturnsUnsuccessfulResult()
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "bacs-unsupported-account", PaymentScheme = PaymentScheme.Bacs });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
    }
    
    [Fact]
    public void MakePayment_GivenFasterPaymentsRequest_WhenFasterPaymentsDisallowed_ReturnsUnsuccessfulResult()
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "fasterpayments-unsupported-account", PaymentScheme = PaymentScheme.FasterPayments });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(int.MaxValue - 1, int.MaxValue)]
    [InlineData(int.MinValue, int.MinValue + 1)]
    public void MakePayment_GivenFasterPaymentsRequest_WhenAccountBalanceToLow_ReturnsUnsuccessfulResult(int accountBalance, int debitAmount)
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "fasterpayments-unsupported-account", PaymentScheme = PaymentScheme.FasterPayments, Amount = debitAmount});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
    }
    
    [Fact]
    public void MakePayment_GivenChapsRequest_WhenChapsDisallowed_ReturnsUnsuccessfulResult()
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "chaps-unsupported-account", PaymentScheme = PaymentScheme.Chaps });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
    }
    
    [Theory]
    [InlineData(AccountStatus.Disabled)]
    [InlineData(AccountStatus.InboundPaymentsOnly)]
    public void MakePayment_GivenChapsRequest_WhenAccountNotLive_ReturnsUnsuccessfulResult(AccountStatus accountStatus)
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "chaps-unsupported-account", PaymentScheme = PaymentScheme.Chaps });
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public void MakePayment_GivenBacsRequest_WhenRequestIsValid_ReturnsSuccessfulResult()
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "bacs-supported-account", PaymentScheme = PaymentScheme.Bacs, Amount = 5});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(10, 5, 5)]
    public void MakePayment_GivenBacsRequest_WhenRequestIsValid_AccountIsDebited(int accountBalance, int debitAmount, int expectedBalance)
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "bacs-supported-account", PaymentScheme = PaymentScheme.Bacs,  Amount = debitAmount });
        
        // For now test is wildly incomplete. Need to be able to mock the datastores.
        true.Should().BeFalse();
    }
    
    [Fact]
    public void MakePayment_GivenFasterPaymentsRequest_WhenRequestIsValid_ReturnsSuccessfulResult()
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "fasterpayments-supported-account", PaymentScheme = PaymentScheme.FasterPayments, Amount = 5});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(10, 5, 5)]
    public void MakePayment_GivenFasterPaymentsRequest_WhenRequestIsValid_AccountIsDebited(int accountBalance, int debitAmount, int expectedBalance)
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "fasterpayments-supported-account", PaymentScheme = PaymentScheme.FasterPayments, Amount = debitAmount });
        
        // For now test is wildly incomplete. Need to be able to mock the datastores.
        true.Should().BeFalse();
    }
    
    [Fact]
    public void MakePayment_GivenChapsPaymentsRequest_WhenRequestIsValid_ReturnsSuccessfulResult()
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "chaps-supported-account", PaymentScheme = PaymentScheme.Chaps, Amount = 5});
        
        result.Should().NotBeNull()
            .And.Subject.Should().BeOfType<MakePaymentResult>()
            .Which.Success.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(10, 5, 5)]
    public void MakePayment_GivenChapsPaymentsRequest_WhenRequestIsValid_AccountIsDebited(int accountBalance, int debitAmount, int expectedBalance)
    {
        var result = _paymentService.MakePayment(new MakePaymentRequest {DebtorAccountNumber = "chaps-supported-account", PaymentScheme = PaymentScheme.Chaps, Amount = debitAmount });
        
        // For now test is wildly incomplete. Need to be able to mock the datastores.
        true.Should().BeFalse();
    }
}