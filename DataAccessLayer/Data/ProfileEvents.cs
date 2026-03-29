using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer
{
    public class ProfileEvents
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Event")]
        public int EventId { get; set; }
        [ForeignKey("Profile")]
        public int ProfileId { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<Profile> Profiles { get; set; }
    }
}
