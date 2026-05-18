using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddLogoContentTypeToAplicacao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "aplicacao",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "LogoContentType", table: "aplicacao");
        }
    }
}
