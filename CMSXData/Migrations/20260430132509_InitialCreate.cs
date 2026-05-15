using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CMSXData.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aplicacao",
                columns: table => new
                {
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 400, nullable: true),
                    Url = table.Column<string>(maxLength: 20, nullable: true),
                    DataInicio = table.Column<DateTime>(nullable: true),
                    DataFinal = table.Column<DateTime>(nullable: true),
                    IdUsuarioInicio = table.Column<string>(maxLength: 36, nullable: true),
                    IdUsuarioFim = table.Column<string>(maxLength: 36, nullable: true),
                    PagSeguroToken = table.Column<string>(maxLength: 120, nullable: true),
                    LayoutChoose = table.Column<string>(maxLength: 150, nullable: true),
                    Posicao = table.Column<int>(nullable: true),
                    mailUser = table.Column<string>(maxLength: 150, nullable: true),
                    mailPassword = table.Column<string>(maxLength: 45, nullable: true),
                    mailServer = table.Column<string>(maxLength: 80, nullable: true),
                    mailPort = table.Column<int>(nullable: true),
                    isSecure = table.Column<byte>(nullable: true),
                    pageFacebook = table.Column<string>(maxLength: 255, nullable: true),
                    pageLinkedin = table.Column<string>(maxLength: 255, nullable: true),
                    pageInstagram = table.Column<string>(maxLength: 255, nullable: true),
                    pageTwitter = table.Column<string>(maxLength: 255, nullable: true),
                    pagePinterest = table.Column<string>(maxLength: 255, nullable: true),
                    pageFlicker = table.Column<string>(maxLength: 255, nullable: true),
                    logotipo = table.Column<byte[]>(nullable: true),
                    googleAdSense = table.Column<string>(maxLength: 500, nullable: true),
                    header = table.Column<string>(maxLength: 245, nullable: true),
                    isActive = table.Column<bool>(nullable: true),
                    Telefone = table.Column<string>(nullable: true),
                    Endereco = table.Column<string>(nullable: true),
                    Descricao = table.Column<string>(nullable: true),
                    IsDemo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aplicacao", x => x.AplicacaoId);
                });

            migrationBuilder.CreateTable(
                name: "areas",
                columns: table => new
                {
                    AreaId = table.Column<string>(maxLength: 64, nullable: false),
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: true),
                    Nome = table.Column<string>(maxLength: 80, nullable: true),
                    Url = table.Column<string>(maxLength: 300, nullable: true),
                    Descricao = table.Column<string>(maxLength: 255, nullable: true),
                    AreaIdPai = table.Column<string>(maxLength: 64, nullable: true),
                    DataInicial = table.Column<DateTime>(nullable: true),
                    DataFinal = table.Column<DateTime>(nullable: true),
                    Imagem = table.Column<byte>(nullable: true),
                    MenuLateral = table.Column<byte>(nullable: true),
                    MenuSplash = table.Column<byte>(nullable: true),
                    MenuCentral = table.Column<byte>(nullable: true),
                    posicao = table.Column<int>(nullable: true),
                    MenuFixo = table.Column<byte>(nullable: true),
                    ListaSimples = table.Column<byte>(nullable: true),
                    ListaSplash = table.Column<byte>(nullable: true),
                    ListaBanner = table.Column<byte>(nullable: true),
                    TipoArea = table.Column<int>(nullable: true),
                    layout = table.Column<string>(nullable: true),
                    PageBuilderVersion = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_areas", x => x.AreaId);
                });

            migrationBuilder.CreateTable(
                name: "arquivo",
                columns: table => new
                {
                    ArquivoId = table.Column<string>(maxLength: 64, nullable: false),
                    AreaId = table.Column<string>(maxLength: 64, nullable: true),
                    ConteudoId = table.Column<string>(maxLength: 64, nullable: true),
                    Nome = table.Column<string>(maxLength: 64, nullable: true),
                    TipoId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_arquivo", x => x.ArquivoId);
                });

            migrationBuilder.CreateTable(
                name: "atributo",
                columns: table => new
                {
                    AtributoId = table.Column<Guid>(nullable: false),
                    Nome = table.Column<string>(maxLength: 45, nullable: false),
                    Descricao = table.Column<string>(maxLength: 45, nullable: false),
                    ProdutoId = table.Column<string>(maxLength: 64, nullable: true),
                    ParentAtributoId = table.Column<Guid>(nullable: true),
                    Ordem = table.Column<int>(nullable: true),
                    ValorAdicional = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atributo", x => x.AtributoId);
                    table.ForeignKey(
                        name: "FK_Atributo_Parent",
                        column: x => x.ParentAtributoId,
                        principalTable: "atributo",
                        principalColumn: "AtributoId");
                });

            migrationBuilder.CreateTable(
                name: "cambio",
                columns: table => new
                {
                    CambioGroupId = table.Column<string>(maxLength: 64, nullable: false),
                    DataCotacao = table.Column<DateTime>(nullable: true),
                    MoedasXml = table.Column<string>(maxLength: 1000, nullable: true),
                    Tipo = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cambio", x => x.CambioGroupId);
                });

            migrationBuilder.CreateTable(
                name: "categoria",
                columns: table => new
                {
                    CategoriaId = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 200, nullable: true),
                    Descricao = table.Column<string>(maxLength: 1000, nullable: true),
                    TipoCategoria = table.Column<int>(nullable: true),
                    CategoriaIdPai = table.Column<string>(maxLength: 64, nullable: true),
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria", x => x.CategoriaId);
                });

            migrationBuilder.CreateTable(
                name: "ciaaerea",
                columns: table => new
                {
                    CiaAereaId = table.Column<string>(maxLength: 64, nullable: false),
                    Descricao = table.Column<string>(maxLength: 4000, nullable: true),
                    Logotipo = table.Column<string>(maxLength: 300, nullable: true),
                    Descricao_Longa = table.Column<string>(nullable: true),
                    Ativo = table.Column<byte>(nullable: true),
                    TipoNac = table.Column<byte>(nullable: true),
                    TipoInt = table.Column<byte>(nullable: true),
                    webticket_str = table.Column<string>(maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciaaerea", x => x.CiaAereaId);
                });

            migrationBuilder.CreateTable(
                name: "conteudo",
                columns: table => new
                {
                    ConteudoId = table.Column<string>(maxLength: 64, nullable: false),
                    AreaId = table.Column<string>(maxLength: 64, nullable: true),
                    Autor = table.Column<string>(maxLength: 80, nullable: true),
                    Titulo = table.Column<string>(maxLength: 80, nullable: true),
                    Texto = table.Column<string>(nullable: true),
                    DataInclusao = table.Column<DateTime>(nullable: true),
                    DataFinal = table.Column<DateTime>(nullable: true),
                    CategoriaId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conteudo", x => x.ConteudoId);
                });

            migrationBuilder.CreateTable(
                name: "conteudovalor",
                columns: table => new
                {
                    ConteudoId = table.Column<string>(maxLength: 64, nullable: false),
                    UnidadeId = table.Column<Guid>(nullable: true),
                    Valor = table.Column<decimal>(precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conteudovalor", x => x.ConteudoId);
                });

            migrationBuilder.CreateTable(
                name: "dict_blocos",
                columns: table => new
                {
                    tipobloco = table.Column<string>(maxLength: 50, nullable: false),
                    nome = table.Column<string>(maxLength: 100, nullable: false),
                    descricao = table.Column<string>(maxLength: 255, nullable: true),
                    icone = table.Column<string>(maxLength: 50, nullable: true),
                    schema_config = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dict_blocos", x => x.tipobloco);
                });

            migrationBuilder.CreateTable(
                name: "dict_socialmedia",
                columns: table => new
                {
                    SocialMediaId = table.Column<int>(nullable: false),
                    SocialMediaName = table.Column<string>(maxLength: 45, nullable: true),
                    SocialMediaUrl = table.Column<string>(maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dict_socialmedia", x => x.SocialMediaId);
                });

            migrationBuilder.CreateTable(
                name: "dict_templates",
                columns: table => new
                {
                    idTemplate = table.Column<string>(maxLength: 100, nullable: false),
                    Nome = table.Column<string>(maxLength: 255, nullable: true),
                    Descricao = table.Column<string>(maxLength: 255, nullable: true),
                    viewRelacionada = table.Column<string>(maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dict_templates", x => x.idTemplate);
                });

            migrationBuilder.CreateTable(
                name: "dictareas",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    nome = table.Column<string>(maxLength: 12, nullable: true),
                    tipo = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dictareas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "faq",
                columns: table => new
                {
                    faqid = table.Column<string>(maxLength: 64, nullable: false),
                    formularioid = table.Column<string>(maxLength: 64, nullable: false),
                    pergunta = table.Column<string>(maxLength: 500, nullable: false),
                    resposta = table.Column<string>(nullable: false),
                    ordem = table.Column<int>(nullable: false),
                    ativo = table.Column<bool>(nullable: false),
                    datainclusao = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faq", x => x.faqid);
                });

            migrationBuilder.CreateTable(
                name: "formulario",
                columns: table => new
                {
                    formularioid = table.Column<string>(maxLength: 64, nullable: false),
                    nome = table.Column<string>(maxLength: 255, nullable: false),
                    valor = table.Column<string>(maxLength: 8000, nullable: true),
                    ativo = table.Column<bool>(nullable: true),
                    datainclusao = table.Column<DateTime>(nullable: true),
                    areaid = table.Column<string>(maxLength: 64, nullable: true),
                    categoriaid = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formulario", x => new { x.formularioid, x.nome });
                });

            migrationBuilder.CreateTable(
                name: "formularionew",
                columns: table => new
                {
                    IdForm = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(maxLength: 100, nullable: true),
                    Tipo = table.Column<int>(nullable: true),
                    Email = table.Column<string>(maxLength: 155, nullable: true),
                    Telefone = table.Column<string>(maxLength: 15, nullable: true),
                    Texto = table.Column<string>(maxLength: 255, nullable: true),
                    Ativo = table.Column<int>(nullable: true),
                    formularioid = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formularionew", x => x.IdForm);
                });

            migrationBuilder.CreateTable(
                name: "grupo",
                columns: table => new
                {
                    GrupoId = table.Column<string>(nullable: false),
                    Nome = table.Column<string>(maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(maxLength: 255, nullable: true),
                    AcessoTotal = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupo", x => x.GrupoId);
                });

            migrationBuilder.CreateTable(
                name: "ia_cache",
                columns: table => new
                {
                    cacheid = table.Column<string>(maxLength: 36, nullable: false),
                    hash = table.Column<string>(maxLength: 64, nullable: false),
                    resultado = table.Column<string>(nullable: false),
                    datainclusao = table.Column<DateTime>(nullable: false),
                    datavencimento = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ia_cache", x => x.cacheid);
                });

            migrationBuilder.CreateTable(
                name: "ia_config",
                columns: table => new
                {
                    aplicacaoid = table.Column<string>(maxLength: 50, nullable: false),
                    provedor = table.Column<string>(maxLength: 20, nullable: true),
                    apikey = table.Column<string>(maxLength: 500, nullable: true),
                    modelo = table.Column<string>(maxLength: 100, nullable: true),
                    limite_diario = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ia_config", x => x.aplicacaoid);
                });

            migrationBuilder.CreateTable(
                name: "ia_uso",
                columns: table => new
                {
                    usoid = table.Column<string>(maxLength: 36, nullable: false),
                    aplicacaoid = table.Column<string>(maxLength: 50, nullable: false),
                    data = table.Column<DateTime>(nullable: false),
                    contador = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ia_uso", x => x.usoid);
                });

            migrationBuilder.CreateTable(
                name: "imagem",
                columns: table => new
                {
                    ImagemId = table.Column<string>(maxLength: 64, nullable: false),
                    Url = table.Column<string>(maxLength: 300, nullable: true),
                    Largura = table.Column<int>(nullable: true),
                    Altura = table.Column<int>(nullable: true),
                    AreaId = table.Column<string>(maxLength: 64, nullable: true),
                    ConteudoId = table.Column<string>(maxLength: 64, nullable: true),
                    Descricao = table.Column<string>(maxLength: 255, nullable: true),
                    ParentId = table.Column<string>(maxLength: 64, nullable: false),
                    TipoId = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imagem", x => x.ImagemId);
                });

            migrationBuilder.CreateTable(
                name: "infofoto",
                columns: table => new
                {
                    FotoId = table.Column<string>(maxLength: 64, nullable: false),
                    CategoriaId = table.Column<string>(maxLength: 64, nullable: false),
                    FotoUrl = table.Column<string>(maxLength: 300, nullable: true),
                    Descricao = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_infofoto", x => new { x.FotoId, x.CategoriaId });
                });

            migrationBuilder.CreateTable(
                name: "informativo",
                columns: table => new
                {
                    InfoId = table.Column<string>(maxLength: 64, nullable: false),
                    Titulo = table.Column<string>(maxLength: 300, nullable: true),
                    Data = table.Column<DateTime>(nullable: true),
                    Texto = table.Column<string>(nullable: true),
                    Foto = table.Column<string>(maxLength: 64, nullable: true),
                    Ativo = table.Column<byte>(nullable: true),
                    TipoEnvio = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informativo", x => x.InfoId);
                });

            migrationBuilder.CreateTable(
                name: "layout_template",
                columns: table => new
                {
                    templateid = table.Column<string>(maxLength: 64, nullable: false),
                    nome = table.Column<string>(maxLength: 100, nullable: false),
                    descricao = table.Column<string>(maxLength: 255, nullable: true),
                    tipo = table.Column<string>(maxLength: 50, nullable: true),
                    layout = table.Column<string>(nullable: true),
                    padrao = table.Column<bool>(nullable: false),
                    datainclusao = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layout_template", x => x.templateid);
                });

            migrationBuilder.CreateTable(
                name: "modelocomposto",
                columns: table => new
                {
                    ModeloCompostoId = table.Column<Guid>(nullable: false),
                    Aplicacaoid = table.Column<string>(maxLength: 64, nullable: false),
                    Produtoid = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 500, nullable: false),
                    ValorUnitario = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    ConfiguracaoHash = table.Column<string>(maxLength: 64, nullable: false),
                    Usos = table.Column<int>(nullable: false, defaultValue: 1),
                    Datacriacao = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modelocomposto", x => x.ModeloCompostoId);
                });

            migrationBuilder.CreateTable(
                name: "modulo",
                columns: table => new
                {
                    ModuloId = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 400, nullable: true),
                    Url = table.Column<string>(maxLength: 400, nullable: true),
                    Posicao = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modulo", x => x.ModuloId);
                });

            migrationBuilder.CreateTable(
                name: "moduloconf",
                columns: table => new
                {
                    ModuloConfId = table.Column<string>(maxLength: 64, nullable: false),
                    Descricao = table.Column<string>(maxLength: 800, nullable: true),
                    Nome = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moduloconf", x => x.ModuloConfId);
                });

            migrationBuilder.CreateTable(
                name: "moeda",
                columns: table => new
                {
                    MoedaId = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 80, nullable: false),
                    Sigla = table.Column<string>(fixedLength: true, maxLength: 34, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moeda", x => new { x.MoedaId, x.Nome });
                });

            migrationBuilder.CreateTable(
                name: "newsletter",
                columns: table => new
                {
                    NewsId = table.Column<string>(maxLength: 64, nullable: false),
                    Titulo = table.Column<string>(maxLength: 3000, nullable: true),
                    Autor = table.Column<string>(maxLength: 1000, nullable: true),
                    Data = table.Column<DateTime>(nullable: true),
                    Frente = table.Column<byte>(nullable: true),
                    Texto = table.Column<string>(nullable: true),
                    Foto = table.Column<string>(maxLength: 4000, nullable: true),
                    CategoriaId = table.Column<string>(maxLength: 64, nullable: true),
                    Ativo = table.Column<byte>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newsletter", x => x.NewsId);
                });

            migrationBuilder.CreateTable(
                name: "orcamentocabecalho",
                columns: table => new
                {
                    orcamentoid = table.Column<Guid>(nullable: false),
                    aplicacaoid = table.Column<string>(maxLength: 64, nullable: false),
                    nome = table.Column<string>(maxLength: 200, nullable: false),
                    email = table.Column<string>(maxLength: 200, nullable: true),
                    telefone = table.Column<string>(maxLength: 50, nullable: true),
                    descricaoservico = table.Column<string>(nullable: true),
                    valorestimado = table.Column<decimal>(precision: 12, scale: 2, nullable: true),
                    prazo = table.Column<string>(maxLength: 200, nullable: true),
                    nomevendedor = table.Column<string>(maxLength: 200, nullable: true),
                    aprovado = table.Column<bool>(nullable: false),
                    datainclusao = table.Column<DateTime>(nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentocabecalho", x => x.orcamentoid);
                });

            migrationBuilder.CreateTable(
                name: "pedido",
                columns: table => new
                {
                    pedidoid = table.Column<Guid>(nullable: false),
                    aplicacaoid = table.Column<string>(maxLength: 36, nullable: true),
                    numeropedido = table.Column<string>(maxLength: 100, nullable: true),
                    clientenome = table.Column<string>(maxLength: 200, nullable: true),
                    clienteemail = table.Column<string>(maxLength: 200, nullable: true),
                    valorpedido = table.Column<decimal>(precision: 12, scale: 2, nullable: true),
                    statusatual = table.Column<string>(maxLength: 50, nullable: true),
                    metodopagamento = table.Column<string>(maxLength: 50, nullable: true),
                    datainclusao = table.Column<DateTime>(nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pedidoPK", x => x.pedidoid);
                });

            migrationBuilder.CreateTable(
                name: "produto",
                columns: table => new
                {
                    ProdutoId = table.Column<string>(maxLength: 64, nullable: false),
                    sku = table.Column<string>(maxLength: 45, nullable: false),
                    Nome = table.Column<string>(maxLength: 130, nullable: true),
                    Descricao = table.Column<string>(nullable: true),
                    Valor = table.Column<decimal>(precision: 18, scale: 2, nullable: true),
                    Tipo = table.Column<int>(nullable: true),
                    Destaque = table.Column<int>(nullable: true),
                    DataInicio = table.Column<DateTime>(nullable: true),
                    DataFinal = table.Column<DateTime>(nullable: true),
                    CategoriaId = table.Column<string>(maxLength: 64, nullable: true),
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: true),
                    PagSeguroKey = table.Column<string>(maxLength: 800, nullable: true),
                    DetalheTecnico = table.Column<string>(nullable: true),
                    DescricaCurta = table.Column<string>(maxLength: 150, nullable: true),
                    produtocol = table.Column<string>(maxLength: 45, nullable: true),
                    SalematicProdutoid = table.Column<int>(nullable: true),
                    UnidadeVenda = table.Column<string>(maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto", x => new { x.ProdutoId, x.sku });
                    table.UniqueConstraint("AK_produto_ProdutoId", x => x.ProdutoId);
                });

            migrationBuilder.CreateTable(
                name: "publictoken",
                columns: table => new
                {
                    publictokenid = table.Column<Guid>(nullable: false),
                    token = table.Column<string>(maxLength: 100, nullable: false),
                    aplicacaoid = table.Column<string>(maxLength: 64, nullable: false),
                    ativo = table.Column<bool>(nullable: false),
                    datainclusao = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    datavencimento = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publictoken", x => x.publictokenid);
                });

            migrationBuilder.CreateTable(
                name: "refatributoxopcao",
                columns: table => new
                {
                    relacaoid = table.Column<Guid>(nullable: false),
                    atributoid = table.Column<Guid>(nullable: false),
                    opcaoid = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refatributoxopcao", x => x.relacaoid);
                });

            migrationBuilder.CreateTable(
                name: "relatributoxproduto",
                columns: table => new
                {
                    Relacaoid = table.Column<Guid>(nullable: false),
                    Atributoid = table.Column<Guid>(nullable: false),
                    ProdutoId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relatributoxproduto", x => x.Relacaoid);
                });

            migrationBuilder.CreateTable(
                name: "relimagemconteudo",
                columns: table => new
                {
                    relid = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    imagemid = table.Column<string>(maxLength: 45, nullable: true),
                    parentid = table.Column<string>(maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relimagemconteudo", x => x.relid);
                });

            migrationBuilder.CreateTable(
                name: "relmoduloaplicacao",
                columns: table => new
                {
                    RelacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    ModuloId = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relmoduloaplicacao", x => new { x.RelacaoId, x.AplicacaoId, x.ModuloId });
                });

            migrationBuilder.CreateTable(
                name: "relmoduloconfaplicacao",
                columns: table => new
                {
                    RelacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    ModuloConfId = table.Column<string>(maxLength: 64, nullable: false),
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    DataInclusao = table.Column<DateTime>(nullable: false),
                    DataFinalizacao = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relmoduloconfaplicacao", x => new { x.RelacaoId, x.ModuloConfId, x.AplicacaoId, x.DataInclusao, x.DataFinalizacao });
                });

            migrationBuilder.CreateTable(
                name: "relmodulousuario",
                columns: table => new
                {
                    RelacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    ModuloId = table.Column<string>(maxLength: 64, nullable: false),
                    UsuarioId = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relmodulousuario", x => new { x.RelacaoId, x.ModuloId, x.UsuarioId });
                });

            migrationBuilder.CreateTable(
                name: "relusuarioaplicacao",
                columns: table => new
                {
                    RelacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    AplicacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    UsuarioId = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relusuarioaplicacao", x => new { x.RelacaoId, x.AplicacaoId, x.UsuarioId });
                });

            migrationBuilder.CreateTable(
                name: "relusuariogrupo",
                columns: table => new
                {
                    RelacaoId = table.Column<string>(nullable: false),
                    UsuarioId = table.Column<string>(nullable: false),
                    GrupoId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relusuariogrupo", x => x.RelacaoId);
                });

            migrationBuilder.CreateTable(
                name: "socialmedia",
                columns: table => new
                {
                    SocialMediaId = table.Column<string>(maxLength: 45, nullable: false),
                    AplicacaoId = table.Column<string>(maxLength: 45, nullable: true),
                    SocialMediaTypeId = table.Column<int>(nullable: true),
                    SocialMediaLink = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socialmedia", x => x.SocialMediaId);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(maxLength: 155, nullable: true),
                    Descricao = table.Column<string>(maxLength: 255, nullable: true),
                    Url = table.Column<string>(maxLength: 80, nullable: true),
                    Ativo = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipocotacao",
                columns: table => new
                {
                    TipoCotacaoId = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 200, nullable: true),
                    Descricao = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipocotacao", x => x.TipoCotacaoId);
                });

            migrationBuilder.CreateTable(
                name: "tipoenvio",
                columns: table => new
                {
                    TipoEnvioId = table.Column<string>(maxLength: 64, nullable: false),
                    TipoEnvioDesc = table.Column<string>(maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipoenvio", x => x.TipoEnvioId);
                });

            migrationBuilder.CreateTable(
                name: "unidades",
                columns: table => new
                {
                    UnidadeId = table.Column<Guid>(nullable: false),
                    Nome = table.Column<string>(maxLength: 45, nullable: true),
                    Sigla = table.Column<string>(maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unidades", x => x.UnidadeId);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    UserId = table.Column<string>(maxLength: 64, nullable: false),
                    Nome = table.Column<string>(maxLength: 300, nullable: false),
                    Sobrenome = table.Column<string>(maxLength: 300, nullable: false),
                    Apelido = table.Column<string>(maxLength: 6, nullable: true),
                    Senha = table.Column<string>(maxLength: 12, nullable: true),
                    Ativo = table.Column<byte>(nullable: true),
                    DataInclusao = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "opcao",
                columns: table => new
                {
                    OpcaoId = table.Column<string>(maxLength: 64, nullable: false),
                    AtributoId = table.Column<Guid>(nullable: false),
                    Qtd = table.Column<int>(nullable: false),
                    Nome = table.Column<string>(maxLength: 500, nullable: true),
                    Descricao = table.Column<string>(maxLength: 500, nullable: true),
                    Estoque = table.Column<int>(nullable: true),
                    ValorAdicional = table.Column<decimal>(precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opcao", x => x.OpcaoId);
                    table.ForeignKey(
                        name: "FK_Opcao_Atributo",
                        column: x => x.AtributoId,
                        principalTable: "atributo",
                        principalColumn: "AtributoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "modeloselecao",
                columns: table => new
                {
                    ModeloSelecaoId = table.Column<Guid>(nullable: false),
                    ModeloCompostoId = table.Column<Guid>(nullable: false),
                    Atributoid = table.Column<Guid>(nullable: false),
                    Opcaoid = table.Column<string>(maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modeloselecao", x => x.ModeloSelecaoId);
                    table.ForeignKey(
                        name: "FK_ModeloSelecao_Modelo",
                        column: x => x.ModeloCompostoId,
                        principalTable: "modelocomposto",
                        principalColumn: "ModeloCompostoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orcamentodetalhe",
                columns: table => new
                {
                    orcamentodetalheid = table.Column<Guid>(nullable: false),
                    orcamentoid = table.Column<Guid>(nullable: false),
                    descricao = table.Column<string>(maxLength: 500, nullable: false),
                    quantidade = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                    valor = table.Column<decimal>(precision: 12, scale: 2, nullable: true),
                    ativo = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentodetalhe", x => x.orcamentodetalheid);
                    table.ForeignKey(
                        name: "FK_orcamentodetalhe_orcamentoid",
                        column: x => x.orcamentoid,
                        principalTable: "orcamentocabecalho",
                        principalColumn: "orcamentoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "statuspedido",
                columns: table => new
                {
                    statuspedidoid = table.Column<Guid>(nullable: false),
                    pedidoid = table.Column<Guid>(nullable: false),
                    status = table.Column<string>(maxLength: 50, nullable: true),
                    descricao = table.Column<string>(maxLength: 500, nullable: true),
                    datahora = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("statuspedidoPK", x => x.statuspedidoid);
                    table.ForeignKey(
                        name: "statuspedido_pedidoid_fk",
                        column: x => x.pedidoid,
                        principalTable: "pedido",
                        principalColumn: "pedidoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orcamentodetalhecomposto",
                columns: table => new
                {
                    OrcamentoDetalheCompostoId = table.Column<Guid>(nullable: false),
                    orcamentoid = table.Column<Guid>(nullable: false),
                    Produtoid = table.Column<string>(maxLength: 64, nullable: false),
                    Quantidade = table.Column<decimal>(precision: 10, scale: 2, nullable: false),
                    ValorBase = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    ValorTotal = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    ConfiguracaoJson = table.Column<string>(nullable: false),
                    Versao = table.Column<int>(nullable: false, defaultValue: 1),
                    Atual = table.Column<bool>(nullable: false, defaultValue: true),
                    Datainclusao = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentodetalhecomposto", x => x.OrcamentoDetalheCompostoId);
                    table.ForeignKey(
                        name: "FK_OrcDetalheComposto_Cabecalho",
                        column: x => x.orcamentoid,
                        principalTable: "orcamentocabecalho",
                        principalColumn: "orcamentoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrcDetalheComposto_Produto",
                        column: x => x.Produtoid,
                        principalTable: "produto",
                        principalColumn: "ProdutoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "selecao",
                columns: table => new
                {
                    SelecaoId = table.Column<Guid>(nullable: false),
                    OrcamentoDetalheCompostoId = table.Column<Guid>(nullable: false),
                    Atributoid = table.Column<Guid>(nullable: false),
                    Opcaoid = table.Column<string>(maxLength: 64, nullable: false),
                    ValorAdicional = table.Column<decimal>(precision: 18, scale: 2, nullable: false, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_selecao", x => x.SelecaoId);
                    table.ForeignKey(
                        name: "FK_Selecao_Atributo",
                        column: x => x.Atributoid,
                        principalTable: "atributo",
                        principalColumn: "AtributoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Selecao_Detalhe",
                        column: x => x.OrcamentoDetalheCompostoId,
                        principalTable: "orcamentodetalhecomposto",
                        principalColumn: "OrcamentoDetalheCompostoId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Selecao_Opcao",
                        column: x => x.Opcaoid,
                        principalTable: "opcao",
                        principalColumn: "OpcaoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atributo_ParentAtributoId",
                table: "atributo",
                column: "ParentAtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_modelocomposto_hash",
                table: "modelocomposto",
                columns: new[] { "ConfiguracaoHash", "Aplicacaoid", "Produtoid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modeloselecao_ModeloCompostoId",
                table: "modeloselecao",
                column: "ModeloCompostoId");

            migrationBuilder.CreateIndex(
                name: "IX_opcao_AtributoId",
                table: "opcao",
                column: "AtributoId");

            migrationBuilder.CreateIndex(
                name: "IX_orcamentodetalhe_orcamentoid",
                table: "orcamentodetalhe",
                column: "orcamentoid");

            migrationBuilder.CreateIndex(
                name: "IX_orcamentodetalhecomposto_orcamentoid",
                table: "orcamentodetalhecomposto",
                column: "orcamentoid");

            migrationBuilder.CreateIndex(
                name: "IX_orcamentodetalhecomposto_Produtoid",
                table: "orcamentodetalhecomposto",
                column: "Produtoid");

            migrationBuilder.CreateIndex(
                name: "IX_publictoken_aplicacaoid",
                table: "publictoken",
                column: "aplicacaoid");

            migrationBuilder.CreateIndex(
                name: "UQ_publictoken_token",
                table: "publictoken",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_selecao_Atributoid",
                table: "selecao",
                column: "Atributoid");

            migrationBuilder.CreateIndex(
                name: "IX_selecao_Opcaoid",
                table: "selecao",
                column: "Opcaoid");

            migrationBuilder.CreateIndex(
                name: "IX_selecao_OrcamentoDetalheCompostoId",
                table: "selecao",
                column: "OrcamentoDetalheCompostoId");

            migrationBuilder.CreateIndex(
                name: "IX_statuspedido_pedidoid",
                table: "statuspedido",
                column: "pedidoid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aplicacao");

            migrationBuilder.DropTable(
                name: "areas");

            migrationBuilder.DropTable(
                name: "arquivo");

            migrationBuilder.DropTable(
                name: "cambio");

            migrationBuilder.DropTable(
                name: "categoria");

            migrationBuilder.DropTable(
                name: "ciaaerea");

            migrationBuilder.DropTable(
                name: "conteudo");

            migrationBuilder.DropTable(
                name: "conteudovalor");

            migrationBuilder.DropTable(
                name: "dict_blocos");

            migrationBuilder.DropTable(
                name: "dict_socialmedia");

            migrationBuilder.DropTable(
                name: "dict_templates");

            migrationBuilder.DropTable(
                name: "dictareas");

            migrationBuilder.DropTable(
                name: "faq");

            migrationBuilder.DropTable(
                name: "formulario");

            migrationBuilder.DropTable(
                name: "formularionew");

            migrationBuilder.DropTable(
                name: "grupo");

            migrationBuilder.DropTable(
                name: "ia_cache");

            migrationBuilder.DropTable(
                name: "ia_config");

            migrationBuilder.DropTable(
                name: "ia_uso");

            migrationBuilder.DropTable(
                name: "imagem");

            migrationBuilder.DropTable(
                name: "infofoto");

            migrationBuilder.DropTable(
                name: "informativo");

            migrationBuilder.DropTable(
                name: "layout_template");

            migrationBuilder.DropTable(
                name: "modeloselecao");

            migrationBuilder.DropTable(
                name: "modulo");

            migrationBuilder.DropTable(
                name: "moduloconf");

            migrationBuilder.DropTable(
                name: "moeda");

            migrationBuilder.DropTable(
                name: "newsletter");

            migrationBuilder.DropTable(
                name: "orcamentodetalhe");

            migrationBuilder.DropTable(
                name: "publictoken");

            migrationBuilder.DropTable(
                name: "refatributoxopcao");

            migrationBuilder.DropTable(
                name: "relatributoxproduto");

            migrationBuilder.DropTable(
                name: "relimagemconteudo");

            migrationBuilder.DropTable(
                name: "relmoduloaplicacao");

            migrationBuilder.DropTable(
                name: "relmoduloconfaplicacao");

            migrationBuilder.DropTable(
                name: "relmodulousuario");

            migrationBuilder.DropTable(
                name: "relusuarioaplicacao");

            migrationBuilder.DropTable(
                name: "relusuariogrupo");

            migrationBuilder.DropTable(
                name: "selecao");

            migrationBuilder.DropTable(
                name: "socialmedia");

            migrationBuilder.DropTable(
                name: "statuspedido");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "tipocotacao");

            migrationBuilder.DropTable(
                name: "tipoenvio");

            migrationBuilder.DropTable(
                name: "unidades");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "modelocomposto");

            migrationBuilder.DropTable(
                name: "orcamentodetalhecomposto");

            migrationBuilder.DropTable(
                name: "opcao");

            migrationBuilder.DropTable(
                name: "pedido");

            migrationBuilder.DropTable(
                name: "orcamentocabecalho");

            migrationBuilder.DropTable(
                name: "produto");

            migrationBuilder.DropTable(
                name: "atributo");
        }
    }
}
