using CMSXData.Models;

namespace ICMSX;

public interface IFaqRepositorio
{
    // ── Wiki pública ─────────────────────────────────────────────────────────
    IEnumerable<Caterium> ListaCategoriasPorApp(string aplicacaoid);
    IEnumerable<Formulario> ListaFormulariosComCategoria(string aplicacaoid);
    IEnumerable<Faq> ListaFaqsAtivos(IEnumerable<string> formularioIds);

    // ── CRUD ─────────────────────────────────────────────────────────────────
    IEnumerable<Faq> ListaPorFormulario(string formularioid);
    Faq? BuscaPorId(string id);
    bool TemAcesso(string? formularioid, string? claimAppId);
    void Criar(Faq item);
    void Atualizar(Faq item);
    void Remover(Faq item);

    // ── Promoção de resposta inbox → FAQ ─────────────────────────────────────
    Formularionew? BuscaRespostaInbox(int idform);
}
