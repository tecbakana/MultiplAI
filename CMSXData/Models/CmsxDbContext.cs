using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CMSXData.Models;

public partial class CmsxDbContext : DbContext
{
    public CmsxDbContext()
    {
    }

    public CmsxDbContext(DbContextOptions<CmsxDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Aplicacao> Aplicacaos { get; set; }
    public virtual DbSet<Area> Areas { get; set; }
    public virtual DbSet<DictBloco> DictBlocos { get; set; }
    public virtual DbSet<IaConfig> IaConfigs { get; set; }
    public virtual DbSet<IaCache> IaCaches { get; set; }
    public virtual DbSet<IaUso> IaUsos { get; set; }
    public virtual DbSet<Arquivo> Arquivos { get; set; }
    public virtual DbSet<Atributo> Atributos { get; set; }
    public virtual DbSet<Cambio> Cambios { get; set; }
    public virtual DbSet<Caterium> Cateria { get; set; }
    public virtual DbSet<Ciaaerea> Ciaaereas { get; set; }
    public virtual DbSet<Conteudo> Conteudos { get; set; }
    public virtual DbSet<Conteudovalor> Conteudovalors { get; set; }
    public virtual DbSet<DictSocialmedium> DictSocialmedia { get; set; }
    public virtual DbSet<DictTemplate> DictTemplates { get; set; }
    public virtual DbSet<Dictarea> Dictareas { get; set; }
    public virtual DbSet<Faq> Faqs { get; set; }
    public virtual DbSet<Formulario> Formularios { get; set; }
    public virtual DbSet<Formularionew> Formularionews { get; set; }
    public virtual DbSet<Imagem> Imagems { get; set; }
    public virtual DbSet<Infofoto> Infofotos { get; set; }
    public virtual DbSet<Informativo> Informativos { get; set; }
    public virtual DbSet<Modulo> Modulos { get; set; }
    public virtual DbSet<Moduloconf> Moduloconfs { get; set; }
    public virtual DbSet<Moedum> Moeda { get; set; }
    public virtual DbSet<Newsletter> Newsletters { get; set; }
    public virtual DbSet<Opcao> Opcaos { get; set; }
    public virtual DbSet<Produto> Produtos { get; set; }
    public virtual DbSet<Refatributoxopcao> Refatributoxopcaos { get; set; }
    public virtual DbSet<Relatributoxproduto> Relatributoxprodutos { get; set; }
    public virtual DbSet<Relimagemconteudo> Relimagemconteudos { get; set; }
    public virtual DbSet<Relmoduloaplicacao> Relmoduloaplicacaos { get; set; }
    public virtual DbSet<Relmoduloconfaplicacao> Relmoduloconfaplicacaos { get; set; }
    public virtual DbSet<Relmodulousuario> Relmodulousuarios { get; set; }
    public virtual DbSet<Relusuarioaplicacao> Relusuarioaplicacaos { get; set; }
    public virtual DbSet<Grupo> Grupos { get; set; }
    public virtual DbSet<Relusuariogrupo> Relusuariogrupos { get; set; }
    public virtual DbSet<Socialmedium> Socialmedia { get; set; }
    public virtual DbSet<Template> Templates { get; set; }
    public virtual DbSet<LayoutTemplate> LayoutTemplates { get; set; }
    public virtual DbSet<Tipocotacao> Tipocotacaos { get; set; }
    public virtual DbSet<Tipoenvio> Tipoenvios { get; set; }
    public virtual DbSet<Unidade> Unidades { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<Pedido> Pedidos { get; set; }
    public virtual DbSet<Statuspedido> Statuspedidos { get; set; }
    public virtual DbSet<OrcamentoCabecalho> OrcamentoCabecalhos { get; set; }
    public virtual DbSet<OrcamentoDetalhe> OrcamentoDetalhes { get; set; }
    public virtual DbSet<OrcamentoDetalheComposto> OrcamentoDetalheCompostos { get; set; }
    public virtual DbSet<Selecao> Selecaos { get; set; }
    public virtual DbSet<ModeloComposto> ModeloCompostos { get; set; }
    public virtual DbSet<ModeloSelecao> ModeloSelecaos { get; set; }
    public virtual DbSet<PublicToken> PublicTokens { get; set; }
    public virtual DbSet<ProdutoMaoDeObra> ProdutoMaoDeObras { get; set; }
    public virtual DbSet<ProdutoTemplate> ProdutoTemplates { get; set; }
    public virtual DbSet<SegmentoTenant> SegmentoTenants { get; set; }
    public virtual DbSet<AplicacaoSegmento> AplicacaoSegmentos { get; set; }
    public virtual DbSet<VitrineTemplate> VitrineTemplates { get; set; }
    public virtual DbSet<VitrineConfigurada> VitrineConfiguradas { get; set; }
    public virtual DbSet<ClienteLoja> ClienteLojas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Grupo>(entity =>
        {
            entity.HasKey(e => e.Grupoid).HasName("PK_grupo");
            entity.ToTable("grupo");
            entity.Property(e => e.Grupoid).HasColumnName("GrupoId");
            entity.Property(e => e.Nome).HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(255);
            entity.Property(e => e.Acessototal).HasColumnName("AcessoTotal");
        });

        modelBuilder.Entity<Relusuariogrupo>(entity =>
        {
            entity.HasKey(e => e.Relacaoid).HasName("PK_relusuariogrupo");
            entity.ToTable("relusuariogrupo");
            entity.Property(e => e.Relacaoid).HasColumnName("RelacaoId");
            entity.Property(e => e.Usuarioid).HasColumnName("UsuarioId");
            entity.Property(e => e.Grupoid).HasColumnName("GrupoId");
        });

        modelBuilder.Entity<Aplicacao>(entity =>
        {
            entity.HasKey(e => e.Aplicacaoid).HasName("PK_aplicacao");
            entity.ToTable("aplicacao");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Datafinal).HasColumnName("DataFinal");
            entity.Property(e => e.Datainicio).HasColumnName("DataInicio");
            entity.Property(e => e.Header).HasMaxLength(245).HasColumnName("header");
            entity.Property(e => e.Idusuariofim).HasMaxLength(36).HasColumnName("IdUsuarioFim");
            entity.Property(e => e.Idusuarioinicio).HasMaxLength(36).HasColumnName("IdUsuarioInicio");
            entity.Property(e => e.Isactive).HasColumnName("isActive");
            entity.Property(e => e.Issecure).HasColumnName("isSecure");
            entity.Property(e => e.Layoutchoose).HasMaxLength(150).HasColumnName("LayoutChoose");
            entity.Property(e => e.Logotipo).HasColumnName("logotipo");
            entity.Property(e => e.Mailpassword).HasMaxLength(45).HasColumnName("mailPassword");
            entity.Property(e => e.Mailport).HasColumnName("mailPort");
            entity.Property(e => e.Mailserver).HasMaxLength(80).HasColumnName("mailServer");
            entity.Property(e => e.Mailuser).HasMaxLength(150).HasColumnName("mailUser");
            entity.Property(e => e.Nome).HasMaxLength(400).HasColumnName("Nome");
            entity.Property(e => e.Ogleadsense).HasMaxLength(500).HasColumnName("googleAdSense");
            entity.Property(e => e.Pagefacebook).HasMaxLength(255).HasColumnName("pageFacebook");
            entity.Property(e => e.Pageflicker).HasMaxLength(255).HasColumnName("pageFlicker");
            entity.Property(e => e.Pageinstagram).HasMaxLength(255).HasColumnName("pageInstagram");
            entity.Property(e => e.Pagelinkedin).HasMaxLength(255).HasColumnName("pageLinkedin");
            entity.Property(e => e.Pagepinterest).HasMaxLength(255).HasColumnName("pagePinterest");
            entity.Property(e => e.Pagetwitter).HasMaxLength(255).HasColumnName("pageTwitter");
            entity.Property(e => e.Pagsegurotoken).HasMaxLength(120).HasColumnName("PagSeguroToken");
            entity.Property(e => e.Posicao).HasColumnName("Posicao");
            entity.Property(e => e.Url).HasMaxLength(20).HasColumnName("Url");
        });

        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(e => e.Areaid).HasName("PK_areas");
            entity.ToTable("areas");
            entity.Property(e => e.Areaid).HasMaxLength(64).HasColumnName("AreaId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Areaidpai).HasMaxLength(64).HasColumnName("AreaIdPai");
            entity.Property(e => e.Datafinal).HasColumnName("DataFinal");
            entity.Property(e => e.Datainicial).HasColumnName("DataInicial");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("Descricao");
            entity.Property(e => e.Imagem).HasColumnName("Imagem");
            entity.Property(e => e.Listabanner).HasColumnName("ListaBanner");
            entity.Property(e => e.Listasimples).HasColumnName("ListaSimples");
            entity.Property(e => e.Listasplash).HasColumnName("ListaSplash");
            entity.Property(e => e.Menucentral).HasColumnName("MenuCentral");
            entity.Property(e => e.Menufixo).HasColumnName("MenuFixo");
            entity.Property(e => e.Menulateral).HasColumnName("MenuLateral");
            entity.Property(e => e.Menusplash).HasColumnName("MenuSplash");
            entity.Property(e => e.Nome).HasMaxLength(80).HasColumnName("Nome");
            entity.Property(e => e.Posicao).HasColumnName("posicao");
            entity.Property(e => e.Tipoarea).HasColumnName("TipoArea");
            entity.Property(e => e.Url).HasMaxLength(300).HasColumnName("Url");
            entity.Property(e => e.Layout).HasColumnName("layout");
            entity.Property(e => e.Tipo).HasMaxLength(20).HasColumnName("Tipo").HasDefaultValue("pagina");
        });

