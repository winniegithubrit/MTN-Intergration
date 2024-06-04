namespace BankSystem.Models
{
  public class GetPaymentStatus
  {
    public int? Id { get; set; }
    public string? XReferenceId { get; set; }

    public int? CreatePaymentId { get; set; }
    public CreatePayment? CreatePayment { get; set; }
  }
}
