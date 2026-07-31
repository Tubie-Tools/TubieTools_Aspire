using Microsoft.Extensions.Logging;
using TubieTools_Aspire.EnterpriseAutomation.ServiceNow;

namespace TubieTools_Aspire.EnterpriseAutomation.ServiceNow.Tools
{
    /// <summary>
    /// Factory for creating and managing ServiceNow tools
    /// </summary>
    public interface IServiceNowToolsFactory
    {
        ICreateIncidentTool GetCreateIncidentTool();
        ISearchIncidentTool GetSearchIncidentTool();
        ICloseIncidentTool GetCloseIncidentTool();
        List<IServiceNowTool> GetAllTools();
    }

    /// <summary>
    /// Implementation of ServiceNowToolsFactory
    /// </summary>
    public class ServiceNowToolsFactory : IServiceNowToolsFactory
    {
        private readonly ICreateIncidentTool _createIncidentTool;
        private readonly ISearchIncidentTool _searchIncidentTool;
        private readonly ICloseIncidentTool _closeIncidentTool;
        private readonly ILogger<ServiceNowToolsFactory> _logger;

        public ServiceNowToolsFactory(
            ICreateIncidentTool createIncidentTool,
            ISearchIncidentTool searchIncidentTool,
            ICloseIncidentTool closeIncidentTool,
            ILogger<ServiceNowToolsFactory> logger)
        {
            _createIncidentTool = createIncidentTool;
            _searchIncidentTool = searchIncidentTool;
            _closeIncidentTool = closeIncidentTool;
            _logger = logger;

            _logger.LogInformation("ServiceNowToolsFactory initialized with 3 tools available");
        }

        public ICreateIncidentTool GetCreateIncidentTool() => _createIncidentTool;

        public ISearchIncidentTool GetSearchIncidentTool() => _searchIncidentTool;

        public ICloseIncidentTool GetCloseIncidentTool() => _closeIncidentTool;

        public List<IServiceNowTool> GetAllTools()
        {
            return new List<IServiceNowTool>
            {
                _createIncidentTool,
                _searchIncidentTool,
                _closeIncidentTool
            };
        }
    }
}
