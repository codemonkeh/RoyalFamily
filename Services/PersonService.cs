using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoyalFamily.Common;
using RoyalFamily.Common.Data.Entities;
using RoyalFamily.Common.Data.Models;
using RoyalFamily.Common.Data.Repositories;
using RoyalFamily.Common.Services;

namespace RoyalFamily.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _personRepository;

        public PersonService(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        /// <summary>
        /// Calculates the relationship between two people within the royal family tree
        /// </summary>
        /// <param name="fromName">The name of the person being compared</param>
        /// <param name="toName">The name of the person being compared to</param>
        /// <returns>The relationship e.g. Grandfather</returns>
        public async Task<string> GetRelationship(string fromName, string toName)
        {
            if (fromName == null) throw new ArgumentNullException(nameof(fromName));
            if (toName == null) throw new ArgumentNullException(nameof(toName));

            var fromPerson = await _personRepository.GetByNameAsync(fromName) ?? throw new ValidationException($"Unknown person \"{fromName}\"");
            var toPerson = await _personRepository.GetByNameAsync(toName) ?? throw new ValidationException($"Unknown person \"{toName}\"");

            // If either person is not royal, a.k.a a spouse, identify the royal partner
            var person1 = fromPerson.IsRoyal ? fromPerson : await _personRepository.GetSpouse(fromPerson);
            var person2 = toPerson.IsRoyal ? toPerson : await _personRepository.GetSpouse(toPerson);

            // Map the royal ancestry from leaf to root
            var person1Ancestry = await GetRoyalAncestry(person1);
            var person2Ancestry = await GetRoyalAncestry(person2);

            // Find the common ancestor and use this as the new root 
            RemoveCommonAncestry(ref person1Ancestry, ref person2Ancestry);

            // Determine the relationship and calculate the name
            var relationship = GetRelationship(fromPerson, toPerson, person1Ancestry, person2Ancestry);
            return GetRelationshipName(relationship);
        }        

        /// <summary>
        /// Determines the relationship between two people given their royal ancestry
        /// </summary>
        /// <param name="person1">The person being compared</param>
        /// <param name="person2">The person being compared to</param>
        /// <param name="ancestry1">The royal ancestry of <c>person1</c></param>
        /// <param name="ancestry2">The royal ancestry of <c>person1</c></param>
        /// <returns>An object containing relationship particulars</returns>
        /// <remarks>The ancestry objects are a sequential list of royal ancestry starting from the person themself (or their spouse if they are not royal)</remarks>
        public Relationship GetRelationship(Person person1, Person person2, IList<Person> ancestry1, IList<Person> ancestry2)
        {
            // Calculate the ancestor in levels of removal to yourself, and the other person.
            // E.g. if the ancestor is you it will be 0, 1 for your parents, etc.
            var person1Level = ancestry1.Count - 1; // discounting the first element which is yourself
            var person2Level = ancestry2.Count - 1;

            // if either of the persons are not royal then this is a relationship through marriage
            bool inLaw = !(person1.IsRoyal && person2.IsRoyal);

            // 0:0 - no separation, you are either the same person or husband and wife
            if (person1Level == 0 && person2Level == 0)
            {
                if (person1.Id == person2.Id)
                    return new Relationship { RelationshipType = RelationshipType.Self };
                
                // Otherwise they are partners
                return new Relationship()
                {
                    RelationshipType = RelationshipType.Spouse,
                    Gender = person2.Gender
                };
            }
            // if one of the results are 0 and the other different you are in the same nuclear family tree and they are either your descendant or ancestor
            else if (person1Level == 0)
            {
                return new Relationship()
                {
                    RelationshipType = RelationshipType.Descendant,
                    Gender = person2.Gender,
                    DistanceRemoved = person2Level,
                    InLaw = inLaw
                };
            }
            else if (person2Level == 0)
            {
                return new Relationship()
                {
                    RelationshipType = RelationshipType.Ancestor,
                    Gender = person2.Gender,
                    DistanceRemoved = person1Level,
                    InLaw = inLaw,
                    ParentalRelationType = GetParentalRelationType(ancestry1)
                };
            }
            else if (person1Level == person2Level)
            {
                // 1:1 - siblings
                if (person1Level == 1 && person2Level == 1)
                {
                    // if they share the same parents they are brother or sister (possibly in-law)
                    return new Relationship()
                    {
                        RelationshipType = RelationshipType.Sibling,
                        Gender = person2.Gender,
                        InLaw = inLaw
                    };
                }
                else // 2:2 - cousins, 3:3 - cousins once removed, 4:4 - second cousins, etc..
                {                    
                    return new Relationship()
                    {
                        RelationshipType = RelationshipType.Cousin,
                        DistanceRemoved = person1Level - 2, // adjust so cousins removed distance is accurate
                        InLaw = inLaw
                    };
                }                
            }
            else // if the result is otherwise uneven, you are a combination of aunt/uncle and nephew/niece
            {
                // niece/nephew and etc.
                if (person1Level < person2Level)
                {
                    return new Relationship()
                    {
                        RelationshipType = RelationshipType.SiblingsDescendant,
                        DistanceRemoved = Math.Abs(person1Level - person2Level),
                        InLaw = inLaw,
                        Gender = person2.Gender
                    };
                }
                else // aunt/uncle and etc.
                {
                    return new Relationship()
                    {
                        RelationshipType = RelationshipType.AncestorsSibling,
                        DistanceRemoved = Math.Abs(person1Level - person2Level),
                        InLaw = inLaw,
                        Gender = person2.Gender,
                        ParentalRelationType = GetParentalRelationType(ancestry1)
                    };
                }
            }
        }

        /// <summary>
        /// Determines if the person is related through their mother or father
        /// </summary>
        /// <param name="ancestry">A list of ancestors from the current person</param>
        /// <returns>A type exposing if related through their mother or father</returns>
        public ParentalRelationType GetParentalRelationType(IList<Person> ancestry)
        {
            if (ancestry == null) throw new ArgumentNullException(nameof(ancestry));

            // No parents, no relation
            if (ancestry.Count <= 1) return ParentalRelationType.None;

            return ancestry[1].Gender == Gender.Female ? ParentalRelationType.Maternal : ParentalRelationType.Paternal;
        }

        /// <summary>
        /// Calculates the exact nature of the relationship from its specifics
        /// </summary>
        /// <param name="relationship">The relationship described in parts</param>
        /// <returns>The name of the relationship</returns>
        public string GetRelationshipName(Relationship relationship)
        {
            var sb = new StringBuilder();
            // Maternal or Paternal prefix
            if (relationship.ParentalRelationType != ParentalRelationType.None)
                sb.Append((relationship.ParentalRelationType == ParentalRelationType.Paternal ? "paternal" : "maternal") + " ");

            switch (relationship.RelationshipType)
            {
                // nonspecific
                case RelationshipType.Self:
                    sb.Append("self");
                    break;
                case RelationshipType.Cousin:
                    sb.Append("cousin");
                    break;

                // gender specific
                case RelationshipType.Spouse:
                    sb.Append(relationship.Gender == Gender.Male ? "husband" : "wife");
                    break;
                case RelationshipType.Sibling:
                    sb.Append(relationship.Gender == Gender.Male ? "brother" : "sister");
                    break;

                case RelationshipType.Ancestor:
                    if (relationship.DistanceRemoved > 1)
                        sb.Append(GetRelatedPrefix(relationship.RelationshipType, relationship.DistanceRemoved));
                    sb.Append(relationship.Gender == Gender.Male ? "father" : "mother");
                    break;
                case RelationshipType.Descendant:
                    if (relationship.DistanceRemoved > 1)
                        sb.Append(GetRelatedPrefix(relationship.RelationshipType, relationship.DistanceRemoved));
                    sb.Append(relationship.Gender == Gender.Male ? "son" : "daughter");
                    break;
                case RelationshipType.SiblingsDescendant:
                    if (relationship.DistanceRemoved > 1)
                        sb.Append(GetRelatedPrefix(relationship.RelationshipType, relationship.DistanceRemoved));
                    sb.Append(relationship.Gender == Gender.Male ? "nephew" : "niece");
                    break;
                case RelationshipType.AncestorsSibling:
                    if (relationship.DistanceRemoved > 1)
                        sb.Append(GetRelatedPrefix(relationship.RelationshipType, relationship.DistanceRemoved));
                    sb.Append(relationship.Gender == Gender.Male ? "uncle" : "aunt");
                    break;
                default:
                    throw new Exception($"Relationship type {relationship.RelationshipType} is not supported");
            }

            if (relationship.InLaw)
                sb.Append("-in-law");

            //Capitalise the first initial
            if (sb.Length > 0) sb[0] = sb[0] .ToString().ToUpper()[0];

            return sb.ToString();
        }

        /// <summary>
        /// Generates the prefix applied to relations more than 2 levels distant.
        /// E.g. grandfather, great-grandfather
        /// </summary>
        /// <param name="type">The type of relation</param>
        /// <param name="distanceRemoved">Number of ancestors/descendants removed in relation</param>
        /// <returns>The prefix of the relationship  e.g. "grand", "great-grand"</returns>
        public string GetRelatedPrefix(RelationshipType type, int distanceRemoved)
        {
            if (distanceRemoved <= 1) return string.Empty;

            var sb = new StringBuilder("grand");
            if (distanceRemoved > 2)
            {
                // Keep adding "Great" prefixes as required
                // e.g. great-great-grandfather
                for (int i = 2; i < distanceRemoved; i++)
                    sb.Insert(0, "great-");
            }

            return sb.ToString();            
        }

        /// <summary>
        /// Removes ancestry beyond the common ancestor
        /// </summary>
        private void RemoveCommonAncestry(ref IList<Person> person1Ancestry, ref IList<Person> person2Ancestry)
        {
            int count = 0;

            // search from root to leaf as all ancestry should have the same root
            var ancestry1 = person1Ancestry.Reverse().ToArray();
            var ancestry2 = person2Ancestry.Reverse().ToArray();

            for (int i = 0; i < Math.Min(ancestry1.Length, ancestry2.Length); i++)
            {
                if (ancestry1[i].Id != ancestry2[i].Id) break;
                count = i;
            }

            // ensure the ancestry remains sorted leaf to root for further calculation
            person1Ancestry = ancestry1.Skip(count).Reverse().ToArray();
            person2Ancestry = ancestry2.Skip(count).Reverse().ToArray();
        }

        /// <summary>
        /// Calculates a list of royal ancestors for a given person
        /// </summary>
        /// <param name="person">A given person</param>
        /// <returns>A list of royal ancestors</returns>
        public async Task<IList<Person>> GetRoyalAncestry(Person person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));

            var ancestry = new List<Person>();
            var ancestor = person;
            do
            {
                ancestry.Add(ancestor);
                Person nextAncestor = null;
                if (ancestor.Parents?.Any() == true)
                {
                    foreach (var p in ancestor.Parents)
                    {
                        // Source the parent again to prevent a circular dependency
                        var parent = await _personRepository.GetByIdAsync(p.ParentId);
                        if (parent.IsRoyal)
                        {
                            nextAncestor = parent;
                            break;
                        }
                    }
                }

                ancestor = nextAncestor;
            }
            while (ancestor != null);

            return ancestry;
        }
    }
}