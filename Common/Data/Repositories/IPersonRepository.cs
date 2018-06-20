using System.Threading.Tasks;
using RoyalFamily.Common.Data.Entities;

namespace RoyalFamily.Common.Data.Repositories
{    
    public interface IPersonRepository : IRepository<Person, int>
    {
        Task<Person> GetByIdAsync(int id);
        Task<Person> GetByNameAsync(string name);
        Task<Person> GetSpouse(Person person);
    }
}