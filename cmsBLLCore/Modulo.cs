using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace CMSXBLL
{
    public class Modulo
    {
        public Guid ModuloId { get; set; }
        public string? Nome { get; set; }
        public string? Url { get; set; }
        public int RelacaoUsuario { get; set; }

        public static Modulo ObterNovoModulo()
        {
            return new Modulo();
        }
    }
}
