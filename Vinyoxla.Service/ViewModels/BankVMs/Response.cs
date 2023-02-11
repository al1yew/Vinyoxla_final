namespace Vinyoxla.Service.ViewModels.BankVMs
{
    public class Response
    {
        public string Operation { get; set; }
        public string Status { get; set; }
        public Order Order { get; set; }
        public AdditionalInfo AdditionalInfo { get; set; }
    }
}