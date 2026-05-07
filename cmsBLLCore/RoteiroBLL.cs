using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class RoteiroBll
    {
        #region PROPERTIES
        public int IdRoteiro { get; set; }
        public int IdCidade { get; set; }
        public int IdFornecedor { get; set; }
        public int IdTabrot { get; set; }
        public int IdCidOrig { get; set; }
        public string? ChaveId { get; set; }
        public string? TextoRoteiro { get; set; }
        public string? TipoRoteiro { get; set; }
        public string? Fornecedor { get; set; }
        public string? Cidade { get; set; }
        public string? Imagem { get; set; }
        public string? CidadeOrigem { get; set; }

        #endregion

        public static RoteiroBll ObtemRoteiro()
        {
            return new RoteiroBll();
        }
    }
}
