using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer
{
    public class EventType
    {
        public string Name { get;   set; }
        public DateTime DateCreated { get;   set; }
        public string CreatedBy { get;   set; }
        [Key]
        public int Id { get;   set; }
    }
}