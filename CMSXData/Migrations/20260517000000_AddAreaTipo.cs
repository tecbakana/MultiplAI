using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddAreaTipo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "areas",
                maxLength: 20,
                nullable: false,
                defaultValue: "pagina");

            // data migration: áreas cujo nome sugere home recebem Tipo = 'home'
            migrationBuilder.Sql(
                "UPDATE areas SET Tipo = 'home' WHERE LOWER(Nome) IN ('home', 'index', 'principal')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Tipo", table: "areas");
        }
    }
}
