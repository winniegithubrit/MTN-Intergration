namespace BankSystem.Models
{
  public class CreatePayment
  {
    public int Id { get; set; }
    public string? ExternalTransactionId { get; set; }
    public Money? Money { get; set; }
    public string? CustomerReference { get; set; }
    public string? ServiceProviderUserName { get; set; }

    public RequestToPay? RequestToPay { get; set; }
    public GetPaymentStatus? PaymentStatus { get; set; }
  }

  public class Money
  {
    public string? Amount { get; set; }
    public string? Currency { get; set; }
  }
}
