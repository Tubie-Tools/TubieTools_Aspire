using System;

namespace ModelLayer
{
    public interface IProfile
    {
        public int Id { get; set; }
        public string Bio { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; } 
        public int GenderId { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}