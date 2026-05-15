using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class ExpandAtributoFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "atributo",
                maxLength: 255,
                nullable: false,
                oldMaxLength: 45);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "atributo",
                maxLength: 500,
                nullable: false,
                oldMaxLength: 45);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "atributo",
                maxLength: 45,
                nullable: false,
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "atributo",
                maxLength: 45,
                nullable: false,
                oldMaxLength: 500);
        }
    }
}
