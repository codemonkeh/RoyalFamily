using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoyalFamily.Common.Data.Repositories
{
    public interface IRepository<TEntity, in TKey> where TEntity : class
    {
        Task<TEntity> GetAsync(TKey key);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<int> SaveAsync(TEntity entity);
        Task<int> DeleteAsync(TEntity entity);
    }
}