        modelBuilder.Entity<LayoutTemplate>(entity =>
        {
            entity.HasKey(e => e.Templateid).HasName("PK_layout_template");
            entity.ToTable("layout_template");
            entity.Property(e => e.Templateid).HasMaxLength(64).HasColumnName("templateid");
            entity.Property(e => e.Nome).HasMaxLength(100).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("descricao");
            entity.Property(e => e.Tipo).HasMaxLength(50).HasColumnName("tipo");
            entity.Property(e => e.Layout).HasColumnName("layout");
            entity.Property(e => e.Padrao).HasColumnName("padrao");
            entity.Property(e => e.Datainclusao).HasColumnName("datainclusao");
        });

        modelBuilder.Entity<DictBloco>(entity =>
        {
            entity.HasKey(e => e.Tipobloco).HasName("PK_dict_blocos");
            entity.ToTable("dict_blocos");
            entity.Property(e => e.Tipobloco).HasMaxLength(50).HasColumnName("tipobloco");
            entity.Property(e => e.Nome).HasMaxLength(100).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("descricao");
            entity.Property(e => e.Icone).HasMaxLength(50).HasColumnName("icone");
            entity.Property(e => e.SchemaConfig).HasColumnName("schema_config");
        });

        modelBuilder.Entity<Arquivo>(entity =>
        {
            entity.HasKey(e => e.Arquivoid).HasName("PK_arquivo");
            entity.ToTable("arquivo");
            entity.Property(e => e.Arquivoid).HasMaxLength(64).HasColumnName("ArquivoId");
            entity.Property(e => e.Areaid).HasMaxLength(64).HasColumnName("AreaId");
            entity.Property(e => e.Conteudoid).HasMaxLength(64).HasColumnName("ConteudoId");
            entity.Property(e => e.Nome).HasMaxLength(64).HasColumnName("Nome");
            entity.Property(e => e.Tipoid).HasMaxLength(64).HasColumnName("TipoId");
        });

