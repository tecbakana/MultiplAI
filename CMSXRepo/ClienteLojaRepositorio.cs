using System.Dynamic;
using CMSXData.Models;
using ICMSX;

namespace CMSXRepo;

public class ClienteLojaRepositorio : BaseRepositorio, IClienteLojaRepositorio
{
    private readonly IClienteLojaDAL _dal;

    public ClienteLojaRepositorio(CmsxDbContext db, IClienteLojaDAL dal) : base(db) { _dal = dal; }

    public Task MakeConnectionAsync(dynamic prop) { _dal.MakeConnection((ExpandoObject)prop); return Task.CompletedTask; }

    public Task CriaClienteLojaAsync(ClienteLoja cliente)
    {
        _dal.CriaClienteLoja(
            Guid.NewGuid(),
            Guid.TryParse(cliente.Aplicacaoid, out var aid) ? aid : Guid.Empty,
            cliente.SalematicClienteId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ClienteLoja>> ListaClienteLojaAsync() =>
        Task.FromResult(_dal.ListaClienteLoja());
}
