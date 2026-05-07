using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class PageBuilderRepositorio : BaseRepositorio, IPageBuilderRepositorio
{
    public PageBuilderRepositorio(CmsxDbContext db) : base(db) { }

    public async Task<IEnumerable<DictBloco>> ListaBlocosAsync() =>
        await _db.DictBlocos.AsNoTracking().OrderBy(b => b.Nome).ToListAsync();

    public async Task<IEnumerable<string>> ListaTiposBlocosAsync() =>
        await _db.DictBlocos.AsNoTracking().Select(b => b.Tipobloco).ToListAsync();

    public async Task<IaConfig?> BuscaConfigAsync(string aplicacaoid) =>
        await _db.IaConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Aplicacaoid == aplicacaoid);

    public async Task SalvarConfigAsync(string aplicacaoid, string? provedor, string? modelo, int? limiteDiario, string? apikey)
    {
        var config = await _db.IaConfigs.FirstOrDefaultAsync(c => c.Aplicacaoid == aplicacaoid);
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

        await _db.SaveChangesAsync();
    }

    public async Task RemoverApiKeyAsync(string aplicacaoid)
    {
        var config = await _db.IaConfigs.FirstOrDefaultAsync(c => c.Aplicacaoid == aplicacaoid);
        if (config != null)
        {
            config.Apikey = null;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<int> BuscaUsoHojeAsync(string aplicacaoid, DateOnly data) =>
        await _db.IaUsos.AsNoTracking()
            .Where(u => u.Aplicacaoid == aplicacaoid && u.Data == data)
            .Select(u => u.Contador)
            .FirstOrDefaultAsync();

    public async Task<IaCache?> BuscaCacheAsync(string hash, DateTime agora) =>
        await _db.IaCaches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Hash == hash && c.Datavencimento > agora);

    public async Task RegistrarGeracaoAsync(IaCache cache, string? aplicacaoid, DateOnly? data, bool incrementarUso)
    {
        _db.IaCaches.Add(cache);

        if (incrementarUso && aplicacaoid != null && data.HasValue)
        {
            var uso = await _db.IaUsos.FirstOrDefaultAsync(u => u.Aplicacaoid == aplicacaoid && u.Data == data.Value);
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

        await _db.SaveChangesAsync();
    }
}
