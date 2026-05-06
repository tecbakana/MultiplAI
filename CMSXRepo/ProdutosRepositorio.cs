using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ProdutosRepositorio : BaseRepositorio, IProdutosRepositorio
{
    public ProdutosRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<Produto>> ListaAsync(string? aplicacaoid)
    {
        var q = _db.Produtos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(aplicacaoid))
            q = q.Where(p => p.Aplicacaoid == aplicacaoid);
        return await q.OrderBy(p => p.Nome).ToListAsync();
    }

    public async Task<Produto?> BuscaPorIdAsync(string id) =>
        await _db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Produtoid == id);

    public async Task CriarAsync(Produto produto)
    {
        _db.Produtos.Add(produto);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Produto produto)
    {
        _db.Produtos.Update(produto);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAsync(Produto produto)
    {
        _db.Produtos.Remove(produto);
        await _db.SaveChangesAsync();
    }

    public async Task<ArvoreAtributos> BuscaArvoreComOpcoesAsync(string produtoid)
    {
        var todos = await _db.Atributos
            .AsNoTracking()
            .Where(a => a.Produtoid == produtoid)
            .ToListAsync();

        var idsConhecidos = todos.Select(a => a.Atributoid).ToHashSet();
        bool achouMais;
        do
        {
            achouMais = false;
            var idsLista = idsConhecidos.ToList();
            var novos = (await _db.Atributos
                .AsNoTracking()
                .Where(a => a.ParentAtributoId.HasValue && idsLista.Contains(a.ParentAtributoId.Value))
                .ToListAsync())
                .Where(a => !idsConhecidos.Contains(a.Atributoid))
                .ToList();
            if (novos.Count > 0)
            {
                todos.AddRange(novos);
                foreach (var n in novos) idsConhecidos.Add(n.Atributoid);
                achouMais = true;
            }
        } while (achouMais);

        var idsParaOpcoes = idsConhecidos.ToList();
        var opcoes = await _db.Opcaos
            .AsNoTracking()
            .Where(o => idsParaOpcoes.Contains(o.Atributoid))
            .ToListAsync();

        return new ArvoreAtributos(todos, opcoes);
    }

    public async Task<Atributo?> BuscaAtributoAsync(Guid id) =>
        await _db.Atributos.AsNoTracking().FirstOrDefaultAsync(a => a.Atributoid == id);

    public async Task CriarAtributoAsync(Atributo atributo)
    {
        _db.Atributos.Add(atributo);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAtributoAsync(Atributo atributo)
    {
        _db.Atributos.Update(atributo);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverAtributoComDescendentesAsync(Guid id)
    {
        var todosIds = new List<Guid> { id };
        var queue = new Queue<Guid>();
        queue.Enqueue(id);
        while (queue.Count > 0)
        {
            var pid = queue.Dequeue();
            var filhos = await _db.Atributos
                .Where(x => x.ParentAtributoId == pid)
                .Select(x => x.Atributoid)
                .ToListAsync();
            foreach (var fid in filhos) { todosIds.Add(fid); queue.Enqueue(fid); }
        }

        _db.Opcaos.RemoveRange(await _db.Opcaos.Where(o => todosIds.Contains(o.Atributoid)).ToListAsync());
        _db.Atributos.RemoveRange(await _db.Atributos.Where(x => todosIds.Contains(x.Atributoid)).ToListAsync());
        await _db.SaveChangesAsync();
    }

    public async Task<Opcao?> BuscaOpcaoAsync(string opcaoid, Guid atributoid) =>
        await _db.Opcaos.AsNoTracking().FirstOrDefaultAsync(x => x.Opcaoid == opcaoid && x.Atributoid == atributoid);

    public async Task CriarOpcaoAsync(Opcao opcao)
    {
        _db.Opcaos.Add(opcao);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarOpcaoAsync(Opcao opcao)
    {
        _db.Opcaos.Update(opcao);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverOpcaoAsync(Opcao opcao)
    {
        _db.Opcaos.Remove(opcao);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Imagem>> ListaImagensPorProdutoAsync(string produtoid) =>
        await _db.Imagems.AsNoTracking()
            .Where(i => i.Parentid == produtoid && i.Tipoid == "produto")
            .ToListAsync();

    public async Task<Imagem?> BuscaImagemAsync(string imagemid, string produtoid) =>
        await _db.Imagems.AsNoTracking().FirstOrDefaultAsync(i => i.Imagemid == imagemid && i.Parentid == produtoid);

    public async Task CriarImagemAsync(Imagem imagem)
    {
        _db.Imagems.Add(imagem);
        await _db.SaveChangesAsync();
    }

    public async Task RemoverImagemAsync(Imagem imagem)
    {
        _db.Imagems.Remove(imagem);
        await _db.SaveChangesAsync();
    }
}
