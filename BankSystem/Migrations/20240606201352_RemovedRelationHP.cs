using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BankSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemovedRelationHP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Birthdate", "FamilyName", "Gender", "GivenName", "Locale", "Status" },
                values: new object[,]
                {
                    { 1, "1990-01-01", "Tom", "Male", "John", "en-US", "Active" },
                    { 2, "1992-05-15", "Smith", "Female", "Alice", "en-GB", "Active" },
                    { 3, "1985-11-20", "Johnson", "Male", "Bob", "en-AU", "Inactive" }
                });
        }
    }
}
