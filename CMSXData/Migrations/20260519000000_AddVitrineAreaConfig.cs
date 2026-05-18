using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddVitrineAreaConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VitrineTemplateId",
                table: "areas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitrineValoresJson",
                table: "areas",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitrineHtmlSnapshot",
                table: "areas",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VitrinePublicado",
                table: "areas",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "VitrinePublicado", table: "areas");
            migrationBuilder.DropColumn(name: "VitrineHtmlSnapshot", table: "areas");
            migrationBuilder.DropColumn(name: "VitrineValoresJson", table: "areas");
            migrationBuilder.DropColumn(name: "VitrineTemplateId", table: "areas");
        }
    }
}
