using CMSXData.Models;

namespace ICMSX;

public interface IFaqRepositorio
{
    // ── Wiki pública ─────────────────────────────────────────────────────────
    Task<IEnumerable<Caterium>> ListaCategoriasPorAppAsync(string aplicacaoid);
    Task<IEnumerable<Formulario>> ListaFormulariosComCategoriaAsync(string aplicacaoid);
    Task<IEnumerable<Faq>> ListaFaqsAtivosAsync(IEnumerable<string> formularioIds);

    // ── CRUD ─────────────────────────────────────────────────────────────────
    Task<IEnumerable<Faq>> ListaPorFormularioAsync(string formularioid);
    Task<Faq?> BuscaPorIdAsync(string id);
    Task<bool> TemAcessoAsync(string? formularioid, string? claimAppId);
    Task CriarAsync(Faq item);
    Task AtualizarAsync(Faq item);
    Task RemoverAsync(Faq item);

    // ── Promoção de resposta inbox → FAQ ─────────────────────────────────────
    Task<Formularionew?> BuscaRespostaInboxAsync(int idform);
}
