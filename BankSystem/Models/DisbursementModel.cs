using System;
using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models
{
  
  // Represents a refund operation
  public class Refund
  {
    [Key]
    public int RefundId { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public string? ReferenceIdToRefund { get; set; } 
    public string? PayerId { get; set; } 

    // Navigation property for the associated user (payer)
    public int UserId { get; set; }
    public User? User { get; set; }
  }

  public class Transfer
  {
    [Key]
    public int TransferId { get; set; }
  
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public string? PayerId { get; set; }
    public Party? Payee { get; set; }

    // Navigation property for the associated user
    public int UserId { get; set; }
    public User? User { get; set; }
  }

  public class User
  {
    public int Id { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Birthdate { get; set; }
    public string? Locale { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }

    // Navigation properties
    public List<Deposit>? Deposits { get; set; }
    public List<Refund>? Refunds { get; set; }
    public List<Transfer>? Transfers { get; set; }
  }

  public class Party
  {
    public string? PartyIdType { get; set; }
    public string? PartyId { get; set; }
  }
}
