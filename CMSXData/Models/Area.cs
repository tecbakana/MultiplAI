using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class Area
{
    public string Areaid { get; set; } = null!;

    public string? Aplicacaoid { get; set; }

    public string? Nome { get; set; }

    public string? Url { get; set; }

    public string? Descricao { get; set; }

    public string? Areaidpai { get; set; }

    public DateTime? Datainicial { get; set; }

    public DateTime? Datafinal { get; set; }

    public byte? Imagem { get; set; }

    public byte? Menulateral { get; set; }

    public byte? Menusplash { get; set; }

    public byte? Menucentral { get; set; }

    public int? Posicao { get; set; }

    public byte? Menufixo { get; set; }

    public byte? Listasimples { get; set; }

    public byte? Listasplash { get; set; }

    public byte? Listabanner { get; set; }

    public int? Tipoarea { get; set; }
    public string? Layout { get; set; }

    public string? PageBuilderVersion { get; set; }

    public string Tipo { get; set; } = "pagina"; // "home" | "pagina"

    public Guid? VitrineTemplateId { get; set; }
    public string? VitrineValoresJson { get; set; }
    public string? VitrineHtmlSnapshot { get; set; }
    public bool VitrinePublicado { get; set; } = false;
    public bool CanonicalArea { get; set; } = false;
}
