using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoyalFamily.Common.Data.Entities;
using RoyalFamily.Common.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RoyalFamily.Data.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly DataContext _context;

        public PersonRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Person> GetAsync(int id)
        {
            if (id <= 0) throw new IndexOutOfRangeException($"{nameof(id)} must be greater than zero");

            return await _context.People.SingleOrDefaultAsync(x => x.Id == id);
        }

        // For this example the name is assumed to be sufficiently unique
        public async Task<Person> GetByIdAsync(int parentId)
        {
            return await _context.People
                .Include(p => p.Parents)
                .SingleOrDefaultAsync(x => x.Id == parentId);
        }

        public async Task<Person> GetByNameAsync(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return await _context.People
                .Include(p => p.Parents)                
                .SingleOrDefaultAsync(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public async Task<Person> GetSpouse(Person person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));

            return person.Spouse ?? await _context.People
                       .Include(p => p.Parents)
                       .SingleOrDefaultAsync(x => x.SpouseId == person.Id);
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.People.OrderBy(x => x.Name).ToListAsync();
        }

        public async Task<int> SaveAsync(Person person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));

            if (person.Id == 0)
                await _context.AddAsync(person);
            else 
                _context.Update(person);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(Person person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));
            _context.People.Remove(person);
            return await _context.SaveChangesAsync();
        }
    }
}
