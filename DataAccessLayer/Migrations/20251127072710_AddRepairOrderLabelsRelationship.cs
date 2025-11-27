using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairOrderLabelsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepairOrderLabels",
                columns: table => new
                {
                    RepairOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairOrderLabels", x => new { x.RepairOrderId, x.LabelId });
                    table.ForeignKey(
                        name: "FK_RepairOrderLabels_Labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Labels",
                        principalColumn: "LabelId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairOrderLabels_RepairOrders_RepairOrderId",
                        column: x => x.RepairOrderId,
                        principalTable: "RepairOrders",
                        principalColumn: "RepairOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderLabels_LabelId",
                table: "RepairOrderLabels",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_RepairOrderLabels_RepairOrderId",
                table: "RepairOrderLabels",
                column: "RepairOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepairOrderLabels");
        }
    }
}
