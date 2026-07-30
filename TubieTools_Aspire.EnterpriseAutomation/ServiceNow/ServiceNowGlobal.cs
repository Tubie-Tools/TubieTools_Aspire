namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow
{
    public interface IServiceNowService
    {
        Task<List<Incident>> GetIncidentsAsync(string query = "");
        Task<Incident> GetIncidentAsync(string incidentNumber);
        Task<Incident> CreateIncidentAsync(CreateIncidentRequest request);
        Task<bool> UpdateIncidentAsync(string incidentNumber, UpdateIncidentRequest request);
        Task<List<ChangeRequest>> GetChangeRequestsAsync();
        Task<ChangeRequest> CreateChangeRequestAsync(CreateChangeRequest request);
        Task<bool> ApproveChangeAsync(string changeId);
    }

    public class Incident
    {
        public string Number { get; set; }
        public string Title { get; set; }
        public string State { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }

    public class CreateIncidentRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Category { get; set; }
    }

    public class UpdateIncidentRequest
    {
        public string State { get; set; }
        public string WorkNotes { get; set; }
        public string AssignedTo { get; set; }
    }

    public class ChangeRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
    }

    public class CreateChangeRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ChangeType { get; set; }
        public string Priority { get; set; }
    }
}
