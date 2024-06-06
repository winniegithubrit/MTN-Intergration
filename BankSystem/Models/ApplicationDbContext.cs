using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
  public DbSet<User> Users { get; set; }
  public DbSet<Deposit> Deposits { get; set; }
  public DbSet<Refund> Refunds { get; set; }
  public DbSet<Transfer> Transfers { get; set; }
  public DbSet<CreateInvoiceModel> Invoices { get; set; }

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

    modelBuilder.Entity<CreateInvoiceModel>(entity =>
    {
      entity.OwnsOne(i => i.IntendedPayer);
      entity.OwnsOne(i => i.Payee);
    });

    // disbursement relationships
    modelBuilder.Entity<Deposit>()
        .HasOne(d => d.User)
        .WithMany(u => u.Deposits)
        .HasForeignKey(d => d.UserId);

    modelBuilder.Entity<Refund>()
        .HasOne(r => r.User)
        .WithMany(u => u.Refunds)
        .HasForeignKey(r => r.UserId);

    modelBuilder.Entity<Transfer>()
        .HasOne(t => t.User)
        .WithMany(u => u.Transfers)
        .HasForeignKey(t => t.UserId);

    modelBuilder.Entity<Deposit>().HasKey(d => d.DepositId);
    modelBuilder.Entity<Refund>().HasKey(r => r.RefundId);
    modelBuilder.Entity<Transfer>().HasKey(t => t.TransferId);

    modelBuilder.Entity<GetBasicUserInfo>().HasData(
            new GetBasicUserInfo
            {
              Id = 1,
              GivenName = "John",
              FamilyName = "Wilson",
              Birthdate = "1980-01-01",
              Locale = "en-US",
              Gender = "Male",
              Status = "Active"
            },
            new GetBasicUserInfo
            {
              Id = 2,
              GivenName = "Jane",
              FamilyName = "Smith",
              Birthdate = "1985-05-15",
              Locale = "en-GB",
              Gender = "Female",
              Status = "Active"
            },
            new GetBasicUserInfo
            {
              Id = 3,
              GivenName = "Bob",
              FamilyName = "Johnson",
              Birthdate = "1990-11-20",
              Locale = "en-AU",
              Gender = "Male",
              Status = "Inactive"
            }
        );

  }
}
