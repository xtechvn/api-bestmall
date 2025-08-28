using HuloToys_Service.Models.Models;

namespace HuloToys_Service.Models.Client
{
    public class ClientAffiliatePaymentRequestModel: ClientAddressGeneralRequestModel
    {
        public BankingAccount detail { get; set; }  
    }
}
