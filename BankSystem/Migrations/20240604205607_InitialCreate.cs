using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace BankSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BasicUserInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    GivenName = table.Column<string>(type: "longtext", nullable: true),
                    FamilyName = table.Column<string>(type: "longtext", nullable: true),
                    Birthdate = table.Column<string>(type: "longtext", nullable: true),
                    Locale = table.Column<string>(type: "longtext", nullable: true),
                    Gender = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasicUserInfos", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CreatePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ExternalTransactionId = table.Column<string>(type: "longtext", nullable: true),
                    Money_Amount = table.Column<string>(type: "longtext", nullable: true),
                    Money_Currency = table.Column<string>(type: "longtext", nullable: true),
                    CustomerReference = table.Column<string>(type: "longtext", nullable: true),
                    ServiceProviderUserName = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreatePayments", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AvailableBalance = table.Column<string>(type: "longtext", nullable: true),
                    Currency = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalances_BasicUserInfos_UserId",
                        column: x => x.UserId,
                        principalTable: "BasicUserInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountBalancesInSpecificCurrency",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AvailableBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountBalancesInSpecificCurrency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountBalancesInSpecificCurrency_BasicUserInfos_UserId",
                        column: x => x.UserId,
                        principalTable: "BasicUserInfos",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    XReferenceId = table.Column<string>(type: "longtext", nullable: true),
                    CreatePaymentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentStatuses_CreatePayments_CreatePaymentId",
                        column: x => x.CreatePaymentId,
                        principalTable: "CreatePayments",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RequestToPays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<string>(type: "longtext", nullable: true),
                    Currency = table.Column<string>(type: "longtext", nullable: true),
                    ExternalId = table.Column<string>(type: "longtext", nullable: true),
                    Payer_PartyIdType = table.Column<string>(type: "longtext", nullable: true),
                    Payer_PartyId = table.Column<string>(type: "longtext", nullable: true),
                    PayerMessage = table.Column<string>(type: "longtext", nullable: true),
                    PayeeNote = table.Column<string>(type: "longtext", nullable: true),
                    CreatePaymentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestToPays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestToPays_CreatePayments_CreatePaymentId",
                        column: x => x.CreatePaymentId,
                        principalTable: "CreatePayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalances_UserId",
                table: "AccountBalances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountBalancesInSpecificCurrency_UserId",
                table: "AccountBalancesInSpecificCurrency",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_CreatePaymentId",
                table: "PaymentStatuses",
                column: "CreatePaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestToPays_CreatePaymentId",
                table: "RequestToPays",
                column: "CreatePaymentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountBalances");

            migrationBuilder.DropTable(
                name: "AccountBalancesInSpecificCurrency");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "RequestToPays");

            migrationBuilder.DropTable(
                name: "BasicUserInfos");

            migrationBuilder.DropTable(
                name: "CreatePayments");
        }
    }
}
