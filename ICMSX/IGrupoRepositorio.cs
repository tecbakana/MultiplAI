using CMSXData.Models;

namespace ICMSX;

public interface IGrupoRepositorio
{
    IEnumerable<Grupo> Lista();
    Grupo? BuscaPorId(string id);
    IEnumerable<object> UsuariosPorGrupo(string grupoid);
    bool ExisteVinculoUsuario(string grupoid, string usuarioid);
    void Criar(Grupo grupo);
    void Atualizar(Grupo grupo);
    void RemoverComVinculos(Grupo grupo);
    void AdicionarUsuario(Relusuariogrupo rel);
    Relusuariogrupo? BuscaVinculoPorRelacaoid(string relacaoid);
    void RemoverVinculoUsuario(Relusuariogrupo rel);
}
