using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddCanonicalAreaToArea : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanonicalArea",
                table: "areas",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CanonicalArea", table: "areas");
        }
    }
}
