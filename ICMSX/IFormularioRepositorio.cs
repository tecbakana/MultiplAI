using CMSXData.Models;

namespace ICMSX;

public interface IFormularioRepositorio
{
    // ── Definições ──────────────────────────────────────────────────────────
    Task<IEnumerable<Formulario>> ListaDefsAsync(string? aplicacaoid, string? areaid);
    Task<Formulario?> BuscaDefPorIdAsync(string id);
    Task<string?> AplicacaoidDaAreaAsync(string? areaid);
    Task CriarDefAsync(Formulario item);
    Task AtualizarDefAsync(Formulario item);
    Task RemoverDefAsync(Formulario item);

    // ── Submissões públicas ─────────────────────────────────────────────────
    Task<Formulario?> BuscaFormularioPorIdAsync(string formularioid);
    Task SubmeterAsync(Formularionew item);

    // ── Respostas (inbox) ────────────────────────────────────────────────────
    Task<IEnumerable<Formularionew>> ListaRespostasAsync(string? aplicacaoid);
    Task<Formularionew?> BuscaRespostaPorIdAsync(int id);
    Task<string?> AplicacaoidDaRespostaAsync(string? formularioid);
    Task AtualizarRespostaAtivoAsync(Formularionew item);
    Task RemoverRespostaAsync(Formularionew item);
}
