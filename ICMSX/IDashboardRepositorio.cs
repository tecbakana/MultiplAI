namespace ICMSX;

public record DashboardTotais(
    int Usuarios,
    int Aplicacoes,
    int Conteudos,
    int Areas,
    int Categorias,
    int Modulos);

public interface IDashboardRepositorio
{
    Task<DashboardTotais> TotaisGlobaisAsync();
    Task<DashboardTotais> TotaisPorAplicacaoAsync(string aplicacaoid);
}
