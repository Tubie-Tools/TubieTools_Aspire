using System.ComponentModel.DataAnnotations;

namespace ModelLayer
{
    public class Gender
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
