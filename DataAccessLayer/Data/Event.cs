using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        [Display(Name ="What occured (Vomiting, Diarrhea, Nausea)")]
        public string EventName { get; set; }

        [ForeignKey("Event Type")]
        public int EventTypeId { get; set; }

        [Display(Name = "Time of Occurance")]
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        [ForeignKey("Profile")]
        public int ProfileId { get; set; }
        //public ICollection<ProfileEvents> ProfileEvents { get; set; }

        public int? WeightPounds { get; set; }
        public int? WeightOunces { get; set; }
        public ICollection<EventThings> EventThings { get; set; }
    }
}
