using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Opcao
    {
        #region PROPRIEDADES
        public Guid AtributoId { get; set; }
        public Guid OpcaoId { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int Qtd { get; set; }
        public bool Estoque { get; set; }
        #endregion

        #region METODOS
        public static Opcao ObterNovaOpcao()
        {
            return new Opcao();
        }

        #endregion
    }
}
