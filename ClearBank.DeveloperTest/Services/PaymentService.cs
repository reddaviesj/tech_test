using System;
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Types;
using System.Configuration;

namespace ClearBank.DeveloperTest.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IDataStore _dataStore;

        public PaymentService(IDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        public MakePaymentResult MakePayment(MakePaymentRequest request)
        {
            var account = _dataStore.GetAccount(request.DebtorAccountNumber);

            var result = new MakePaymentResult {Success = true};
            
            if (account is null)
            {
                result.Success = false;
                return result;
            }
            
            switch (request.PaymentScheme)
            {
                case PaymentScheme.Bacs:
                    result.Success = CheckBacsAllowed(account);
                    break;

                case PaymentScheme.FasterPayments:
                    result.Success = CheckFasterPaymentsAllowed(account, request);
                    break;

                case PaymentScheme.Chaps:
                    result.Success = CheckChapsAllowed(account);
                    break;
            }

            if (result.Success)
            {
                account.Balance -= request.Amount;
                _dataStore.UpdateAccount(account);
            }

            return result;
        }

        private static bool CheckBacsAllowed(Account account)
        {
            return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Bacs);
        }
        
        private static bool CheckFasterPaymentsAllowed(Account account, MakePaymentRequest request)
        {
            return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.FasterPayments)
                   && account.Balance >= request.Amount;
        }
        
        private static bool CheckChapsAllowed(Account account)
        {
            return account.AllowedPaymentSchemes.HasFlag(AllowedPaymentSchemes.Chaps)
                && account.Status == AccountStatus.Live;; 
        }
    }
}
