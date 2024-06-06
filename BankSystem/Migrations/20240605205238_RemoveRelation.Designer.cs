﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BankSystem.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240605205238_RemoveRelation")]
    partial class RemoveRelation
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("BankSystem.Models.CreatePayment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CustomerReference")
                        .HasColumnType("longtext");

                    b.Property<string>("ExternalTransactionId")
                        .HasColumnType("longtext");

                    b.Property<string>("ServiceProviderUserName")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("CreatePayments");
                });

            modelBuilder.Entity("BankSystem.Models.GetAccountBalance", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int?>("Id"));

                    b.Property<string>("AvailableBalance")
                        .HasColumnType("longtext");

                    b.Property<string>("Currency")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AccountBalances");
                });

            modelBuilder.Entity("BankSystem.Models.GetAccountBalanceInSpecificCurrency", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int?>("Id"));

                    b.Property<decimal>("AvailableBalance")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("Currency")
                        .HasColumnType("longtext");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AccountBalancesInSpecificCurrency");
                });

            modelBuilder.Entity("BankSystem.Models.GetBasicUserInfo", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int?>("Id"));

                    b.Property<string>("Birthdate")
                        .HasColumnType("longtext");

                    b.Property<string>("FamilyName")
                        .HasColumnType("longtext");

                    b.Property<string>("Gender")
                        .HasColumnType("longtext");

                    b.Property<string>("GivenName")
                        .HasColumnType("longtext");

                    b.Property<string>("Locale")
                        .HasColumnType("longtext");

                    b.Property<string>("Status")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("BasicUserInfos");
                });

            modelBuilder.Entity("BankSystem.Models.GetPaymentStatus", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int?>("Id"));

                    b.Property<int?>("CreatePaymentId")
                        .HasColumnType("int");

                    b.Property<string>("XReferenceId")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("CreatePaymentId")
                        .IsUnique();

                    b.ToTable("PaymentStatuses");
                });

            modelBuilder.Entity("BankSystem.Models.RequestToPay", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Amount")
                        .HasColumnType("longtext");

                    b.Property<string>("Currency")
                        .HasColumnType("longtext");

                    b.Property<string>("ExternalId")
                        .HasColumnType("longtext");

                    b.Property<string>("PayeeNote")
                        .HasColumnType("longtext");

                    b.Property<string>("PayerMessage")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("RequestToPays");
                });

            modelBuilder.Entity("BankSystem.Models.CreatePayment", b =>
                {
                    b.OwnsOne("BankSystem.Models.Money", "Money", b1 =>
                        {
                            b1.Property<int>("CreatePaymentId")
                                .HasColumnType("int");

                            b1.Property<string>("Amount")
                                .HasColumnType("longtext");

                            b1.Property<string>("Currency")
                                .HasColumnType("longtext");

                            b1.HasKey("CreatePaymentId");

                            b1.ToTable("CreatePayments");

                            b1.WithOwner()
                                .HasForeignKey("CreatePaymentId");
                        });

                    b.Navigation("Money");
                });

            modelBuilder.Entity("BankSystem.Models.GetAccountBalance", b =>
                {
                    b.HasOne("BankSystem.Models.GetBasicUserInfo", "User")
                        .WithMany("AccountBalances")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("BankSystem.Models.GetAccountBalanceInSpecificCurrency", b =>
                {
                    b.HasOne("BankSystem.Models.GetBasicUserInfo", "User")
                        .WithMany("AccountBalancesInSpecificCurrency")
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("BankSystem.Models.GetPaymentStatus", b =>
                {
                    b.HasOne("BankSystem.Models.CreatePayment", "CreatePayment")
                        .WithOne("PaymentStatus")
                        .HasForeignKey("BankSystem.Models.GetPaymentStatus", "CreatePaymentId");

                    b.Navigation("CreatePayment");
                });

            modelBuilder.Entity("BankSystem.Models.RequestToPay", b =>
                {
                    b.OwnsOne("BankSystem.Models.Payer", "Payer", b1 =>
                        {
                            b1.Property<int>("RequestToPayId")
                                .HasColumnType("int");

                            b1.Property<string>("PartyId")
                                .HasColumnType("longtext");

                            b1.Property<string>("PartyIdType")
                                .HasColumnType("longtext");

                            b1.HasKey("RequestToPayId");

                            b1.ToTable("RequestToPays");

                            b1.WithOwner()
                                .HasForeignKey("RequestToPayId");
                        });

                    b.Navigation("Payer");
                });

            modelBuilder.Entity("BankSystem.Models.CreatePayment", b =>
                {
                    b.Navigation("PaymentStatus");
                });

            modelBuilder.Entity("BankSystem.Models.GetBasicUserInfo", b =>
                {
                    b.Navigation("AccountBalances");

                    b.Navigation("AccountBalancesInSpecificCurrency");
                });
#pragma warning restore 612, 618
        }
    }
}