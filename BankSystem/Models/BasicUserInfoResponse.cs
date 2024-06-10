using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models
{
  public class BasicUserInfoResponse
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
}
