using Microsoft.EntityFrameworkCore.Migrations;

namespace Projekt.Migrations
{
    public partial class NameFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "date",
                table: "Orders",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "Orders",
                newName: "Amount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Orders",
                newName: "date");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Orders",
                newName: "amount");
        }
    }
}
