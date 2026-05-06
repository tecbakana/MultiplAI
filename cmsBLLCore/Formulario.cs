using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSXBLL;
using ICMSX;

namespace CMSXBLL
{
    public class Formulario
    {
        public Guid Formularioid { get; set; }
        public Guid AreaId { get; set; }
        public string? Nome { get; set; }
        public string? AreaNome { get; set; }
        public string? Valor { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataInclusao { get; set; }

        public static Formulario ObtemNovoFormulario()
        {
            return new Formulario();
        }
    }
}
