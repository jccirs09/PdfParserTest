using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickingListApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    AddressLine = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    Province = table.Column<string>(type: "TEXT", nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PickingLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesOrderNumber = table.Column<string>(type: "TEXT", nullable: false),
                    PrintDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PickingGroup = table.Column<string>(type: "TEXT", nullable: true),
                    Buyer = table.Column<string>(type: "TEXT", nullable: true),
                    ShipDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "TEXT", nullable: true),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    JobName = table.Column<string>(type: "TEXT", nullable: true),
                    SalesRep = table.Column<string>(type: "TEXT", nullable: true),
                    ShipVia = table.Column<string>(type: "TEXT", nullable: true),
                    SoldToId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShipToId = table.Column<int>(type: "INTEGER", nullable: false),
                    FobPoint = table.Column<string>(type: "TEXT", nullable: true),
                    Route = table.Column<string>(type: "TEXT", nullable: true),
                    Terms = table.Column<string>(type: "TEXT", nullable: true),
                    ReceivingHours = table.Column<string>(type: "TEXT", nullable: true),
                    CallBeforePhone = table.Column<string>(type: "TEXT", nullable: true),
                    TotalWeightLbs = table.Column<decimal>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingLists_Parties_ShipToId",
                        column: x => x.ShipToId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PickingLists_Parties_SoldToId",
                        column: x => x.SoldToId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PickingListItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PickingListId = table.Column<int>(type: "INTEGER", nullable: false),
                    LineNo = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantityUnit = table.Column<string>(type: "TEXT", nullable: false),
                    QuantityStaged = table.Column<int>(type: "INTEGER", nullable: true),
                    ItemCode = table.Column<string>(type: "TEXT", nullable: false),
                    WidthIn = table.Column<decimal>(type: "TEXT", nullable: true),
                    LengthIn = table.Column<decimal>(type: "TEXT", nullable: true),
                    WeightLbs = table.Column<decimal>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickingListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickingListItems_PickingLists_PickingListId",
                        column: x => x.PickingListId,
                        principalTable: "PickingLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemTagDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PickingListItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagNo = table.Column<string>(type: "TEXT", nullable: true),
                    HeatNo = table.Column<string>(type: "TEXT", nullable: true),
                    MillRef = table.Column<string>(type: "TEXT", nullable: true),
                    Qty = table.Column<int>(type: "INTEGER", nullable: true),
                    ThicknessIn = table.Column<decimal>(type: "TEXT", nullable: true),
                    Size = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTagDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemTagDetails_PickingListItems_PickingListItemId",
                        column: x => x.PickingListItemId,
                        principalTable: "PickingListItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTagDetails_PickingListItemId",
                table: "ItemTagDetails",
                column: "PickingListItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingListItems_PickingListId",
                table: "PickingListItems",
                column: "PickingListId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_SalesOrderNumber",
                table: "PickingLists",
                column: "SalesOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_ShipToId",
                table: "PickingLists",
                column: "ShipToId");

            migrationBuilder.CreateIndex(
                name: "IX_PickingLists_SoldToId",
                table: "PickingLists",
                column: "SoldToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemTagDetails");

            migrationBuilder.DropTable(
                name: "PickingListItems");

            migrationBuilder.DropTable(
                name: "PickingLists");

            migrationBuilder.DropTable(
                name: "Parties");
        }
    }
}
