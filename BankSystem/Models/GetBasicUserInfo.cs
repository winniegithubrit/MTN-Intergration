namespace BankSystem.Models
{
  public class GetBasicUserInfo
  {
    public int? Id { get; set; }
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Birthdate { get; set; }
    public string? Locale { get; set; }
    public string? Gender { get; set; }
    public string? Status { get; set; }

    public ICollection<GetAccountBalance>? AccountBalances { get; set; }
    public ICollection<GetAccountBalanceInSpecificCurrency>? AccountBalancesInSpecificCurrency { get; set; }
  }
}
