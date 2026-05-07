using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Produto
    {
        public Guid ProdutoId { get; set; }
        public Guid AplicacaoId { get; set; }
        public Guid CategoriaId { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public string? DescricaoCurta { get; set; }
        public string? DetalheTecnico { get; set; }
        public int Tipo { get; set; }
        public int Destaque { get; set; }
        public decimal Valor { get; set; }
        public string? Sku { get; set; }
        public string? PagSeguroBotao { get; set; }
        public List<Imagem>? galeria { get; set; }

        public static Produto ObterNovoProduto()
        {
            return new Produto();
        }
    }
}
