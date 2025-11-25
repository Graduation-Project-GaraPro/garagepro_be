using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class removeServicePart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceParts_Parts_PartId",
                table: "ServiceParts");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceParts_Services_ServiceId",
                table: "ServiceParts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceParts",
                table: "ServiceParts");

            migrationBuilder.RenameTable(
                name: "ServiceParts",
                newName: "ServicePart");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceParts_PartId",
                table: "ServicePart",
                newName: "IX_ServicePart_PartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicePart",
                table: "ServicePart",
                column: "ServicePartId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePart_ServiceId",
                table: "ServicePart",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePart_Parts_PartId",
                table: "ServicePart",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "PartId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServicePart_Services_ServiceId",
                table: "ServicePart",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServicePart_Parts_PartId",
                table: "ServicePart");

            migrationBuilder.DropForeignKey(
                name: "FK_ServicePart_Services_ServiceId",
                table: "ServicePart");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicePart",
                table: "ServicePart");

            migrationBuilder.DropIndex(
                name: "IX_ServicePart_ServiceId",
                table: "ServicePart");

            migrationBuilder.RenameTable(
                name: "ServicePart",
                newName: "ServiceParts");

            migrationBuilder.RenameIndex(
                name: "IX_ServicePart_PartId",
                table: "ServiceParts",
                newName: "IX_ServiceParts_PartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceParts",
                table: "ServiceParts",
                columns: new[] { "ServiceId", "PartId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceParts_Parts_PartId",
                table: "ServiceParts",
                column: "PartId",
                principalTable: "Parts",
                principalColumn: "PartId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceParts_Services_ServiceId",
                table: "ServiceParts",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "ServiceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
