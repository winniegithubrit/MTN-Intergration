using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models
{
  public class DepositStatusResponse
  {
    public string? FinancialTransactionId { get; set; }
    public string? ExternalId { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public Party? Payee { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public string? Status { get; set; }
    public Reason? Reason { get; set; }
  }

  public class Reason
  {
    public string? Code { get; set; }
    public string? Message { get; set; }
  }
}
