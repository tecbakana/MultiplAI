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
    DashboardTotais TotaisGlobais();
    DashboardTotais TotaisPorAplicacao(string aplicacaoid);
}
