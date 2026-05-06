using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Atributo
    {
        #region PROPRIEDADES
        public int AtributoId { get; set; }
        public Guid ProdutoId { get; set; }
        public string? Descricao { get; set; }
        public string? Nome { get; set; }

        #endregion

        #region METODOS
        public static Atributo ObterNovoAtributo()
        {
            return new Atributo();
        }

        #endregion
    }
}
