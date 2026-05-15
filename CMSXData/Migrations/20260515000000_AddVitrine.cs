using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddVitrine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vitrine_template",
                columns: table => new
                {
                    vitrine_template_id = table.Column<Guid>(nullable: false),
                    nome = table.Column<string>(maxLength: 200, nullable: false),
                    descricao = table.Column<string>(nullable: true),
                    segmento_tenant_id = table.Column<string>(maxLength: 64, nullable: true),
                    html_css = table.Column<string>(nullable: false),
                    variaveis_json = table.Column<string>(nullable: false),
                    thumbnail_url = table.Column<string>(maxLength: 500, nullable: true),
                    data_criacao = table.Column<DateTime>(nullable: false),
                    ativo = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vitrine_template", x => x.vitrine_template_id);
                });

            migrationBuilder.CreateTable(
                name: "vitrine_configurada",
                columns: table => new
                {
                    vitrine_configurada_id = table.Column<Guid>(nullable: false),
                    aplicacao_id = table.Column<string>(maxLength: 64, nullable: false),
                    vitrine_template_id = table.Column<Guid>(nullable: false),
                    valores_json = table.Column<string>(nullable: false),
                    publicado = table.Column<bool>(nullable: false, defaultValue: false),
                    data_atualizacao = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vitrine_configurada", x => x.vitrine_configurada_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_vitrine_configurada_aplicacao_id",
                table: "vitrine_configurada",
                column: "aplicacao_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vitrine_template_segmento_tenant_id",
                table: "vitrine_template",
                column: "segmento_tenant_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "vitrine_configurada");
            migrationBuilder.DropTable(name: "vitrine_template");
        }
    }
}
