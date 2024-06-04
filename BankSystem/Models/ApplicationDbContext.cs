using Microsoft.EntityFrameworkCore;
using BankSystem.Models;

public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<CreatePayment> CreatePayments { get; set; }
  public DbSet<GetPaymentStatus> PaymentStatuses { get; set; }
  public DbSet<RequestToPay> RequestToPays { get; set; }
  public DbSet<GetAccountBalance> AccountBalances { get; set; }
  public DbSet<GetAccountBalanceInSpecificCurrency> AccountBalancesInSpecificCurrency { get; set; }
  public DbSet<GetBasicUserInfo> BasicUserInfos { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<CreatePayment>(entity =>
    {
      entity.OwnsOne(p => p.Money);
    });

    modelBuilder.Entity<RequestToPay>(entity =>
    {
      entity.OwnsOne(p => p.Payer);
    });

    modelBuilder.Entity<GetAccountBalance>()
        .HasOne(b => b.User)
        .WithMany(u => u.AccountBalances)
        .HasForeignKey(b => b.UserId);

    modelBuilder.Entity<GetAccountBalanceInSpecificCurrency>()
        .HasOne(b => b.User)
        .WithMany(u => u.AccountBalancesInSpecificCurrency)
        .HasForeignKey(b => b.UserId);
  }
}
