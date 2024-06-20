using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models
{
  public class AccessTokenResponse
  {
    public string? access_token { get; set; }
    public string? token_type { get; set; }
    public int expires_in { get; set; }
  }
  public class GetAccountBalance
  {
    public string? AvailableBalance { get; set; }
    public string? Currency { get; set; }
  }

  public class BasicUserInfo
  {
    [Key]
    public int Id { get; set; }

    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Birthdate { get; set; }
    public string? Locale { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }
  }
  public class CreateInvoiceModel
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string? ExternalId { get; set; }

    [Required]
    public string? Amount { get; set; }

    [Required]
    public string? Currency { get; set; }

    public string? ValidityDuration { get; set; }

    [Required]
    public Party? IntendedPayer { get; set; }

    public Party? Payee { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }
  }

  public class Party
  {
    [Required]
    public string? PartyIdType { get; set; }

    [Required]
    public string? PartyId { get; set; }
  }

  public class GetInvoiceStatusModel
  {
    [Required]
    public string? ExternalId { get; set; }
  }

  public class CreatePayment
  {
    public CreatePayment()
    {
      Id = Guid.NewGuid().ToString();
    }

    [Key]
    public string Id { get; set; }
    public string? ExternalTransactionId { get; set; }
    public Money? Money { get; set; }
    public string? CustomerReference { get; set; }
    public string? ServiceProviderUserName { get; set; }
    public string? CouponId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductOfferingId { get; set; }
    public string? ReceiverMessage { get; set; }
    public string? SenderNote { get; set; }
    public int MaxNumberOfRetries { get; set; }
    public bool IncludeSenderCharges { get; set; }
    public string PaymentStatus { get; set; }
  }

  public class Money
  {
    public string? Amount { get; set; }
    public string? Currency { get; set; }
  }

  public class PaymentResult
  {
    public string? ReferenceId { get; set; }
    public string? Status { get; set; }
    public string? FinancialTransactionId { get; set; }
    public string? Reason { get; set; }
  }

  public class Deposit
  {
    public Deposit()
    {
      DepositId = Guid.NewGuid().ToString();
    }

    [Key]
    public string DepositId { get; set; }

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


  public class DepositResult
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

  public class GetAccountBalanceInSpecificCurrency
  {
    public int? Id { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? Currency { get; set; }
  }

  public class Refund
  {
    public int Id { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
    public string? ReferenceIdToRefund { get; set; }
  }

  public class Payer
  {
    public string? PartyIdType { get; set; }
    public string? PartyId { get; set; }
  }

  public class RequestToPay
  {
    public RequestToPay()
    {
      Id = Guid.NewGuid().ToString();
    }

    [Key]
    public string Id { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public string? ExternalId { get; set; }
    public Payer? Payer { get; set; }
    public string? PayerMessage { get; set; }
    public string? PayeeNote { get; set; }
  }

}
