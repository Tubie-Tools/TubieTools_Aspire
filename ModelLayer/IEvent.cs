using System;

namespace ModelLayer
{
    public interface IEvent
    {
        public int Id { get; set; } 
        public string Notes { get; set; } 
        public string EventName { get; set; }         
        public int EventTypeId { get; set; }         
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }         
        public int ProfileId { get; set; }
        public int? WeightPounds { get; set; }
        public int? WeightOunces { get; set; }
    }
}