using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpenedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashRegisters_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashRegisters_Users_ClosedById",
                        column: x => x.ClosedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashRegisters_Users_OpenedById",
                        column: x => x.OpenedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashRegisterEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpeningAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ClosingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisterEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashRegisterEntries_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashRegisterEntries_CashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "CashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisterEntries_AccountId",
                table: "CashRegisterEntries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisterEntries_CashRegisterId_AccountId",
                table: "CashRegisterEntries",
                columns: new[] { "CashRegisterId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_ClosedById",
                table: "CashRegisters",
                column: "ClosedById");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_CompanyId_Date",
                table: "CashRegisters",
                columns: new[] { "CompanyId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_OpenedById",
                table: "CashRegisters",
                column: "OpenedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashRegisterEntries");

            migrationBuilder.DropTable(
                name: "CashRegisters");
        }
    }
}
