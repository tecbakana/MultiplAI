using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    public partial class AddSegmentoTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "segmento_tenant",
                columns: table => new
                {
                    segmento_tenant_id = table.Column<string>(maxLength: 64, nullable: false),
                    nome = table.Column<string>(maxLength: 100, nullable: false),
                    descricao = table.Column<string>(nullable: true),
                    ativo = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segmento_tenant", x => x.segmento_tenant_id);
                });

            migrationBuilder.CreateTable(
                name: "aplicacao_segmento",
                columns: table => new
                {
                    aplicacao_id = table.Column<string>(maxLength: 64, nullable: false),
                    segmento_tenant_id = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aplicacao_segmento", x => new { x.aplicacao_id, x.segmento_tenant_id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_produto_template_segmento_tenant_id",
                table: "produto_template",
                column: "segmento_tenant_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_produto_template_segmento_tenant_id",
                table: "produto_template");

            migrationBuilder.DropTable(name: "aplicacao_segmento");
            migrationBuilder.DropTable(name: "segmento_tenant");
        }
    }
}
