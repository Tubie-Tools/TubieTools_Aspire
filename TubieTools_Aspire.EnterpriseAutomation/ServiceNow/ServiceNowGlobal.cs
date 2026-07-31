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

    // ServiceNow Tools Interfaces and Result Classes
    public interface IServiceNowTool
    {
        string Name { get; }
        string Description { get; }
        Task<object> ExecuteAsync(Dictionary<string, object> parameters);
    }

    public interface ICreateIncidentTool : IServiceNowTool
    {
    }

    public interface ISearchIncidentTool : IServiceNowTool
    {
    }

    public interface ICloseIncidentTool : IServiceNowTool
    {
    }

    public class CreateIncidentResult
    {
        public bool Success { get; set; }
        public string IncidentNumber { get; set; }
        public string Message { get; set; }
        public Incident CreatedIncident { get; set; }
    }

    public class SearchIncidentResult
    {
        public bool Success { get; set; }
        public int TotalCount { get; set; }
        public List<Incident> Incidents { get; set; } = new();
        public string Message { get; set; }
    }

    public class CloseIncidentResult
    {
        public bool Success { get; set; }
        public string IncidentNumber { get; set; }
        public string ClosureNotes { get; set; }
        public string Message { get; set; }
    }
}
