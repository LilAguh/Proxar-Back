using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeBudgetIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_ClientId",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ClientId_CompanyId",
                table: "Budgets",
                columns: new[] { "ClientId", "CompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TicketId_CompanyId",
                table: "Budgets",
                columns: new[] { "TicketId", "CompanyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_ClientId_CompanyId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TicketId_CompanyId",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_ClientId",
                table: "Budgets",
                column: "ClientId");
        }
    }
}
