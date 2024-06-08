using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models
{
  public class Deposit
  {
    [Key]
    public int DepositId { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public Party? Payee { get; set; } 
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
  }
}
