using System.Linq;
using System.Threading.Tasks;
using RoyalFamily.Common.Data.Entities;
using RoyalFamily.Common.Data.Models;
using RoyalFamily.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace RoyalFamily.Data.Tests
{
    public class PersonRepositoryTests
    {
        public PersonRepositoryTests()
        {            
        }

        private DataContext GetNewTestDataContext()
        {
            var builder = new DbContextOptionsBuilder<DataContext>();
            builder.UseInMemoryDatabase("RoyalFamily");
            return new DataContext(builder.Options);
        }

        [Fact]
        public async Task SaveAsync_CreateAndSaveNewPerson_IdIsIncremented()
        {            
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var repository = new PersonRepository(context);
                var person = new Person
                {
                    Id = 0,
                    Name = "Morty",
                    Gender = Gender.Male
                };

                // act
                await repository.SaveAsync(person);

                // assert
                Assert.Equal(1, person.Id);
            }
        }

        [Fact]
        public async Task SaveAsync_CreatePersonWithParents_ParentsAreAssociated()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var repository = new PersonRepository(context);
                var child = new Person
                {
                    Name = "Morty",
                    Gender = Gender.Male
                };
                var parent1 = new Person
                {
                    Name = "Beth",
                    Gender = Gender.Female
                };
                var parent2 = new Person
                {
                    Name = "Jerry",
                    Gender = Gender.Male
                };

                // act
                await repository.SaveAsync(child);
                await repository.SaveAsync(parent1);
                await repository.SaveAsync(parent2);
                child.Parents.Add(new FamilialRelationship { Child = child, Parent = parent1 });
                child.Parents.Add(new FamilialRelationship { Child = child, Parent = parent2 });
                await repository.SaveAsync(child);

                var newChild = await repository.GetAsync(child.Id);

                // assert
                Assert.True(parent1.Id > 0);
                Assert.True(parent2.Id > 0);
                Assert.NotNull(newChild.Parents);
                Assert.Equal(2, newChild.Parents.Count);
                var parentRelationship1 = newChild.Parents.SingleOrDefault(x => x.ParentId == parent1.Id);
                var parentRelationship2 = newChild.Parents.SingleOrDefault(x => x.ParentId == parent2.Id);
                Assert.NotNull(parentRelationship1);
                Assert.NotNull(parentRelationship1.Parent);
                Assert.Equal(parent1.Id, parentRelationship1.Parent.Id);
                Assert.NotNull(parentRelationship2);
                Assert.NotNull(parentRelationship2.Parent);
                Assert.Equal(parent2.Id, parentRelationship2.Parent.Id);
            }
        }

        [Fact]
        public async Task SaveAsync_CreatePersonWithChildren_ChildrenAreAssociated()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var repository = new PersonRepository(context);                
                var parent = new Person
                {
                    Name = "Beth",
                    Gender = Gender.Female
                };
                var child1 = new Person
                {
                    Name = "Morty",
                    Gender = Gender.Male
                };
                var child2 = new Person
                {
                    Name = "Summer",
                    Gender = Gender.Female
                };

                // act
                await repository.SaveAsync(parent);
                await repository.SaveAsync(child1);
                await repository.SaveAsync(child2);
                parent.Children.Add(new FamilialRelationship { Parent = parent, Child = child1 });
                parent.Children.Add(new FamilialRelationship { Parent = parent, Child = child2 });
                await repository.SaveAsync(parent);

                var savedParent = await repository.GetAsync(parent.Id);

                // assert
                Assert.True(child1.Id > 0);
                Assert.True(child2.Id > 0);
                Assert.NotNull(savedParent.Children);
                Assert.Equal(2, savedParent.Children.Count);
                var childRelationship1 = savedParent.Children.SingleOrDefault(x => x.ChildId == child1.Id);
                var childRelationship2 = savedParent.Children.SingleOrDefault(x => x.ChildId == child2.Id);
                Assert.NotNull(childRelationship1);
                Assert.NotNull(childRelationship1.Child);
                Assert.Equal(child1.Id, childRelationship1.Child.Id);
                Assert.NotNull(childRelationship2);
                Assert.NotNull(childRelationship2.Child);
                Assert.Equal(child2.Id, childRelationship2.Child.Id);
            }
        }

        [Fact]
        public async Task SaveAsync_CreatePersonWithSpouse_SpouseIsAssociated()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var repository = new PersonRepository(context);               
                var person1 = new Person
                {
                    Name = "Beth",
                    Gender = Gender.Female,
                    IsRoyal = true
                };
                var person2 = new Person
                {
                    Name = "Jerry",
                    Gender = Gender.Male
                };

                // act
                await repository.SaveAsync(person1);
                await repository.SaveAsync(person2);
                person1.Spouse = person2;
                await repository.SaveAsync(person1);

                var newPerson = await repository.GetAsync(person1.Id);

                // assert
                Assert.NotNull(newPerson);
                Assert.Equal(person1.Id, newPerson.Id);
                Assert.NotNull(newPerson.Spouse);
                Assert.Equal(person2.Id, newPerson.SpouseId);
                Assert.Null(newPerson.Spouse.Spouse);
            }
        }

        [Fact]
        public async Task SaveAsync_UpdatePersonName_NameIsChanged()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var originalName = "Morty";
                var newName = "Rick";
                var repository = new PersonRepository(context);
                var person = new Person
                {
                    Name = originalName,
                    Gender = Gender.Male
                };

                // act
                await repository.SaveAsync(person);
                person.Name = newName;
                await repository.SaveAsync(person);

                // assert
                Assert.Equal(newName, person.Name);
            }
        }

        [Fact]
        public async Task GetByNameAsync_FindASpecificPersonByName_PersonIsFound()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var name = "Beth";
                var repository = new PersonRepository(context);
                var person = new Person
                {
                    Name = name,
                    Gender = Gender.Female
                };

                // act
                await repository.SaveAsync(person);
                var foundPerson = await repository.GetByNameAsync(name);

                // assert
                Assert.NotNull(foundPerson);
                Assert.Equal(name, foundPerson.Name);
                Assert.Equal(person.Id, foundPerson.Id);
            }
        }

        [Fact]
        public async Task GetSpouse_SpouseIsDirectlyAssociated_SpouseIsReturned()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var repository = new PersonRepository(context);
                var person1 = new Person
                {
                    Name = "Beth",
                    Gender = Gender.Female,
                    IsRoyal = true
                };
                var person2 = new Person
                {
                    Name = "Jerry",
                    Gender = Gender.Male
                };

                // act
                await repository.SaveAsync(person1);
                await repository.SaveAsync(person2);
                person1.Spouse = person2;
                await repository.SaveAsync(person1);

                var spouse = await repository.GetSpouse(person1);

                // assert                
                Assert.NotNull(spouse);
                Assert.Equal(person2.Id, spouse.Id);
            }
        }

        [Fact]
        public async Task GetSpouse_SpouseIsNotDirectlyAssociated_SpouseIsReturned()
        {
            using (var context = GetNewTestDataContext())
            {
                // arrange
                var repository = new PersonRepository(context);
                var person1 = new Person
                {
                    Name = "Beth",
                    Gender = Gender.Female,
                    IsRoyal = true
                };
                var person2 = new Person
                {
                    Name = "Jerry",
                    Gender = Gender.Male
                };

                // act
                await repository.SaveAsync(person1);
                await repository.SaveAsync(person2);
                person1.Spouse = person2;
                await repository.SaveAsync(person1);

                var spouse = await repository.GetSpouse(person2);

                // assert                
                Assert.NotNull(spouse);
                Assert.Equal(person1.Id, spouse.Id);
            }
        }
    }
}
