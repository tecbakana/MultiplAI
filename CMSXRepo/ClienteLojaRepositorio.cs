using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class ClienteLojaRepositorio : BaseRepositorio, IClienteLojaRepositorio
{
    public ClienteLojaRepositorio(CmsxDbContext db) : base(db) { }

    public async Task CriaClienteLojaAsync(ICMSX.IClienteLojaRepositorio.CriaClienteLojaInput cliente)
    {
        var clienteLoja = new ClienteLoja
        {
            ClienteLojaid = Guid.NewGuid().ToString(),
            Aplicacaoid = cliente.Aplicacaoid,
            SalematicClienteId = cliente.salematicClienteId
        };

        _db.Add(clienteLoja);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<ClienteLoja>> ListaClienteLojaAsync() =>
        await _db.ClienteLojas.AsNoTracking().ToListAsync();
}
