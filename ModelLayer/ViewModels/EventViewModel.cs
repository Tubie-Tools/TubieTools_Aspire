using System.ComponentModel.DataAnnotations;

namespace ModelLayer.ViewModels;

public class EventViewModel : IEvent
{
    public int Id { get; set; }
    [Display(Name = "Notes")]
    public string Notes { get; set; }
    [Display(Name = "What occured (Vomiting, Diarrhea, Nausea)")]
    public string EventName { get; set; }
    public int EventTypeId { get; set; }

    [Display(Name = "Time of Occurance")]
    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }

    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }

    public int ProfileId { get; set; }
    public int? WeightPounds { get; set; }
    public int? WeightOunces { get; set; }
}
