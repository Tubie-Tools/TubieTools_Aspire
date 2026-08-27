namespace TubieTools_Aspire.EnterpriseAutomation.AzureDevOps
{
    public class PipelineRunStatus
    {
        public string RunId { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
        public List<string> Logs { get; set; }
    }


}
