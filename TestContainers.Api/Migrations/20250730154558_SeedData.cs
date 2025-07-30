using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestContainers.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Messages",
            columns: new[] { "Subject", "Content", "CreatedOn" },
            values: new object[,]
            {
                { "Hello", "This is a test message.", DateTime.UtcNow },
                { "Reminder", "Don't forget the meeting at 3 PM.", DateTime.UtcNow }
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
