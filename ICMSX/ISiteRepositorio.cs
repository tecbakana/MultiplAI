using CMSXData.Models;

namespace ICMSX;

public record ProdutoPublico(string Produtoid, string? Nome, string? Descricacurta, decimal? Valor, string? Imagem);

public interface ISiteRepositorio
{
    Task<Aplicacao?> BuscaPorSlugAsync(string slug);
    Task<IEnumerable<Area>> ListaAreasAsync(string aplicacaoid);
    Task<IEnumerable<Conteudo>> ListaConteudosPorAreaAsync(string areaid, int limite);
    Task<IEnumerable<ProdutoPublico>> ListaProdutosAsync(string aplicacaoid, string? cateriaid, int limite);
    Task<IEnumerable<Caterium>> ListaCategoriasAsync(string aplicacaoid, string? cateriaidpai);
    Task<IEnumerable<Faq>> ListaFaqsAtivosAsync(string formularioid);
    Task<Formulario?> BuscaFormularioAsync(string formularioid);
    Task<IEnumerable<Area>> ListaAreasMenuAsync(string aplicacaoid);
}
