using System.Dynamic;
using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class AtributoRepositorio : BaseRepositorio, IAtributoRepositorio
{
    private readonly IAtributoDAL _dal;

    public AtributoRepositorio(CmsxDbContext db, IAtributoDAL dal) : base(db) { _dal = dal; }

    public Task MakeConnectionAsync(dynamic prop) { _dal.MakeConnection((ExpandoObject)prop); return Task.CompletedTask; }

    public Task<List<Atributo>> ListaAtributoAsync() => throw new NotImplementedException();
    public Task<List<Atributo>> ListaAtributoXProdutoAsync() => throw new NotImplementedException();
    public Task CriaAtributoAsync(Atributo at) => throw new NotImplementedException();
    public Task InativaAtributoAsync() => throw new NotImplementedException();

    public async Task<List<Atributo>> ListaAtributosArvoreAsync(IEnumerable<string> produtoIds)
    {
        var produtoList = produtoIds.ToList();
        var todos = new List<Atributo>();

        var raizes = await _db.Atributos
            .Where(a => a.Produtoid != null && produtoList.Contains(a.Produtoid))
            .ToListAsync();

        todos.AddRange(raizes);

        var idsParaBuscar = raizes.Select(a => a.Atributoid).ToList();
        while (idsParaBuscar.Count > 0)
        {
            var filhos = await _db.Atributos
                .Where(a => a.ParentAtributoId.HasValue && idsParaBuscar.Contains(a.ParentAtributoId.Value))
                .ToListAsync();

            if (filhos.Count == 0) break;

            todos.AddRange(filhos);
            idsParaBuscar = filhos.Select(a => a.Atributoid).ToList();
        }

        return todos;
    }
}
