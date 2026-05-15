using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produto_template",
                columns: table => new
                {
                    produto_template_id = table.Column<string>(maxLength: 64, nullable: false),
                    aplicacao_id = table.Column<string>(maxLength: 64, nullable: false),
                    segmento_tenant_id = table.Column<string>(maxLength: 64, nullable: true),
                    nome = table.Column<string>(maxLength: 200, nullable: false),
                    descricao = table.Column<string>(nullable: true),
                    conteudo_json = table.Column<string>(nullable: false),
                    data_criacao = table.Column<DateTime>(nullable: false),
                    ativo = table.Column<bool>(nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_template", x => x.produto_template_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_produto_template_aplicacao_id",
                table: "produto_template",
                column: "aplicacao_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "produto_template");
        }
    }
}
