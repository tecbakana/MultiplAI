using CMSXData.Models;

namespace ICMSX;

public interface IGrupoRepositorio
{
    Task<IEnumerable<Grupo>> ListaAsync();
    Task<Grupo?> BuscaPorIdAsync(string id);
    Task<IEnumerable<object>> UsuariosPorGrupoAsync(string grupoid);
    Task<bool> ExisteVinculoUsuarioAsync(string grupoid, string usuarioid);
    Task CriarAsync(Grupo grupo);
    Task AtualizarAsync(Grupo grupo);
    Task RemoverComVinculosAsync(Grupo grupo);
    Task AdicionarUsuarioAsync(Relusuariogrupo rel);
    Task<Relusuariogrupo?> BuscaVinculoPorRelacaoidAsync(string relacaoid);
    Task RemoverVinculoUsuarioAsync(Relusuariogrupo rel);
}
