using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ProdutosRepositorio : BaseRepositorio, IProdutosRepositorio
{
    public ProdutosRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<Produto> Lista(string? aplicacaoid)
    {
        var q = _db.Produtos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(aplicacaoid))
            q = q.Where(p => p.Aplicacaoid == aplicacaoid);
        return q.OrderBy(p => p.Nome).ToList();
    }

    public Produto? BuscaPorId(string id) =>
        _db.Produtos.AsNoTracking().FirstOrDefault(p => p.Produtoid == id);

    public void Criar(Produto produto)
    {
        _db.Produtos.Add(produto);
        _db.SaveChanges();
    }

    public void Atualizar(Produto produto)
    {
        _db.Produtos.Update(produto);
        _db.SaveChanges();
    }

    public void Remover(Produto produto)
    {
        _db.Produtos.Remove(produto);
        _db.SaveChanges();
    }

    public ArvoreAtributos BuscaArvoreComOpcoes(string produtoid)
    {
        var todos = _db.Atributos
            .AsNoTracking()
            .Where(a => a.Produtoid == produtoid)
            .ToList();

        var idsConhecidos = todos.Select(a => a.Atributoid).ToHashSet();
        bool achouMais;
        do
        {
            achouMais = false;
            var idsLista = idsConhecidos.ToList();
            var novos = _db.Atributos
                .AsNoTracking()
                .Where(a => a.ParentAtributoId.HasValue && idsLista.Contains(a.ParentAtributoId.Value))
                .ToList()
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
        var opcoes = _db.Opcaos
            .AsNoTracking()
            .Where(o => idsParaOpcoes.Contains(o.Atributoid))
            .ToList();

        return new ArvoreAtributos(todos, opcoes);
    }

    public Atributo? BuscaAtributo(Guid id) =>
        _db.Atributos.AsNoTracking().FirstOrDefault(a => a.Atributoid == id);

    public void CriarAtributo(Atributo atributo)
    {
        _db.Atributos.Add(atributo);
        _db.SaveChanges();
    }

    public void AtualizarAtributo(Atributo atributo)
    {
        _db.Atributos.Update(atributo);
        _db.SaveChanges();
    }

    public void RemoverAtributoComDescendentes(Guid id)
    {
        var todosIds = new List<Guid> { id };
        var queue = new Queue<Guid>();
        queue.Enqueue(id);
        while (queue.Count > 0)
        {
            var pid = queue.Dequeue();
            var filhos = _db.Atributos
                .Where(x => x.ParentAtributoId == pid)
                .Select(x => x.Atributoid)
                .ToList();
            foreach (var fid in filhos) { todosIds.Add(fid); queue.Enqueue(fid); }
        }

        _db.Opcaos.RemoveRange(_db.Opcaos.Where(o => todosIds.Contains(o.Atributoid)).ToList());
        _db.Atributos.RemoveRange(_db.Atributos.Where(x => todosIds.Contains(x.Atributoid)).ToList());
        _db.SaveChanges();
    }

    public Opcao? BuscaOpcao(string opcaoid, Guid atributoid) =>
        _db.Opcaos.AsNoTracking().FirstOrDefault(x => x.Opcaoid == opcaoid && x.Atributoid == atributoid);

    public void CriarOpcao(Opcao opcao)
    {
        _db.Opcaos.Add(opcao);
        _db.SaveChanges();
    }

    public void AtualizarOpcao(Opcao opcao)
    {
        _db.Opcaos.Update(opcao);
        _db.SaveChanges();
    }

    public void RemoverOpcao(Opcao opcao)
    {
        _db.Opcaos.Remove(opcao);
        _db.SaveChanges();
    }

    public IEnumerable<Imagem> ListaImagensPorProduto(string produtoid) =>
        _db.Imagems.AsNoTracking()
            .Where(i => i.Parentid == produtoid && i.Tipoid == "produto")
            .ToList();

    public Imagem? BuscaImagem(string imagemid, string produtoid) =>
        _db.Imagems.AsNoTracking().FirstOrDefault(i => i.Imagemid == imagemid && i.Parentid == produtoid);

    public void CriarImagem(Imagem imagem)
    {
        _db.Imagems.Add(imagem);
        _db.SaveChanges();
    }

    public void RemoverImagem(Imagem imagem)
    {
        _db.Imagems.Remove(imagem);
        _db.SaveChanges();
    }
}
