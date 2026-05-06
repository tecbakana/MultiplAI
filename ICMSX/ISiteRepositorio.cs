using CMSXData.Models;

namespace ICMSX;

public record ProdutoPublico(string Produtoid, string? Nome, string? Descricacurta, decimal? Valor, string? Imagem);

public interface ISiteRepositorio
{
    Aplicacao? BuscaPorSlug(string slug);
    IEnumerable<Area> ListaAreas(string aplicacaoid);
    IEnumerable<Conteudo> ListaConteudosPorArea(string areaid, int limite);
    IEnumerable<ProdutoPublico> ListaProdutos(string aplicacaoid, string? cateriaid, int limite);
    IEnumerable<Caterium> ListaCategorias(string aplicacaoid, string? cateriaidpai);
    IEnumerable<Faq> ListaFaqsAtivos(string formularioid);
    Formulario? BuscaFormulario(string formularioid);
    IEnumerable<Area> ListaAreasMenu(string aplicacaoid);
}
