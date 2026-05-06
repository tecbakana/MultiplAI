using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSXBLL
{
    public class Unidade
    {
        public Guid UnidadeId { get; set; }
        public string? Nome { get; set; }
        public string? Sigla { get; set; }

        public static Unidade ObterNovaUnidade()
        {
            return new Unidade();
        }
    }
}
