using BankSystem.Models;
using System.ComponentModel.DataAnnotations;
public class GetPaymentStatus
{
  public int Id { get; set; }
  public string? Status { get; set; }
  public string? ExternalTransactionId { get; set; }

  // Foreign key to CreatePayment
  public int CreatePaymentId { get; set; }
  public CreatePayment? CreatePayment { get; set; }
}
