using System.Reflection;
using FluentAssertions;
using ICMSX;

namespace CMSX.Tests.Architecture;

/// <summary>
/// Verifica que os repositórios em CMSXRepo respeitam as regras do CLAUDE.md:
/// - toda classe que implementa interface de ICMSX herda BaseRepositorio
/// - nenhum método público retorna IQueryable (vazamento de estrutura)
/// </summary>
public class RepositoryArchitectureTests
{
    private static readonly Assembly RepoAssembly    = typeof(CMSXRepo.BaseRepositorio).Assembly;
    private static readonly Assembly IcmsxAssembly   = typeof(ILojaRepositorio).Assembly;

    private static IEnumerable<Type> InterfacesIcmsx() =>
        IcmsxAssembly.GetTypes().Where(t => t.IsInterface);

    private static IEnumerable<Type> Repositorios() =>
        RepoAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                        && t.GetInterfaces().Any(i => InterfacesIcmsx().Contains(i)));

    [Fact]
    public void Repositorios_Devem_HerdarBaseRepositorio()
    {
        var violacoes = Repositorios()
            .Where(t => !typeof(CMSXRepo.BaseRepositorio).IsAssignableFrom(t))
            .Select(t => t.Name)
            .ToList();

        violacoes.Should().BeEmpty(
            because: "todo repositório que implementa interface ICMSX deve herdar BaseRepositorio");
    }

    [Fact]
    public void Repositorios_NaoDevem_Expor_IQueryable()
    {
        var violacoes = Repositorios()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => IsIQueryable(m.ReturnType))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name} retorna {m.ReturnType.Name}")
            .ToList();

        violacoes.Should().BeEmpty(
            because: "repositórios não devem expor IQueryable — retorne IEnumerable ou tipo concreto");
    }

    [Fact]
    public void Repositorios_Devem_TerConstrutor_ComCmsxDbContext()
    {
        var violacoes = Repositorios()
            .Where(t => !t.GetConstructors()
                .Any(ctor => ctor.GetParameters()
                    .Any(p => p.ParameterType == typeof(CMSXData.Models.CmsxDbContext))))
            .Select(t => t.Name)
            .ToList();

        violacoes.Should().BeEmpty(
            because: "repositórios devem receber CmsxDbContext via construtor (injetado pelo BaseRepositorio)");
    }

    private static bool IsIQueryable(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>)) return true;
        if (type == typeof(IQueryable)) return true;
        return false;
    }
}
