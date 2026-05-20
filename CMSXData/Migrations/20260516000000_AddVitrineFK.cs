using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddVitrineFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_vitrine_configurada_vitrine_template_id",
                table: "vitrine_configurada",
                column: "vitrine_template_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vitrine_configurada_vitrine_template",
                table: "vitrine_configurada",
                column: "vitrine_template_id",
                principalTable: "vitrine_template",
                principalColumn: "vitrine_template_id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vitrine_configurada_vitrine_template",
                table: "vitrine_configurada");

            migrationBuilder.DropIndex(
                name: "IX_vitrine_configurada_vitrine_template_id",
                table: "vitrine_configurada");
        }
    }
}
