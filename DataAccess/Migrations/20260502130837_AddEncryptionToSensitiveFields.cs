using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptionToSensitiveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Increase column sizes to accommodate encrypted data (Base64 encoding increases size ~1.33x + 16 bytes IV)

            // Company.CertPassword: 500 → 1000 chars
            migrationBuilder.AlterColumn<string>(
                name: "CertPassword",
                table: "Companies",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            // Subscription.MercadoPagoCardToken: 255 → 500 chars
            migrationBuilder.AlterColumn<string>(
                name: "MercadoPagoCardToken",
                table: "Subscriptions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            // Subscription.MercadoPagoPreapprovalId: 100 → 300 chars
            migrationBuilder.AlterColumn<string>(
                name: "MercadoPagoPreapprovalId",
                table: "Subscriptions",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            // Subscription.MercadoPagoCustomerId: 100 → 300 chars
            migrationBuilder.AlterColumn<string>(
                name: "MercadoPagoCustomerId",
                table: "Subscriptions",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert column sizes to original values

            migrationBuilder.AlterColumn<string>(
                name: "CertPassword",
                table: "Companies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MercadoPagoCardToken",
                table: "Subscriptions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MercadoPagoPreapprovalId",
                table: "Subscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MercadoPagoCustomerId",
                table: "Subscriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);
        }
    }
}
