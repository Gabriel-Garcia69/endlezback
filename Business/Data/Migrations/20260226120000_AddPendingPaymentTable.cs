using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Business.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingPayment",
                columns: table => new
                {
                    Id               = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newid()"),
                    UserId           = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Total            = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemsJson        = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingFirstName  = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingLastName   = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingAddress    = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingApartment  = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingCity       = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingState      = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShippingPhone      = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt          = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingPayment", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PendingPayment");
        }
    }
}
