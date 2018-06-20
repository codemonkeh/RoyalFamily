namespace RoyalFamily.Common.Data.Models
{
    public class Relationship
    {
        public RelationshipType RelationshipType { get; set; }
        public ParentalRelationType ParentalRelationType { get; set; }
        public int DistanceRemoved { get; set; }
        public bool InLaw { get; set; }
        public Gender Gender { get; set; }
    }
}
