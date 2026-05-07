using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSXBLL;

namespace CMSXBLL
{
    public class Conteudo
    {
        #region PROPRIEDADES

        public string? Autor { get; set; }
        public string? Titulo { get; set; }
        public string? Texto { get; set; }
        public string? UrlImg { get; set; }
        public Guid ConteudoId { get; set; }
        public Guid AreaId { get; set; }
        public Guid CategoriaId { get; set; }
        public string? UnidadeId { get; set; }
        public string? Valor { get; set; }

        #endregion

        #region METODOS

        public static Conteudo ObtemNovoConteudo()
        {
            return new Conteudo();
        }

        #endregion
    }
}