        modelBuilder.Entity<Atributo>(entity =>
        {
            entity.HasKey(e => e.Atributoid).HasName("PK_atributo");
            entity.ToTable("atributo");
            entity.Property(e => e.Atributoid).ValueGeneratedNever().HasColumnName("AtributoId");
            entity.Property(e => e.Descricao).HasMaxLength(500).HasColumnName("Descricao");
            entity.Property(e => e.Nome).HasMaxLength(255).HasColumnName("Nome");
            entity.Property(e => e.Produtoid).HasMaxLength(64).HasColumnName("ProdutoId");
            entity.Property(e => e.ParentAtributoId).HasColumnName("ParentAtributoId");
            entity.Property(e => e.Ordem).HasColumnName("Ordem");
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Filhos)
                .HasForeignKey(e => e.ParentAtributoId)
                .HasConstraintName("FK_Atributo_Parent")
                .IsRequired(false);
            entity.HasMany(e => e.Opcoes)
                .WithOne()
                .HasForeignKey(o => o.Atributoid)
                .HasConstraintName("FK_Opcao_Atributo");
        });

        modelBuilder.Entity<Cambio>(entity =>
        {
            entity.HasKey(e => e.Cambiogroupid).HasName("PK_cambio");
            entity.ToTable("cambio");
            entity.Property(e => e.Cambiogroupid).HasMaxLength(64).HasColumnName("CambioGroupId");
            entity.Property(e => e.Datacotacao).HasColumnName("DataCotacao");
            entity.Property(e => e.Moedasxml).HasMaxLength(1000).HasColumnName("MoedasXml");
            entity.Property(e => e.Tipo).HasColumnName("Tipo");
        });

        modelBuilder.Entity<Caterium>(entity =>
        {
            entity.HasKey(e => e.Cateriaid).HasName("PK_categoria");
            entity.ToTable("categoria");
            entity.Property(e => e.Cateriaid).HasMaxLength(64).HasColumnName("CategoriaId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Cateriaidpai).HasMaxLength(64).HasColumnName("CategoriaIdPai");
            entity.Property(e => e.Descricao).HasMaxLength(1000).HasColumnName("Descricao");
            entity.Property(e => e.Nome).HasMaxLength(200).HasColumnName("Nome");
            entity.Property(e => e.Tipocateria).HasColumnName("TipoCategoria");
        });

        modelBuilder.Entity<Ciaaerea>(entity =>
        {
            entity.HasKey(e => e.Ciaaereaid).HasName("PK_ciaaerea");
            entity.ToTable("ciaaerea");
            entity.Property(e => e.Ciaaereaid).HasMaxLength(64).HasColumnName("CiaAereaId");
            entity.Property(e => e.Ativo).HasColumnName("Ativo");
            entity.Property(e => e.Descricao).HasMaxLength(4000).HasColumnName("Descricao");
            entity.Property(e => e.DescricaoLonga).HasColumnName("Descricao_Longa");
            entity.Property(e => e.Lotipo).HasMaxLength(300).HasColumnName("Logotipo");
            entity.Property(e => e.Tipoint).HasColumnName("TipoInt");
            entity.Property(e => e.Tiponac).HasColumnName("TipoNac");
            entity.Property(e => e.WebticketStr).HasMaxLength(450).HasColumnName("webticket_str");
        });

        modelBuilder.Entity<Conteudo>(entity =>
        {
            entity.HasKey(e => e.Conteudoid).HasName("PK_conteudo");
            entity.ToTable("conteudo");
            entity.Property(e => e.Conteudoid).HasMaxLength(64).HasColumnName("ConteudoId");
            entity.Property(e => e.Areaid).HasMaxLength(64).HasColumnName("AreaId");
            entity.Property(e => e.Autor).HasMaxLength(80).HasColumnName("Autor");
            entity.Property(e => e.Cateriaid).HasMaxLength(64).HasColumnName("CategoriaId");
            entity.Property(e => e.Datafinal).HasColumnName("DataFinal");
            entity.Property(e => e.Datainclusao).HasColumnName("DataInclusao");
            entity.Property(e => e.Texto).HasColumnName("Texto");
            entity.Property(e => e.Titulo).HasMaxLength(80).HasColumnName("Titulo");
        });

        modelBuilder.Entity<Conteudovalor>(entity =>
        {
            entity.HasKey(e => e.Conteudoid).HasName("PK_conteudovalor");
            entity.ToTable("conteudovalor");
            entity.Property(e => e.Conteudoid).HasMaxLength(64).HasColumnName("ConteudoId");
            entity.Property(e => e.Unidadeid).HasColumnName("UnidadeId");
            entity.Property(e => e.Valor).HasPrecision(10, 2).HasColumnName("Valor");
        });

        modelBuilder.Entity<DictSocialmedium>(entity =>
        {
            entity.HasKey(e => e.Socialmediaid).HasName("PK_dict_socialmedia");
            entity.ToTable("dict_socialmedia");
            entity.Property(e => e.Socialmediaid).ValueGeneratedNever().HasColumnName("SocialMediaId");
            entity.Property(e => e.Socialmedianame).HasMaxLength(45).HasColumnName("SocialMediaName");
            entity.Property(e => e.Socialmediaurl).HasMaxLength(45).HasColumnName("SocialMediaUrl");
        });

        modelBuilder.Entity<DictTemplate>(entity =>
        {
            entity.HasKey(e => e.Idtemplate).HasName("PK_dict_templates");
            entity.ToTable("dict_templates");
            entity.Property(e => e.Idtemplate).HasMaxLength(100).HasColumnName("idTemplate");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("Descricao");
            entity.Property(e => e.Nome).HasMaxLength(255).HasColumnName("Nome");
            entity.Property(e => e.Viewrelacionada).HasMaxLength(45).HasColumnName("viewRelacionada");
        });

        modelBuilder.Entity<Dictarea>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dictareas");
            entity.ToTable("dictareas");
            entity.Property(e => e.Id).ValueGeneratedNever().HasColumnName("id");
            entity.Property(e => e.Nome).HasMaxLength(12).HasColumnName("nome");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
        });

        modelBuilder.Entity<Formulario>(entity =>
        {
            entity.HasKey(e => new { e.Formularioid, e.Nome }).HasName("PK_formulario");
            entity.ToTable("formulario");
            entity.Property(e => e.Formularioid).HasMaxLength(64).HasColumnName("formularioid");
            entity.Property(e => e.Nome).HasMaxLength(255).HasColumnName("nome");
            entity.Property(e => e.Areaid).HasMaxLength(64).HasColumnName("areaid");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Datainclusao).HasColumnName("datainclusao");
            entity.Property(e => e.Valor).HasMaxLength(8000).HasColumnName("valor");
            entity.Property(e => e.Categoriaid).HasMaxLength(64).HasColumnName("categoriaid");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Faqid).HasName("PK_faq");
            entity.ToTable("faq");
            entity.Property(e => e.Faqid).HasMaxLength(64).HasColumnName("faqid");
            entity.Property(e => e.Formularioid).HasMaxLength(64).HasColumnName("formularioid");
            entity.Property(e => e.Pergunta).HasMaxLength(500).HasColumnName("pergunta");
            entity.Property(e => e.Resposta).HasColumnName("resposta");
            entity.Property(e => e.Ordem).HasColumnName("ordem");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Datainclusao).HasColumnName("datainclusao");
        });

        modelBuilder.Entity<Formularionew>(entity =>
        {
            entity.HasKey(e => e.Idform).HasName("PK_formularionew");
            entity.ToTable("formularionew");
            entity.Property(e => e.Idform).HasColumnName("IdForm");
            entity.Property(e => e.Ativo).HasColumnName("Ativo");
            entity.Property(e => e.Email).HasMaxLength(155).HasColumnName("Email");
            entity.Property(e => e.Nome).HasMaxLength(100).HasColumnName("Nome");
            entity.Property(e => e.Telefone).HasMaxLength(15).HasColumnName("Telefone");
            entity.Property(e => e.Texto).HasMaxLength(255).HasColumnName("Texto");
            entity.Property(e => e.Tipo).HasColumnName("Tipo");
            entity.Property(e => e.Formularioid).HasMaxLength(64).HasColumnName("formularioid");
        });

        modelBuilder.Entity<Imagem>(entity =>
        {
            entity.HasKey(e => e.Imagemid).HasName("PK_imagem");
            entity.ToTable("imagem");
            entity.Property(e => e.Imagemid).HasMaxLength(64).HasColumnName("ImagemId");
            entity.Property(e => e.Altura).HasColumnName("Altura");
            entity.Property(e => e.Areaid).HasMaxLength(64).HasColumnName("AreaId");
            entity.Property(e => e.Conteudoid).HasMaxLength(64).HasColumnName("ConteudoId");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("Descricao");
            entity.Property(e => e.Largura).HasColumnName("Largura");
            entity.Property(e => e.Parentid).HasMaxLength(64).HasColumnName("ParentId");
            entity.Property(e => e.Tipoid).HasMaxLength(64).HasColumnName("TipoId");
            entity.Property(e => e.Url).HasMaxLength(300).HasColumnName("Url");
        });

        modelBuilder.Entity<Infofoto>(entity =>
        {
            entity.HasKey(e => new { e.Fotoid, e.Cateriaid }).HasName("PK_infofoto");
            entity.ToTable("infofoto");
            entity.Property(e => e.Fotoid).HasMaxLength(64).HasColumnName("FotoId");
            entity.Property(e => e.Cateriaid).HasMaxLength(64).HasColumnName("CategoriaId");
            entity.Property(e => e.Descricao).HasMaxLength(1000).HasColumnName("Descricao");
            entity.Property(e => e.Fotourl).HasMaxLength(300).HasColumnName("FotoUrl");
        });

        modelBuilder.Entity<Informativo>(entity =>
        {
            entity.HasKey(e => e.Infoid).HasName("PK_informativo");
            entity.ToTable("informativo");
            entity.Property(e => e.Infoid).HasMaxLength(64).HasColumnName("InfoId");
            entity.Property(e => e.Ativo).HasColumnName("Ativo");
            entity.Property(e => e.Data).HasColumnName("Data");
            entity.Property(e => e.Foto).HasMaxLength(64).HasColumnName("Foto");
            entity.Property(e => e.Texto).HasColumnName("Texto");
            entity.Property(e => e.Tipoenvio).HasMaxLength(64).HasColumnName("TipoEnvio");
            entity.Property(e => e.Titulo).HasMaxLength(300).HasColumnName("Titulo");
        });

        modelBuilder.Entity<Modulo>(entity =>
        {
            entity.HasKey(e => e.Moduloid).HasName("PK_modulo");
            entity.ToTable("modulo");
            entity.Property(e => e.Moduloid).HasMaxLength(64).HasColumnName("ModuloId");
            entity.Property(e => e.Nome).HasMaxLength(400).HasColumnName("Nome");
            entity.Property(e => e.Posicao).HasColumnName("Posicao");
            entity.Property(e => e.Url).HasMaxLength(400).HasColumnName("Url");
        });

        modelBuilder.Entity<Moduloconf>(entity =>
        {
            entity.HasKey(e => e.Moduloconfid).HasName("PK_moduloconf");
            entity.ToTable("moduloconf");
            entity.Property(e => e.Moduloconfid).HasMaxLength(64).HasColumnName("ModuloConfId");
            entity.Property(e => e.Descricao).HasMaxLength(800).HasColumnName("Descricao");
            entity.Property(e => e.Nome).HasMaxLength(200).HasColumnName("Nome");
        });

        modelBuilder.Entity<Moedum>(entity =>
        {
            entity.HasKey(e => new { e.Moedaid, e.Nome }).HasName("PK_moeda");
            entity.ToTable("moeda");
            entity.Property(e => e.Moedaid).HasMaxLength(64).HasColumnName("MoedaId");
            entity.Property(e => e.Nome).HasMaxLength(80).HasColumnName("Nome");
            entity.Property(e => e.Sigla).HasMaxLength(34).IsFixedLength().HasColumnName("Sigla");
        });

        modelBuilder.Entity<Newsletter>(entity =>
        {
            entity.HasKey(e => e.Newsid).HasName("PK_newsletter");
            entity.ToTable("newsletter");
            entity.Property(e => e.Newsid).HasMaxLength(64).HasColumnName("NewsId");
            entity.Property(e => e.Ativo).HasColumnName("Ativo");
            entity.Property(e => e.Autor).HasMaxLength(1000).HasColumnName("Autor");
            entity.Property(e => e.Cateriaid).HasMaxLength(64).HasColumnName("CategoriaId");
            entity.Property(e => e.Data).HasColumnName("Data");
            entity.Property(e => e.Foto).HasMaxLength(4000).HasColumnName("Foto");
            entity.Property(e => e.Frente).HasColumnName("Frente");
            entity.Property(e => e.Texto).HasColumnName("Texto");
            entity.Property(e => e.Titulo).HasMaxLength(3000).HasColumnName("Titulo");
        });

        modelBuilder.Entity<Opcao>(entity =>
        {
            entity.HasKey(e => e.Opcaoid).HasName("PK_opcao");
            entity.ToTable("opcao");
            entity.Property(e => e.Opcaoid).HasMaxLength(64).HasColumnName("OpcaoId");
            entity.Property(e => e.Atributoid).HasColumnName("AtributoId");
            entity.Property(e => e.Descricao).HasMaxLength(500).HasColumnName("Descricao");
            entity.Property(e => e.Estoque).HasColumnName("Estoque");
            entity.Property(e => e.Nome).HasMaxLength(500).HasColumnName("Nome");
            entity.Property(e => e.Qtd).HasColumnName("Qtd");
            entity.Property(e => e.ValorAdicional).HasPrecision(18, 2).HasColumnName("ValorAdicional");
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => new { e.Produtoid, e.Sku }).HasName("PK_produto");
            entity.ToTable("produto");
            entity.Property(e => e.Produtoid).HasMaxLength(64).HasColumnName("ProdutoId");
            entity.Property(e => e.Sku).HasMaxLength(45).HasColumnName("sku");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Cateriaid).HasMaxLength(64).HasColumnName("CategoriaId");
            entity.Property(e => e.Datafinal).HasColumnName("DataFinal");
            entity.Property(e => e.Datainicio).HasColumnName("DataInicio");
            entity.Property(e => e.Descricacurta).HasMaxLength(150).HasColumnName("DescricaCurta");
            entity.Property(e => e.Descricao).HasColumnName("Descricao");
            entity.Property(e => e.Destaque).HasColumnName("Destaque");
            entity.Property(e => e.Detalhetecnico).HasColumnName("DetalheTecnico");
            entity.Property(e => e.Nome).HasMaxLength(130).HasColumnName("Nome");
            entity.Property(e => e.Pagsegurokey).HasMaxLength(800).HasColumnName("PagSeguroKey");
            entity.Property(e => e.Produtocol).HasMaxLength(45).HasColumnName("produtocol");
            entity.Property(e => e.Tipo).HasColumnName("Tipo");
            entity.Property(e => e.UnidadeVenda).HasMaxLength(45).HasColumnName("UnidadeVenda");
            entity.Property(e => e.Valor).HasPrecision(18, 2).HasColumnName("Valor");
        });

        modelBuilder.Entity<Refatributoxopcao>(entity =>
        {
            entity.HasKey(e => e.Relacaoid).HasName("PK_refatributoxopcao");
            entity.ToTable("refatributoxopcao");
            entity.Property(e => e.Relacaoid).ValueGeneratedNever().HasColumnName("relacaoid");
            entity.Property(e => e.Atributoid).HasColumnName("atributoid");
            entity.Property(e => e.Opcaoid).HasColumnName("opcaoid");
        });

        modelBuilder.Entity<Relatributoxproduto>(entity =>
        {
            entity.HasKey(e => e.Relacaoid).HasName("PK_relatributoxproduto");
            entity.ToTable("relatributoxproduto");
            entity.Property(e => e.Relacaoid).ValueGeneratedNever().HasColumnName("Relacaoid");
            entity.Property(e => e.Atributoid).HasColumnName("Atributoid");
            entity.Property(e => e.Produtoid).HasColumnName("ProdutoId");
        });

        modelBuilder.Entity<Relimagemconteudo>(entity =>
        {
            entity.HasKey(e => e.Relid).HasName("PK_relimagemconteudo");
            entity.ToTable("relimagemconteudo");
            entity.Property(e => e.Relid).HasColumnName("relid");
            entity.Property(e => e.Imagemid).HasMaxLength(45).HasColumnName("imagemid");
            entity.Property(e => e.Parentid).HasMaxLength(45).HasColumnName("parentid");
        });

        modelBuilder.Entity<Relmoduloaplicacao>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Aplicacaoid, e.Moduloid }).HasName("PK_relmoduloaplicacao");
            entity.ToTable("relmoduloaplicacao");
            entity.Property(e => e.Relacaoid).HasMaxLength(64).HasColumnName("RelacaoId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Moduloid).HasMaxLength(64).HasColumnName("ModuloId");
        });

        modelBuilder.Entity<Relmoduloconfaplicacao>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Moduloconfid, e.Aplicacaoid, e.Datainclusao, e.Datafinalizacao }).HasName("PK_relmoduloconfaplicacao");
            entity.ToTable("relmoduloconfaplicacao");
            entity.Property(e => e.Relacaoid).HasMaxLength(64).HasColumnName("RelacaoId");
            entity.Property(e => e.Moduloconfid).HasMaxLength(64).HasColumnName("ModuloConfId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Datainclusao).HasColumnName("DataInclusao");
            entity.Property(e => e.Datafinalizacao).HasColumnName("DataFinalizacao");
        });

        modelBuilder.Entity<Relmodulousuario>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Moduloid, e.Usuarioid }).HasName("PK_relmodulousuario");
            entity.ToTable("relmodulousuario");
            entity.Property(e => e.Relacaoid).HasMaxLength(64).HasColumnName("RelacaoId");
            entity.Property(e => e.Moduloid).HasMaxLength(64).HasColumnName("ModuloId");
            entity.Property(e => e.Usuarioid).HasMaxLength(64).HasColumnName("UsuarioId");
        });

        modelBuilder.Entity<Relusuarioaplicacao>(entity =>
        {
            entity.HasKey(e => new { e.Relacaoid, e.Aplicacaoid, e.Usuarioid }).HasName("PK_relusuarioaplicacao");
            entity.ToTable("relusuarioaplicacao");
            entity.Property(e => e.Relacaoid).HasMaxLength(64).HasColumnName("RelacaoId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("AplicacaoId");
            entity.Property(e => e.Usuarioid).HasMaxLength(64).HasColumnName("UsuarioId");
        });

        modelBuilder.Entity<Socialmedium>(entity =>
        {
            entity.HasKey(e => e.Socialmediaid).HasName("PK_socialmedia");
            entity.ToTable("socialmedia");
            entity.Property(e => e.Socialmediaid).HasMaxLength(45).HasColumnName("SocialMediaId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(45).HasColumnName("AplicacaoId");
            entity.Property(e => e.Socialmedialink).HasMaxLength(255).HasColumnName("SocialMediaLink");
            entity.Property(e => e.Socialmediatypeid).HasColumnName("SocialMediaTypeId");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_templates");
            entity.ToTable("templates");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ativo).HasColumnName("Ativo");
            entity.Property(e => e.Descricao).HasMaxLength(255).HasColumnName("Descricao");
            entity.Property(e => e.Nome).HasMaxLength(155).HasColumnName("Nome");
            entity.Property(e => e.Url).HasMaxLength(80).HasColumnName("Url");
        });

        modelBuilder.Entity<Tipocotacao>(entity =>
        {
            entity.HasKey(e => e.Tipocotacaoid).HasName("PK_tipocotacao");
            entity.ToTable("tipocotacao");
            entity.Property(e => e.Tipocotacaoid).HasMaxLength(64).HasColumnName("TipoCotacaoId");
            entity.Property(e => e.Descricao).HasMaxLength(200).HasColumnName("Descricao");
            entity.Property(e => e.Nome).HasMaxLength(200).HasColumnName("Nome");
        });

        modelBuilder.Entity<Tipoenvio>(entity =>
        {
            entity.HasKey(e => e.Tipoenvioid).HasName("PK_tipoenvio");
            entity.ToTable("tipoenvio");
            entity.Property(e => e.Tipoenvioid).HasMaxLength(64).HasColumnName("TipoEnvioId");
            entity.Property(e => e.Tipoenviodesc).HasMaxLength(300).HasColumnName("TipoEnvioDesc");
        });

        modelBuilder.Entity<Unidade>(entity =>
        {
            entity.HasKey(e => e.Unidadeid).HasName("PK_unidades");
            entity.ToTable("unidades");
            entity.Property(e => e.Unidadeid).ValueGeneratedNever().HasColumnName("UnidadeId");
            entity.Property(e => e.Nome).HasMaxLength(45).HasColumnName("Nome");
            entity.Property(e => e.Sigla).HasMaxLength(45).HasColumnName("Sigla");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("PK_usuario");
            entity.ToTable("usuario");
            entity.Property(e => e.Userid).HasMaxLength(64).HasColumnName("UserId");
            entity.Property(e => e.Apelido).HasMaxLength(6).HasColumnName("Apelido");
            entity.Property(e => e.Ativo).HasColumnName("Ativo");
            entity.Property(e => e.Datainclusao).HasColumnName("DataInclusao");
            entity.Property(e => e.Nome).HasMaxLength(300).HasColumnName("Nome");
            entity.Property(e => e.Senha).HasMaxLength(12).HasColumnName("Senha");
            entity.Property(e => e.Sobrenome).HasMaxLength(300).HasColumnName("Sobrenome");
        });

        modelBuilder.Entity<IaConfig>(entity =>
        {
            entity.HasKey(e => e.Aplicacaoid);
            entity.ToTable("ia_config");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(50).HasColumnName("aplicacaoid");
            entity.Property(e => e.Provedor).HasMaxLength(20).HasColumnName("provedor");
            entity.Property(e => e.Apikey).HasMaxLength(500).HasColumnName("apikey");
            entity.Property(e => e.Modelo).HasMaxLength(100).HasColumnName("modelo");
            entity.Property(e => e.LimiteDiario).HasColumnName("limite_diario");
        });

        modelBuilder.Entity<IaCache>(entity =>
        {
            entity.HasKey(e => e.Cacheid);
            entity.ToTable("ia_cache");
            entity.Property(e => e.Cacheid).HasMaxLength(36).HasColumnName("cacheid");
            entity.Property(e => e.Hash).HasMaxLength(64).HasColumnName("hash");
            entity.Property(e => e.Resultado).HasColumnName("resultado");
            entity.Property(e => e.Datainclusao).HasColumnName("datainclusao");
            entity.Property(e => e.Datavencimento).HasColumnName("datavencimento");
        });

        modelBuilder.Entity<IaUso>(entity =>
        {
            entity.HasKey(e => e.Usoid);
            entity.ToTable("ia_uso");
            entity.Property(e => e.Usoid).HasMaxLength(36).HasColumnName("usoid");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(50).HasColumnName("aplicacaoid");
            entity.Property(e => e.Data).HasColumnName("data").HasColumnType("date")
                .HasConversion(v => v.ToDateTime(TimeOnly.MinValue), v => DateOnly.FromDateTime(v));
            entity.Property(e => e.Contador).HasColumnName("contador");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.Pedidoid).HasName("pedidoPK");
            entity.ToTable("pedido");
            entity.Property(e => e.Pedidoid).ValueGeneratedOnAdd().HasColumnName("pedidoid");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(36).HasColumnName("aplicacaoid");
            entity.Property(e => e.Numeropedido).HasMaxLength(100).HasColumnName("numeropedido");
            entity.Property(e => e.Clientenome).HasMaxLength(200).HasColumnName("clientenome");
            entity.Property(e => e.Clienteemail).HasMaxLength(200).HasColumnName("clienteemail");
            entity.Property(e => e.Valorpedido).HasPrecision(12, 2).HasColumnName("valorpedido");
            entity.Property(e => e.Statusatual).HasMaxLength(50).HasColumnName("statusatual");
            entity.Property(e => e.MetodoPagamento).HasMaxLength(50).HasColumnName("metodopagamento");
            entity.Property(e => e.Datainclusao).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("datainclusao");
        });

        modelBuilder.Entity<Statuspedido>(entity =>
        {
            entity.HasKey(e => e.Statuspedidoid).HasName("statuspedidoPK");
            entity.ToTable("statuspedido");
            entity.Property(e => e.Statuspedidoid).ValueGeneratedOnAdd().HasColumnName("statuspedidoid");
            entity.Property(e => e.Pedidoid).HasColumnName("pedidoid");
            entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
            entity.Property(e => e.Descricao).HasMaxLength(500).HasColumnName("descricao");
            entity.Property(e => e.Datahora).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("datahora");
            entity.HasOne(e => e.PedidoNavigation)
                .WithMany(p => p.Statuspedidos)
                .HasForeignKey(e => e.Pedidoid)
                .HasConstraintName("statuspedido_pedidoid_fk");
        });

        modelBuilder.Entity<OrcamentoCabecalho>(entity =>
        {
            entity.HasKey(e => e.Orcamentoid).HasName("PK_orcamentocabecalho");
            entity.ToTable("orcamentocabecalho");
            entity.Property(e => e.Orcamentoid).ValueGeneratedOnAdd().HasColumnName("orcamentoid");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("aplicacaoid");
            entity.Property(e => e.Nome).HasMaxLength(200).HasColumnName("nome");
            entity.Property(e => e.Email).HasMaxLength(200).HasColumnName("email");
            entity.Property(e => e.Telefone).HasMaxLength(50).HasColumnName("telefone");
            entity.Property(e => e.Descricaoservico).HasColumnName("descricaoservico");
            entity.Property(e => e.Valorestimado).HasPrecision(12, 2).HasColumnName("valorestimado");
            entity.Property(e => e.Prazo).HasMaxLength(200).HasColumnName("prazo");
            entity.Property(e => e.Nomevendedor).HasMaxLength(200).HasColumnName("nomevendedor");
            entity.Property(e => e.Aprovado).HasColumnName("aprovado");
            entity.Property(e => e.Datainclusao).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("datainclusao");
        });

        modelBuilder.Entity<OrcamentoDetalhe>(entity =>
        {
            entity.HasKey(e => e.Orcamentodetalheid).HasName("PK_orcamentodetalhe");
            entity.ToTable("orcamentodetalhe");
            entity.Property(e => e.Orcamentodetalheid).ValueGeneratedOnAdd().HasColumnName("orcamentodetalheid");
            entity.Property(e => e.Orcamentoid).HasColumnName("orcamentoid");
            entity.Property(e => e.Descricao).HasMaxLength(500).HasColumnName("descricao");
            entity.Property(e => e.Quantidade).HasPrecision(10, 2).HasColumnName("quantidade");
            entity.Property(e => e.Valor).HasPrecision(12, 2).HasColumnName("valor");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.HasOne(e => e.OrcamentoCabecalhoNavigation)
                .WithMany(p => p.OrcamentoDetalhes)
                .HasForeignKey(e => e.Orcamentoid)
                .HasConstraintName("FK_orcamentodetalhe_orcamentoid");
        });

        modelBuilder.Entity<PublicToken>(entity =>
        {
            entity.HasKey(e => e.PublicTokenId).HasName("PK_publictoken");
            entity.ToTable("publictoken");
            entity.HasIndex(e => e.Token).IsUnique().HasDatabaseName("UQ_publictoken_token");
            entity.HasIndex(e => e.Aplicacaoid).HasDatabaseName("IX_publictoken_aplicacaoid");
            entity.Property(e => e.PublicTokenId).ValueGeneratedOnAdd().HasColumnName("publictokenid");
            entity.Property(e => e.Token).HasMaxLength(100).HasColumnName("token");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("aplicacaoid");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.Datainclusao).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("datainclusao");
            entity.Property(e => e.Datavencimento).HasColumnName("datavencimento");
        });

        modelBuilder.Entity<OrcamentoDetalheComposto>(entity =>
        {
            entity.HasKey(e => e.OrcamentoDetalheCompostoId).HasName("PK_orcamentodetalhecomposto");
            entity.ToTable("orcamentodetalhecomposto");
            entity.Property(e => e.OrcamentoDetalheCompostoId).ValueGeneratedOnAdd().HasColumnName("OrcamentoDetalheCompostoId");
            entity.Property(e => e.Orcamentoid).HasColumnName("orcamentoid");
            entity.Property(e => e.Produtoid).HasMaxLength(64).HasColumnName("Produtoid");
            entity.Property(e => e.Quantidade).HasPrecision(10, 2).HasColumnName("Quantidade");
            entity.Property(e => e.ValorBase).HasPrecision(18, 2).HasColumnName("ValorBase");
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2).HasColumnName("ValorTotal");
            entity.Property(e => e.ConfiguracaoJson).HasColumnName("ConfiguracaoJson");
            entity.Property(e => e.Versao).HasDefaultValue(1).HasColumnName("Versao");
            entity.Property(e => e.Atual).HasDefaultValue(true).HasColumnName("Atual");
            entity.Property(e => e.Datainclusao).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("Datainclusao");
            entity.HasOne(e => e.Cabecalho)
                .WithMany(c => c.OrcamentoDetalheCompostos)
                .HasForeignKey(e => e.Orcamentoid)
                .HasConstraintName("FK_OrcDetalheComposto_Cabecalho");
            // Produto tem PK composta (Produtoid, Sku) — necessário especificar a chave principal alternativa
            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.Produtoid)
                .HasPrincipalKey(p => p.Produtoid)
                .HasConstraintName("FK_OrcDetalheComposto_Produto");
        });

        modelBuilder.Entity<Selecao>(entity =>
        {
            entity.HasKey(e => e.SelecaoId).HasName("PK_selecao");
            entity.ToTable("selecao");
            entity.Property(e => e.SelecaoId).ValueGeneratedOnAdd().HasColumnName("SelecaoId");
            entity.Property(e => e.OrcamentoDetalheCompostoId).HasColumnName("OrcamentoDetalheCompostoId");
            entity.Property(e => e.Atributoid).HasColumnName("Atributoid");
            entity.Property(e => e.Opcaoid).HasMaxLength(64).HasColumnName("Opcaoid");
            entity.Property(e => e.ValorAdicional).HasPrecision(18, 2).HasDefaultValue(0m).HasColumnName("ValorAdicional");
            entity.HasOne(e => e.Detalhe)
                .WithMany(d => d.Selecoes)
                .HasForeignKey(e => e.OrcamentoDetalheCompostoId)
                .HasConstraintName("FK_Selecao_Detalhe");
            entity.HasOne(e => e.Atributo)
                .WithMany()
                .HasForeignKey(e => e.Atributoid)
                .HasConstraintName("FK_Selecao_Atributo")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Opcao)
                .WithMany()
                .HasForeignKey(e => e.Opcaoid)
                .HasConstraintName("FK_Selecao_Opcao");
        });

        modelBuilder.Entity<ModeloComposto>(entity =>
        {
            entity.HasKey(e => e.ModeloCompostoId).HasName("PK_modelocomposto");
            entity.ToTable("modelocomposto");
            entity.HasIndex(e => new { e.ConfiguracaoHash, e.Aplicacaoid, e.Produtoid })
                .IsUnique()
                .HasDatabaseName("IX_modelocomposto_hash");
            entity.Property(e => e.ModeloCompostoId).ValueGeneratedOnAdd().HasColumnName("ModeloCompostoId");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("Aplicacaoid");
            entity.Property(e => e.Produtoid).HasMaxLength(64).HasColumnName("Produtoid");
            entity.Property(e => e.Nome).HasMaxLength(500).HasColumnName("Nome");
            entity.Property(e => e.ValorUnitario).HasPrecision(18, 2).HasColumnName("ValorUnitario");
            entity.Property(e => e.ConfiguracaoHash).HasMaxLength(64).HasColumnName("ConfiguracaoHash");
            entity.Property(e => e.Usos).HasDefaultValue(1).HasColumnName("Usos");
            entity.Property(e => e.Datacriacao).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("Datacriacao");
        });

        modelBuilder.Entity<ModeloSelecao>(entity =>
        {
            entity.HasKey(e => e.ModeloSelecaoId).HasName("PK_modeloselecao");
            entity.ToTable("modeloselecao");
            entity.Property(e => e.ModeloSelecaoId).ValueGeneratedOnAdd().HasColumnName("ModeloSelecaoId");
            entity.Property(e => e.ModeloCompostoId).HasColumnName("ModeloCompostoId");
            entity.Property(e => e.Atributoid).HasColumnName("Atributoid");
            entity.Property(e => e.Opcaoid).HasMaxLength(64).HasColumnName("Opcaoid");
            entity.HasOne(e => e.Modelo)
                .WithMany(m => m.Selecoes)
                .HasForeignKey(e => e.ModeloCompostoId)
                .HasConstraintName("FK_ModeloSelecao_Modelo");
        });

        modelBuilder.Entity<ProdutoMaoDeObra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_produto_mao_de_obra");
            entity.ToTable("produto_mao_de_obra");
            entity.Property(e => e.Id).ValueGeneratedOnAdd().HasColumnName("id");
            entity.Property(e => e.Produtoid).HasMaxLength(64).HasColumnName("produtoid");
            entity.Property(e => e.Tipo).HasMaxLength(30).HasColumnName("tipo");
            entity.Property(e => e.Descricao).HasMaxLength(200).HasColumnName("descricao");
            entity.Property(e => e.CapacidadeDia).HasColumnName("capacidade_dia");
            entity.Property(e => e.ValorDia).HasPrecision(12, 2).HasColumnName("valor_dia");
            entity.Property(e => e.ValorMilheiro).HasPrecision(12, 2).HasColumnName("valor_milheiro");
            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.Produtoid)
                .HasPrincipalKey(p => p.Produtoid)
                .HasConstraintName("FK_ProdutoMaoDeObra_Produto");
        });

        modelBuilder.Entity<ProdutoTemplate>(entity =>
        {
            entity.HasKey(e => e.ProdutoTemplateid).HasName("PK_produto_template");
            entity.ToTable("produto_template");
            entity.Property(e => e.ProdutoTemplateid).HasMaxLength(64).HasColumnName("produto_template_id");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(64).HasColumnName("aplicacao_id");
            entity.Property(e => e.SegmentoTenantId).HasMaxLength(64).HasColumnName("segmento_tenant_id");
            entity.Property(e => e.Nome).HasMaxLength(200).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.ConteudoJson).HasColumnName("conteudo_json");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            entity.Property(e => e.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        });

        modelBuilder.Entity<SegmentoTenant>(entity =>
        {
            entity.HasKey(e => e.SegmentoTenantId).HasName("PK_segmento_tenant");
            entity.ToTable("segmento_tenant");
            entity.Property(e => e.SegmentoTenantId).HasMaxLength(64).HasColumnName("segmento_tenant_id");
            entity.Property(e => e.Nome).HasMaxLength(100).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        });

        modelBuilder.Entity<AplicacaoSegmento>(entity =>
        {
            entity.HasKey(e => new { e.AplicacaoId, e.SegmentoTenantId }).HasName("PK_aplicacao_segmento");
            entity.ToTable("aplicacao_segmento");
            entity.Property(e => e.AplicacaoId).HasMaxLength(64).HasColumnName("aplicacao_id");
            entity.Property(e => e.SegmentoTenantId).HasMaxLength(64).HasColumnName("segmento_tenant_id");
        });

        modelBuilder.Entity<VitrineTemplate>(entity =>
        {
            entity.HasKey(e => e.VitrineTemplateId).HasName("PK_vitrine_template");
            entity.ToTable("vitrine_template");
            entity.Property(e => e.VitrineTemplateId).HasColumnName("vitrine_template_id");
            entity.Property(e => e.Nome).HasMaxLength(200).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.SegmentoTenantId).HasMaxLength(64).HasColumnName("segmento_tenant_id");
            entity.Property(e => e.HtmlCss).HasColumnName("html_css");
            entity.Property(e => e.VariaveisJson).HasColumnName("variaveis_json");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500).HasColumnName("thumbnail_url");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            entity.Property(e => e.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        });

        modelBuilder.Entity<VitrineConfigurada>(entity =>
        {
            entity.HasKey(e => e.VitrineConfiguradaId).HasName("PK_vitrine_configurada");
            entity.ToTable("vitrine_configurada");
            entity.HasIndex(e => e.AplicacaoId).IsUnique().HasDatabaseName("IX_vitrine_configurada_aplicacao_id");
            entity.Property(e => e.VitrineConfiguradaId).HasColumnName("vitrine_configurada_id");
            entity.Property(e => e.AplicacaoId).HasMaxLength(64).HasColumnName("aplicacao_id");
            entity.Property(e => e.VitrineTemplateId).HasColumnName("vitrine_template_id");
            entity.Property(e => e.ValoresJson).HasColumnName("valores_json");
            entity.Property(e => e.Publicado).HasColumnName("publicado").HasDefaultValue(false);
            entity.Property(e => e.CssProcessado).HasColumnName("css_processado");
            entity.Property(e => e.DataAtualizacao).HasColumnName("data_atualizacao");
        });

        modelBuilder.Entity<ClienteLoja>(entity =>
        {
            entity.HasKey(e => e.ClienteLojaid).HasName("PK_clienteloja");
            entity.ToTable("clienteloja");
            entity.Property(e => e.ClienteLojaid).HasMaxLength(36).HasColumnName("clientelojaid");
            entity.Property(e => e.Aplicacaoid).HasMaxLength(36).HasColumnName("aplicacaoid");
            entity.Property(e => e.SalematicClienteId).HasColumnName("salematicClienteId");
            entity.Property(e => e.Datainclusao).HasColumnName("datainclusao");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
