using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Bio { get; set; }

    }
}