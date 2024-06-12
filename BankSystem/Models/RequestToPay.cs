using System.Collections.Generic;
namespace BankSystem.Models
{
  public class Payer
  {
    public string? PartyIdType { get; set; }
    public string? PartyId { get; set; }
  }

  public class RequestToPay
  {
    public string Id { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public Payer? Payer { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
  }

}


