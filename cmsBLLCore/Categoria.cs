using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Categoria
    {
        #region PROPRIEDADES
        public Guid CategoriaId { get; set; }
        public Guid AplicacaoId { get; set; }
        public Guid ?CategoriaIdPai { get; set; }
        public string? Nome { get; set; }
        public string? NomePai { get; set; }
        public string? Descricao { get; set; }
        public int TipoCategoria { get; set; }
        #endregion

        #region METODOS
        public static Categoria ObterNovaCategoria()
        {
            return new Categoria();
        }

        #endregion
    }
}
