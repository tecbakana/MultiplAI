using CMSXData.Models;

namespace ICMSX;

public interface IFormularioRepositorio
{
    // ── Definições ──────────────────────────────────────────────────────────
    IEnumerable<Formulario> ListaDefs(string? aplicacaoid, string? areaid);
    Formulario? BuscaDefPorId(string id);
    string? AplicacaoidDaArea(string? areaid);
    void CriarDef(Formulario item);
    void AtualizarDef(Formulario item);
    void RemoverDef(Formulario item);

    // ── Submissões públicas ─────────────────────────────────────────────────
    Formulario? BuscaFormularioPorId(string formularioid);
    void Submeter(Formularionew item);

    // ── Respostas (inbox) ────────────────────────────────────────────────────
    IEnumerable<Formularionew> ListaRespostas(string? aplicacaoid);
    Formularionew? BuscaRespostaPorId(int id);
    string? AplicacaoidDaResposta(string? formularioid);
    void AtualizarRespostaAtivo(Formularionew item);
    void RemoverResposta(Formularionew item);
}
