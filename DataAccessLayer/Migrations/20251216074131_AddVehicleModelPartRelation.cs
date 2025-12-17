using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleModelPartRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModelID",
                table: "Parts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Parts_ModelID",
                table: "Parts",
                column: "ModelID");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_VehicleModels_ModelID",
                table: "Parts",
                column: "ModelID",
                principalTable: "VehicleModels",
                principalColumn: "ModelID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_VehicleModels_ModelID",
                table: "Parts");

            migrationBuilder.DropIndex(
                name: "IX_Parts_ModelID",
                table: "Parts");

            migrationBuilder.DropColumn(
                name: "ModelID",
                table: "Parts");
        }
    }
}
