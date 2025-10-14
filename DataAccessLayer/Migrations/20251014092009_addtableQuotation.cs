using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessLayer.Migrations
{
    /// <inheritdoc />
    public partial class addtableQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotation_AspNetUsers_ApprovedBy",
                table: "Quotation");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotation_Branches_BranchID",
                table: "Quotation");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotation_RepairRequests_RepairRequestID",
                table: "Quotation");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItem_Parts_PartID",
                table: "QuotationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItem_Quotation_QuotationID",
                table: "QuotationItem");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItem_Services_ServiceID",
                table: "QuotationItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuotationItem",
                table: "QuotationItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Quotation",
                table: "Quotation");

            migrationBuilder.RenameTable(
                name: "QuotationItem",
                newName: "QuotationItems");

            migrationBuilder.RenameTable(
                name: "Quotation",
                newName: "Quotations");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationItem_ServiceID",
                table: "QuotationItems",
                newName: "IX_QuotationItems_ServiceID");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationItem_QuotationID",
                table: "QuotationItems",
                newName: "IX_QuotationItems_QuotationID");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationItem_PartID",
                table: "QuotationItems",
                newName: "IX_QuotationItems_PartID");

            migrationBuilder.RenameIndex(
                name: "IX_Quotation_RepairRequestID",
                table: "Quotations",
                newName: "IX_Quotations_RepairRequestID");

            migrationBuilder.RenameIndex(
                name: "IX_Quotation_BranchID",
                table: "Quotations",
                newName: "IX_Quotations_BranchID");

            migrationBuilder.RenameIndex(
                name: "IX_Quotation_ApprovedBy",
                table: "Quotations",
                newName: "IX_Quotations_ApprovedBy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuotationItems",
                table: "QuotationItems",
                column: "QuotationItemID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Quotations",
                table: "Quotations",
                column: "QuotationID");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Parts_PartID",
                table: "QuotationItems",
                column: "PartID",
                principalTable: "Parts",
                principalColumn: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Quotations_QuotationID",
                table: "QuotationItems",
                column: "QuotationID",
                principalTable: "Quotations",
                principalColumn: "QuotationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItems_Services_ServiceID",
                table: "QuotationItems",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_AspNetUsers_ApprovedBy",
                table: "Quotations",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_Branches_BranchID",
                table: "Quotations",
                column: "BranchID",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotations_RepairRequests_RepairRequestID",
                table: "Quotations",
                column: "RepairRequestID",
                principalTable: "RepairRequests",
                principalColumn: "RepairRequestID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Parts_PartID",
                table: "QuotationItems");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Quotations_QuotationID",
                table: "QuotationItems");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationItems_Services_ServiceID",
                table: "QuotationItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_AspNetUsers_ApprovedBy",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_Branches_BranchID",
                table: "Quotations");

            migrationBuilder.DropForeignKey(
                name: "FK_Quotations_RepairRequests_RepairRequestID",
                table: "Quotations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Quotations",
                table: "Quotations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuotationItems",
                table: "QuotationItems");

            migrationBuilder.RenameTable(
                name: "Quotations",
                newName: "Quotation");

            migrationBuilder.RenameTable(
                name: "QuotationItems",
                newName: "QuotationItem");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_RepairRequestID",
                table: "Quotation",
                newName: "IX_Quotation_RepairRequestID");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_BranchID",
                table: "Quotation",
                newName: "IX_Quotation_BranchID");

            migrationBuilder.RenameIndex(
                name: "IX_Quotations_ApprovedBy",
                table: "Quotation",
                newName: "IX_Quotation_ApprovedBy");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationItems_ServiceID",
                table: "QuotationItem",
                newName: "IX_QuotationItem_ServiceID");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationItems_QuotationID",
                table: "QuotationItem",
                newName: "IX_QuotationItem_QuotationID");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationItems_PartID",
                table: "QuotationItem",
                newName: "IX_QuotationItem_PartID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Quotation",
                table: "Quotation",
                column: "QuotationID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuotationItem",
                table: "QuotationItem",
                column: "QuotationItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotation_AspNetUsers_ApprovedBy",
                table: "Quotation",
                column: "ApprovedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotation_Branches_BranchID",
                table: "Quotation",
                column: "BranchID",
                principalTable: "Branches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quotation_RepairRequests_RepairRequestID",
                table: "Quotation",
                column: "RepairRequestID",
                principalTable: "RepairRequests",
                principalColumn: "RepairRequestID");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItem_Parts_PartID",
                table: "QuotationItem",
                column: "PartID",
                principalTable: "Parts",
                principalColumn: "PartId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItem_Quotation_QuotationID",
                table: "QuotationItem",
                column: "QuotationID",
                principalTable: "Quotation",
                principalColumn: "QuotationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationItem_Services_ServiceID",
                table: "QuotationItem",
                column: "ServiceID",
                principalTable: "Services",
                principalColumn: "ServiceId");
        }
    }
}
