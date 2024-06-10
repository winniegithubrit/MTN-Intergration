using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models
{
  public class Deposit
  {
    [Key]
    public int DepositId { get; set; }

    [Required]
    public string? Amount { get; set; }

    [Required]
    public string? Currency { get; set; }

    [Required]
    public string? ExternalId { get; set; }

    [Required]
    public Party? Payee { get; set; }

    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
  }

  public class Party
  {
    [Required]
    public string? PartyIdType { get; set; }

    [Required]
    public string? PartyId { get; set; }
  }
}
