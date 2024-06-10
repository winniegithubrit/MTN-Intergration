using System.ComponentModel.DataAnnotations;

public class CreateInvoiceModel
{
  [Key]
  public int Id { get; set; }
  public string? ExternalId { get; set; }
  public string? Amount { get; set; }
  public string? Currency { get; set; }
  public string? ValidityDuration { get; set; }
  public Party? IntendedPayer { get; set; }
  public Party? Payee { get; set; }
  public string? Description { get; set; }
  public string Status { get; set; } = "Paid";  
}


public class Party
{
  public string? PartyIdType { get; set; }
  public string? PartyId { get; set; }
}

public class GetInvoiceStatusModel
{
  [Required]
  public string? ExternalId { get; set; }
}
