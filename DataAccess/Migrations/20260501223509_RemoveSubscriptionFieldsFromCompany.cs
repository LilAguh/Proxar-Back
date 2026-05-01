using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubscriptionFieldsFromCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "CustomerGatewayToken",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "LastFourDigits",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "MonthlyFee",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "NextBillingDate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PaymentGateway",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "PaymentMethodToken",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionCancelledAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionCreatedAt",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlan",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "SubscriptionStatus",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "TrialEndsAt",
                table: "Companies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerGatewayToken",
                table: "Companies",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastFourDigits",
                table: "Companies",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyFee",
                table: "Companies",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextBillingDate",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentGateway",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodToken",
                table: "Companies",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionCancelledAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionCreatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlan",
                table: "Companies",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionStatus",
                table: "Companies",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndsAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
