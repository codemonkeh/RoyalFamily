using System;

namespace RoyalFamily.Common.Data.Entities
{
    public class FamilialRelationship
    {
        public FamilialRelationship()
        {
        }

        public FamilialRelationship(Person parent, Person child)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));            
            Child = child ?? throw new ArgumentNullException(nameof(child));

            ParentId = parent.Id;
            ChildId = child.Id;
        }

        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public Person Parent { get; set; }
        public Person Child { get; set; }
    }
}
