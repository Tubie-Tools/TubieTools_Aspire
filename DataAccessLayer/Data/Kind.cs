using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer
{
    public class Kind
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
