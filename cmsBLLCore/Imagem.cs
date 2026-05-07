using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSXBLL;
using ICMSX;

namespace CMSXBLL
{
    public class Imagem
    {
        public Guid ImagemId { get; set; }
        public string? Url { get; set; }
        public int Altura { get; set; }
        public int Largura { get; set; }
        public Guid AreaId { get; set; }
        public Guid ConteudoId { get; set; }
        public Guid ParentId { get; set; }
        public Guid TipoId { get; set; }
        public String? Descricao { get; set; }

        public static Imagem ObterImagem()
        {
            return new Imagem();
        }
    }
}
