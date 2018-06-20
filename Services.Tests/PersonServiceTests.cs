using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoyalFamily.Common.Data.Entities;
using RoyalFamily.Common.Data.Models;
using RoyalFamily.Common.Data.Repositories;
using Moq;
using Xunit;

namespace RoyalFamily.Services.Tests
{
    public class PersonServiceTests
    {        
        private static Person _parentA1;
        private static Person _parentA2;        
        private static Person _childA1;
        private static Person _childA2;

        private static Person _parentB1;
        private static Person _parentB2;
        private static Person _childB1;
        private static Person _childB2;

        private static Person _grandParent1;
        private static Person _grandParent2;
        private Mock<IPersonRepository> GetMockedPersonRepository() => new Mock<IPersonRepository>();

        static PersonServiceTests()
        {
            var id = 1;
            _grandParent1 = new Person(id++, "Rick Sanchez", Gender.Male);
            _grandParent2 = new Person(id++, "Mrs Sanchez", Gender.Female, false);
            _parentA1 = new Person(id++, "Beth Smith", Gender.Female);
            _parentA2 = new Person(id++, "Jerry Smith", Gender.Male, false);
            _childA1 = new Person(id++, "Morty Smith", Gender.Male);
            _childA2 = new Person(id++, "Summer Smith", Gender.Female);
                    
            _parentA1.Spouse = _parentA2;
            _parentA1.Parents = CreateParentalRelationships(_parentA1, _grandParent1, _grandParent2);
            _parentA1.Children = CreateChildRelationships(_parentA1, _childA1, _childA1);
            _parentA2.Children = CreateChildRelationships(_parentA2, _childA1, _childA1);

            _childA1.Parents = CreateParentalRelationships(_childA1, _parentA1, _parentA2);
            _childA2.Parents = CreateParentalRelationships(_childA2, _parentA1, _parentA2);            

            _parentB1 = new Person(id++, "parentB1", Gender.Male, false);
            _parentB2 = new Person(id++, "parentB2", Gender.Female);
            _childB1 = new Person(id++, "childB1", Gender.Male);
            _childB2 = new Person(id++, "childB2", Gender.Female);

            _parentB1.Children = CreateChildRelationships(_parentB1, _childB1, _childB1);
            _parentB2.Children = CreateChildRelationships(_parentB2, _childB1, _childB1);
            _childB1.Parents = CreateParentalRelationships(_childB1, _parentB1, _parentB2);
            _childB2.Parents = CreateParentalRelationships(_childB2, _parentB1, _parentB2);

            _grandParent1.Children = CreateChildRelationships(_grandParent1, _parentA1, _parentB1);
            _grandParent2.Children = CreateChildRelationships(_grandParent2, _parentA1, _parentB1);
        }

        private static ICollection<FamilialRelationship> CreateParentalRelationships(Person child, params Person[] parents)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (parents == null) throw new ArgumentNullException(nameof(parents));

            return parents.Select(parent => new FamilialRelationship(parent, child)).ToList();            
        }

        private static ICollection<FamilialRelationship> CreateChildRelationships(Person parent, params Person[] children)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (children == null) throw new ArgumentNullException(nameof(children));

