using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Areas
    {
        #region PROPRIEDADES
        public Guid AreaId { get; set; }
        public Guid ?AreaIdPai { get; set; }
        public Guid AplicacaoId { get; set; }
        public string? Descricao { get; set; }
        public string? Url { get; set; }
        public string? Nome { get; set; }
        public string? NomePai { get; set; }
        public string? UrlImagem { get; set; }
        public bool Imagem { get; set; }
        public string? TipoMenu { get; set; }
        public bool MenuCentral { get; set; }
        public bool MenuSplash { get; set; }
        public bool MenuLateral { get; set; }
        public int Posicao { get; set; }
        public int IdTipoArea { get; set; }
        public string? NomeTipoArea { get; set; }
        public int TipoArea { get; set; }

        #endregion

        #region METODOS
        public static Areas ObterNovaArea()
        {
            return new Areas();
        }

        #endregion
    }
}
