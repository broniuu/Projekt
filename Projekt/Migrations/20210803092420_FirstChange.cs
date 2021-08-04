using Microsoft.EntityFrameworkCore.Migrations;

namespace Projekt.Migrations
{
    public partial class FirstChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.CreateTable(
                name: "DishDatas",
                columns: table => new
                {
                    DishDataId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    Availability = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishDatas", x => x.DishDataId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishDatas");

            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    DishId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Availability = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.DishId);
                });
        }
    }
}
