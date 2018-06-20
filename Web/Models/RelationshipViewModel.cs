using System.ComponentModel.DataAnnotations;

namespace RoyalFamily.Web.Models
{
    public class RelationshipViewModel
    {
        [Display(Name = "Person's name")]
        [Required]
        public string Name1 { get; set; }

        [Display(Name = "Other person's name")]
        [Required]
        public string Name2 { get; set; }

        public string Relationship { get; set; }
    }
}
