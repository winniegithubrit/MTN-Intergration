namespace BankSystem.Models
{
  public class Refund
  {
    public int Id { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public string? ReferenceIdToRefund { get; set; }  
    public RequestToPay? RequestToPay { get; set; }
  }


}
