using System.Reflection;
using CMSXData.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CMSX.Tests.Architecture;

/// <summary>
/// Verifica que nenhum controller viola as regras de camada definidas no CLAUDE.md:
/// - construtores injetam apenas interfaces (nunca CmsxDbContext ou classes concretas de repositório)
/// - classes herdam de ControllerBase
/// </summary>
public class ControllerArchitectureTests
{
    private static readonly Assembly ControllerAssembly = typeof(CMSAPI.TestAnchor).Assembly;

    private static IEnumerable<Type> Controllers() =>
        ControllerAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t));

    [Fact]
    public void Controllers_NaoDevem_InjetarCmsxDbContext()
    {
        var violacoes = Controllers()
            .SelectMany(c => c.GetConstructors())
            .SelectMany(ctor => ctor.GetParameters())
            .Where(p => p.ParameterType == typeof(CmsxDbContext))
            .Select(p => p.Member.DeclaringType!.Name)
            .ToList();

        violacoes.Should().BeEmpty(
            because: "controllers não podem injetar CmsxDbContext diretamente — use interfaces de ICMSX");
    }

    [Fact]
    public void Controllers_NaoDevem_InjetarClassesConcretas_DeRepositorio()
    {
        var tiposRepositorio = typeof(CMSXRepo.BaseRepositorio).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                        && typeof(CMSXRepo.BaseRepositorio).IsAssignableFrom(t))
            .ToHashSet();

        var violacoes = Controllers()
            .SelectMany(c => c.GetConstructors())
            .SelectMany(ctor => ctor.GetParameters())
            .Where(p => tiposRepositorio.Contains(p.ParameterType))
            .Select(p => $"{p.Member.DeclaringType!.Name} injeta {p.ParameterType.Name}")
            .ToList();

        violacoes.Should().BeEmpty(
            because: "controllers devem depender de interfaces ICMSX, não de implementações concretas");
    }

    [Fact]
    public void Controllers_Devem_InjetarApenasInterfaces_OuTiposDoFramework()
    {
        var namespacesPermitidos = new[]
        {
            "ICMSX",
            "CMSAPI.Services.I",
            "Microsoft.",
            "System.",
            "Serilog.",
            "Azure."
        };

        var violacoes = Controllers()
            .SelectMany(c => c.GetConstructors()
                .SelectMany(ctor => ctor.GetParameters()
                    .Where(p =>
                    {
                        var t = p.ParameterType;
                        if (t.IsInterface) return false;
                        var ns = t.Namespace ?? "";
                        return !namespacesPermitidos.Any(perm => ns.StartsWith(perm));
                    })
                    .Select(p => $"{c.Name} injeta {p.ParameterType.FullName}")))
            .ToList();

        violacoes.Should().BeEmpty(
            because: "controllers só podem receber interfaces ou tipos do framework ASP.NET/Azure/System");
    }
}
