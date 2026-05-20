using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMSXData.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoMaoDeObra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produto_mao_de_obra",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    produtoid = table.Column<string>(maxLength: 64, nullable: false),
                    tipo = table.Column<string>(maxLength: 30, nullable: false),
                    descricao = table.Column<string>(maxLength: 200, nullable: false),
                    capacidade_dia = table.Column<int>(nullable: true),
                    valor_dia = table.Column<decimal>(precision: 12, scale: 2, nullable: true),
                    valor_milheiro = table.Column<decimal>(precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_mao_de_obra", x => x.id);
                    table.ForeignKey(
                        name: "FK_ProdutoMaoDeObra_Produto",
                        column: x => x.produtoid,
                        principalTable: "produto",
                        principalColumn: "ProdutoId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_produto_mao_de_obra_produtoid",
                table: "produto_mao_de_obra",
                column: "produtoid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "produto_mao_de_obra");
        }
    }
}
