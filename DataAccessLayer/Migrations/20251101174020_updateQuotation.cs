using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class updateQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecommended",
                table: "QuotationServiceParts");

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "QuotationServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomerNote",
                table: "Quotations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "QuotationServices");

            migrationBuilder.DropColumn(
                name: "CustomerNote",
                table: "Quotations");

            migrationBuilder.AddColumn<bool>(
                name: "IsRecommended",
                table: "QuotationServiceParts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
