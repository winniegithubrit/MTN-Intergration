namespace BankSystem.Models
{
    public class GetAccountBalance
    {
        public int? Id { get; set; }
        public string? AvailableBalance { get; set; }
        public string? Currency { get; set; }

        public int UserId { get; set; }
        public GetBasicUserInfo? User { get; set; }
    }
}
