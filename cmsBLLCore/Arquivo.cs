using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Arquivo
    {
        #region PROPRIEDADES
        public Guid ArquivoId { get; set; }
        public Guid AreaId { get; set; }
        public Guid ConteudoId { get; set; }
        public string? Nome { get; set; }
        public Guid TipoId { get; set; }
        #endregion

        #region METODOS

        public static Arquivo ObtemNovoArquivo()
        {
            return new Arquivo();
        }

        #endregion
    }
}
