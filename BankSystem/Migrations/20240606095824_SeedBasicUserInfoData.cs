using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BankSystem.Migrations
{
    /// <inheritdoc />
    public partial class SeedBasicUserInfoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BasicUserInfos",
                columns: new[] { "Id", "Birthdate", "FamilyName", "Gender", "GivenName", "Locale", "Status" },
                values: new object[,]
                {
                    { 1, "1985-05-15", "Wambui", "Male", "Charles", "USA", "Active" },
                    { 2, "1990-08-20", "Smith", "Female", "Jane", "GERM", "Inactive" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BasicUserInfos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BasicUserInfos",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
