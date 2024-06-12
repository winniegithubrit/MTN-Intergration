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
  public DbSet<Deposit> Deposits { get; set; }
  public DbSet<CreateInvoiceModel> Invoices { get; set; }
  public DbSet<BasicUserInfoResponse> BasicUserInfomation { get; set; }
  public DbSet<Refund> Refunds { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<CreatePayment>(entity =>
    {
      entity.OwnsOne(p => p.Money, m =>
          {
          m.Property(p => p.Amount).IsRequired();
          m.Property(p => p.Currency).IsRequired();
        });
    });

    modelBuilder.Entity<RequestToPay>(entity =>
    {
      entity.OwnsOne(p => p.Payer, p =>
          {
          p.Property(p => p.PartyIdType).IsRequired();
          p.Property(p => p.PartyId).IsRequired();
        });

      entity.HasMany(rtp => rtp.Refunds)
              .WithOne(r => r.RequestToPay)
              .HasForeignKey(r => r.ReferenceIdToRefund);
    });

    modelBuilder.Entity<CreateInvoiceModel>(entity =>
    {
      entity.HasIndex(i => i.ExternalId).IsUnique();

      entity.OwnsOne(i => i.IntendedPayer, ip =>
          {
          ip.Property(p => p.PartyIdType).IsRequired();
          ip.Property(p => p.PartyId).IsRequired();
        });

      entity.OwnsOne(i => i.Payee, p =>
          {
          p.Property(p => p.PartyIdType).IsRequired();
          p.Property(p => p.PartyId).IsRequired();
        });
    });

    modelBuilder.Entity<Deposit>(entity =>
    {
      entity.HasKey(d => d.DepositId);

      entity.OwnsOne(d => d.Payee, p =>
          {
          p.Property(p => p.PartyIdType).IsRequired();
          p.Property(p => p.PartyId).IsRequired();
        });
    });

    modelBuilder.Entity<GetPaymentStatus>()
        .HasOne(g => g.CreatePayment)
        .WithOne(c => c.PaymentStatus)
        .HasForeignKey<GetPaymentStatus>(g => g.CreatePaymentId);


    modelBuilder.Entity<RequestToPay>(entity =>
        {
          entity.HasKey(r => r.Id).HasName("PK_RequestToPays");
          entity.Property(r => r.Id).HasColumnType("varchar(255)").IsRequired();
          entity.HasIndex(r => r.Id).IsUnique(); 

        
          entity.HasMany(rtp => rtp.Refunds)
                .WithOne(r => r.RequestToPay)
                .HasForeignKey(r => r.ReferenceIdToRefund);
        });
  }
}
