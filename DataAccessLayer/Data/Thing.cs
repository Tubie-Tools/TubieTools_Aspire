using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer
{
    public class Thing
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("Kind")]
        public int KindId { get; set; }
        public ICollection<Kind> Kinds { get; set; }
    }
}
