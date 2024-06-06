namespace BankSystem.Models
{
  public class GetAccountBalanceInSpecificCurrency
  {
    public int? Id { get; set; }
    public decimal AvailableBalance { get; set; }
    public string? Currency { get; set; }
  }
}
