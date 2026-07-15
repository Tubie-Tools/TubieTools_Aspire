namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Address information for care providers
    /// </summary>
    public class Address
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsPrimaryAddress { get; set; }
        public string AddressType { get; set; } // "Billing", "Shipping", "Primary"
    }
}
