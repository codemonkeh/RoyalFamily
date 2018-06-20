using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RoyalFamily.Common.Data.Entities;
using RoyalFamily.Common.Data.Models;
using RoyalFamily.Common.Data.Repositories;
using Newtonsoft.Json;

namespace RoyalFamily.Data
{    
    public class JsonDataImporter
    {
        private readonly IPersonRepository _personRepository;

        private class JsonModel
        {
            public string name;
            public string[] parents;
            public bool male;
            public bool royal;
            public string spouse;
        }

        /// <summary>
        /// Imports People test data from disk
        /// </summary>
        /// <param name="path">Path to json data file</param>
        /// <returns>A list of people saved into the database</returns>
        public async Task<IList<Person>> LoadPeopleFromJsonFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var jsonModels = await ParseFile(path);
            var people = await ConvertToPeople(jsonModels);

            return people;
        }

        public JsonDataImporter(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        private async Task<IList<JsonModel>> ParseFile(string filename)
        {
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename))
                throw new FileNotFoundException("Could not find data file", filename);

            var json = await File.ReadAllTextAsync(filename);
            if (json == null)
                throw new Exception($"No context found in data file \"{filename}\"");

            var models = JsonConvert.DeserializeObject<List<JsonModel>>(json);
            return models;
        }

        private async Task<IList<Person>> ConvertToPeople(IList<JsonModel> models)
        {
            if (models == null) throw new ArgumentNullException(nameof(models));

            var people = new List<Person>();
            foreach (var model in models)
            {
                var person = new Person
                {
                    Name = model.name,
                    Gender = model.male ? Gender.Male : Gender.Female,
                    IsRoyal = model.royal
                };                

                if (!string.IsNullOrWhiteSpace(model.spouse))
                {
                    // spouses should be first in the data file                    
                    person.Spouse = await FindByNameOrThrow(model.spouse);
                }
                await _personRepository.SaveAsync(person);

                if (model.parents != null && model.parents.Any())
                {
                    var parents = model.parents.Select(async name => await FindByNameOrThrow(name)).ToList();
                    var relationships = parents.Select(p =>
                        new FamilialRelationship
                        {
                            Parent = p.Result,
                            Child = person
                        }).ToList();
                    person.Parents = relationships;
                }

                // save it to ensure that it can be found be descendants
                await _personRepository.SaveAsync(person);
                people.Add(person);
            }

            return people;
        }

        private async Task<Person> FindByNameOrThrow(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            
            var person = await _personRepository.GetByNameAsync(name);
            if (person == null)
                throw new Exception($"Could not find person named \"{name}\"");

            return person;
        }
    }
}
