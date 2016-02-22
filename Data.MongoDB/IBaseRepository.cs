using System.Collections.Generic;
using System.Threading.Tasks;

using MongoDB.Driver;

namespace Data.MongoDB
{
    public interface IBaseRepository<TEntity, in TId>
    {
        IMongoCollection<TEntity> Collection { get; }

        Task<TEntity> GetById(TId id);

        Task<TEntity> Add(TEntity entity);

        Task<TEntity> Update(TEntity entity);

        Task<IEnumerable<TEntity>> GetAll();
        
        Task Delete(TId id);
        
    }
}