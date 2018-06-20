namespace RoyalFamily.Common.Data.Models
{
    public enum Gender : short
    {
        Male = 1,
        Female = 2
    }

    // Prefixes and suffixes such as pa/maternal and in-law with be programmatically determined
    public enum RelationshipType : short
    {
        None = 0,
        Self,
        Spouse,
        Ancestor,
        Descendant,
        Sibling,
        SiblingsDescendant,
        AncestorsSibling,
        Cousin
    }

    public enum ParentalRelationType : short
    {
        None = 0,
        Paternal,
        Maternal
    }
}