            return children.Select(child => new FamilialRelationship(parent, child)).ToList();
        }

        [Fact]
        public async Task GetRoyalAncestry_With3LevelHierarchy_ReturnsCorrectHierarchy()
        {
            //arrange
            var input = _childA1;
            var expectedResult = new List<Person>()
            {
                _childA1,
                _parentA1, 
                _grandParent1
            };
            var personRepository = GetMockedPersonRepository();
            personRepository.Setup(r => r.GetByIdAsync(_parentA1.Id)).Returns(Task.FromResult(_parentA1));
            personRepository.Setup(r => r.GetByIdAsync(_grandParent1.Id)).Returns(Task.FromResult(_grandParent1));
            var target = new PersonService(personRepository.Object);
            
            //act
            var result = await target.GetRoyalAncestry(input);

            //assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.Count, result.Count);
            for (int i = 0; i < result.Count; i++)
                Assert.Equal(expectedResult[i], result[i]);                
        }

        // input: Person fromPerson, Person toPerson, List<Person> fromAncestry, List<Person> toAncestry,
        // output: Relationship
        public static IEnumerable<object[]> GetRelationshipData()
        {
            // Summer -> Morty (Brother)
            yield return new object[]
            {
                _childA2,
                _childA1,
                new List<Person> {_childA2, _parentA1},
                new List<Person> {_childA1, _parentA1},
                new Relationship { RelationshipType = RelationshipType.Sibling, Gender = Gender.Male }
            };
            // Morty -> Summer (Sister)
            yield return new object[]
            {
                _childA1,
                _childA2,
                new List<Person> {_childA1, _parentA1},
                new List<Person> {_childA2, _parentA1},
                new Relationship { RelationshipType = RelationshipType.Sibling, Gender = Gender.Female }
            };
            // Beth -> Jerry (Husband)
            yield return new object[]
            {
                _parentA1,
                _parentA2,
                new List<Person> { _parentA1 },
                new List<Person> { _parentA2 },
                new Relationship { RelationshipType = RelationshipType.Spouse, Gender = Gender.Male }
            };
            // Jerry -> Beth (Wife)
            yield return new object[]
            {
                _parentA2,
                _parentA1,
                new List<Person> { _parentA2 },
                new List<Person> { _parentA1 },
                new Relationship { RelationshipType = RelationshipType.Spouse, Gender = Gender.Female }
            };
            // Jerry -> Jerry (Self)
            yield return new object[]
            {
                _parentA2,
                _parentA2,
                new List<Person> { _parentA2 },
                new List<Person> { _parentA2 },
                new Relationship { RelationshipType = RelationshipType.Self }
            };
            // Morty -> Rick (Grandfather)
            yield return new object[]
            {
                _childA1,
                _grandParent1,
                new List<Person> { _childA1, _parentA1, _grandParent1 },
                new List<Person> { _grandParent1 },
                new Relationship
                {
                    RelationshipType = RelationshipType.Ancestor, Gender = Gender.Male, DistanceRemoved = 2,
                    ParentalRelationType = ParentalRelationType.Maternal
                }
            };
            // Cousin
            yield return new object[]
            {
                _childA1,
                _childB1,
                new List<Person> { _childA1, _parentA1, _grandParent1 },
                new List<Person> { _childB1, _parentB1, _grandParent1 },                
                new Relationship { RelationshipType = RelationshipType.Cousin, DistanceRemoved = 0 }
            };
            // Nephew 
            yield return new object[]
            {
                _parentA1,
                _childB1,
                new List<Person> { _parentA1, _grandParent1 },
                new List<Person> { _childB1, _parentB1, _grandParent1 },
                new Relationship { RelationshipType = RelationshipType.SiblingsDescendant, Gender = Gender.Male, DistanceRemoved = 1 }
            };
            // Uncle
            yield return new object[]
            {
                _childA1,
                _parentB1,
                new List<Person> { _childA1, _parentA1, _grandParent1 },
                new List<Person> { _parentB1, _grandParent1 },
                new Relationship
                {
                    RelationshipType = RelationshipType.AncestorsSibling, Gender = Gender.Male, DistanceRemoved = 1, InLaw = true,
                    ParentalRelationType = ParentalRelationType.Maternal
                }
            };
        }
 
        [Theory]
        [MemberData(nameof(GetRelationshipData))]
        public void GetRelationShip_FunctionTest_ShouldReturnExpectedRelationshipType(Person fromPerson, Person toPerson, 
            IList<Person> fromAncestry, IList<Person> toAncestry, Relationship expectedOutput)
        {
            //arrange            
            var personRepository = GetMockedPersonRepository();
            var target = new PersonService(personRepository.Object);

            //act
            var result = target.GetRelationship(fromPerson, toPerson, fromAncestry, toAncestry);

            //assert
            Assert.NotNull(result);
            Assert.Equal(expectedOutput.RelationshipType, result.RelationshipType);
            Assert.Equal(expectedOutput.DistanceRemoved, result.DistanceRemoved);
            Assert.Equal(expectedOutput.Gender, result.Gender);
            Assert.Equal(expectedOutput.InLaw, result.InLaw);
            Assert.Equal(expectedOutput.ParentalRelationType, result.ParentalRelationType);
        }

        // input: Relationship
        // output: string
        public static IEnumerable<object[]> GetRelationshipNameData()
        {
            // Summer -> Morty (Brother)
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.Sibling, Gender = Gender.Male },
                "Brother"
            };
            // Morty -> Summer (Sister)
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.Sibling, Gender = Gender.Female },
                "Sister"
            };
            // Beth -> Jerry (Husband)
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.Spouse, Gender = Gender.Male },
                "Husband"
            };
            // Jerry -> Beth (Wife)
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.Spouse, Gender = Gender.Female },
                "Wife"
            };
            // Jerry -> Jerry (Self)
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.Self },
                "Self"
            };
            // Morty -> Rick (Grandfather)
            yield return new object[]
            {                
                new Relationship
                {
                    RelationshipType = RelationshipType.Ancestor, Gender = Gender.Male, DistanceRemoved = 2,
                    ParentalRelationType = ParentalRelationType.Maternal
                },
                "Maternal grandfather"
            };
            // Great grandfather
            yield return new object[]
            {
                new Relationship
                {
                    RelationshipType = RelationshipType.Ancestor, Gender = Gender.Male, DistanceRemoved = 3,
                    ParentalRelationType = ParentalRelationType.Maternal
                },
                "Maternal great-grandfather"
            };
            // Great Great grandfather
            yield return new object[]
            {
                new Relationship
                {
                    RelationshipType = RelationshipType.Ancestor, Gender = Gender.Male, DistanceRemoved = 4,
                    ParentalRelationType = ParentalRelationType.Maternal
                },
                "Maternal great-great-grandfather"
            };
            // Cousin
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.Cousin, DistanceRemoved = 0 },
                "Cousin"
            };
            // Nephew 
            yield return new object[]
            {                
                new Relationship { RelationshipType = RelationshipType.SiblingsDescendant, Gender = Gender.Male, DistanceRemoved = 1 },
                "Nephew"
            };
            // Grandnephew 
            yield return new object[]
            {
                new Relationship { RelationshipType = RelationshipType.SiblingsDescendant, Gender = Gender.Male, DistanceRemoved = 2 },
                "Grandnephew"
            };
            // Uncle
            yield return new object[]
            {                
                new Relationship
                {
                    RelationshipType = RelationshipType.AncestorsSibling, Gender = Gender.Male, DistanceRemoved = 1, InLaw = true,
                    ParentalRelationType = ParentalRelationType.Maternal
                },
                "Maternal uncle-in-law"
            };
            // Great Uncle
            yield return new object[]
            {
                new Relationship
                {
                    RelationshipType = RelationshipType.AncestorsSibling, Gender = Gender.Male, DistanceRemoved = 2, InLaw = true,
                    ParentalRelationType = ParentalRelationType.Maternal
                },
                "Maternal granduncle-in-law"
            };
        }

        [Theory]
        [MemberData(nameof(GetRelationshipNameData))]
        public void GetRelationshipName_FunctionTest_ShouldReturnExpectedRelationName(Relationship relationship, string expectedOutput)
        {
            //arrange            
            var personRepository = GetMockedPersonRepository();
            var target = new PersonService(personRepository.Object);

            //act
            var result = target.GetRelationshipName(relationship);

            //assert
            Assert.NotNull(result);
            Assert.Equal(expectedOutput, result);
        }
    }
}
