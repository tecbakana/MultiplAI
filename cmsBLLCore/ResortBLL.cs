using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class ResortBll
    {
        #region PROPRIEDADES
        public Guid ResortId { get; set; }
        public int idMkt { get; set; }
        public int idTabrot { get; set; }
        public string? Descricao { get; set; }
        public string? Titulo { get; set; }
        public string? Texto { get; set; }
        public string? Acomodacoes { get; set; }
        public string? Regime { get; set; }
        #endregion

        #region METODOS
        public static ResortBll ObterNovoResort()
        {
            return new ResortBll();
        }
        #endregion
    }
}
