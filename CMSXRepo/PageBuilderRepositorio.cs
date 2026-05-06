using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class PageBuilderRepositorio : BaseRepositorio, IPageBuilderRepositorio
{
    public PageBuilderRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<DictBloco> ListaBlocos() =>
        _db.DictBlocos.AsNoTracking().OrderBy(b => b.Nome).ToList();

    public IEnumerable<string> ListaTiposBlocos() =>
        _db.DictBlocos.AsNoTracking().Select(b => b.Tipobloco).ToList();

    public IaConfig? BuscaConfig(string aplicacaoid) =>
        _db.IaConfigs.AsNoTracking().FirstOrDefault(c => c.Aplicacaoid == aplicacaoid);

    public void SalvarConfig(string aplicacaoid, string? provedor, string? modelo, int? limiteDiario, string? apikey)
    {
        var config = _db.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == aplicacaoid);
        if (config == null)
        {
            config = new IaConfig { Aplicacaoid = aplicacaoid };
            _db.IaConfigs.Add(config);
        }

        config.Provedor = provedor;
        config.Modelo = modelo;
        config.LimiteDiario = limiteDiario;

        if (!string.IsNullOrWhiteSpace(apikey))
            config.Apikey = apikey;

        _db.SaveChanges();
    }

    public void RemoverApiKey(string aplicacaoid)
    {
        var config = _db.IaConfigs.FirstOrDefault(c => c.Aplicacaoid == aplicacaoid);
        if (config != null)
        {
            config.Apikey = null;
            _db.SaveChanges();
        }
    }

    public int BuscaUsoHoje(string aplicacaoid, DateOnly data) =>
        _db.IaUsos.AsNoTracking()
            .Where(u => u.Aplicacaoid == aplicacaoid && u.Data == data)
            .Select(u => u.Contador)
            .FirstOrDefault();

    public IaCache? BuscaCache(string hash, DateTime agora) =>
        _db.IaCaches.AsNoTracking()
            .FirstOrDefault(c => c.Hash == hash && c.Datavencimento > agora);

    public void RegistrarGeracao(IaCache cache, string? aplicacaoid, DateOnly? data, bool incrementarUso)
    {
        _db.IaCaches.Add(cache);

        if (incrementarUso && aplicacaoid != null && data.HasValue)
        {
            var uso = _db.IaUsos.FirstOrDefault(u => u.Aplicacaoid == aplicacaoid && u.Data == data.Value);
            if (uso == null)
                _db.IaUsos.Add(new IaUso
                {
                    Usoid = Guid.NewGuid().ToString(),
                    Aplicacaoid = aplicacaoid,
                    Data = data.Value,
                    Contador = 1
                });
            else
                uso.Contador++;
        }

        _db.SaveChanges();
    }
}
