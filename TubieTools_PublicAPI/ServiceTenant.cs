namespace TubieTools_PublicAPI
{
    internal class ServiceTenant : IServiceTenant
    {
        public ServiceTenant(string tenantId, string serviceName)
        {
            TenantId = tenantId;
            ServiceName = serviceName;
        }

        public string TenantId { get; }
        public string ServiceName { get; }
        public Dictionary<string, IEnumerable<NestedArray>> Complicated { get; set; }
    }

    public class NestedArray
    {
        public string Name { get; set; }
        public int[] Values { get; set; } = new int[0];
    }


}