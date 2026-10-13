using Dapper;
using System.Data;
using System.Data.Common;

namespace LeaseBackend.DAL.DapperServices
{
    public interface IDapperService
    {
       Task<T?> Get<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<IEnumerable<T>> GetAll<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task<T?> Insert<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        Task Update<T>(string sp, DynamicParameters parms, CommandType commandType = CommandType.StoredProcedure);
        DbConnection GetDbconnection();
    }
}