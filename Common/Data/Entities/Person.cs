using System.Collections.Generic;
using RoyalFamily.Common.Data.Models;

namespace RoyalFamily.Common.Data.Entities
{
    public class Person
    {
        public Person()
        {
            Parents = new List<FamilialRelationship>();
            Children = new List<FamilialRelationship>();
        }

        public Person(int id, string name, Gender gender, bool isRoyal = true) : this()
        {
            Id = id;
            Name = name;
            Gender = gender;
            IsRoyal = isRoyal;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRoyal { get; set; }
        public Gender Gender { get; set; }

        // The spouse is defined for the direct descendant of the King, as this is strictly relevant, and to prevent a circular dependency
        public int SpouseId { get; set; }
        public Person Spouse { get; set; }        
                
        public ICollection<FamilialRelationship> Parents { get; set; }
        public ICollection<FamilialRelationship> Children { get; set; }
    }
}
