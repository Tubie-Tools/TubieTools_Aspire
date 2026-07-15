namespace TubieTools_PublicAPI.Data.Entities
{
    /// <summary>
    /// Entity Framework model for Address information
    /// </summary>
    public class AddressEntity
    {
        /// <summary>
        /// Primary key - Database ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Street address
        /// </summary>
        public string StreetAddress { get; set; }

        /// <summary>
        /// City
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// State or province
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Postal or zip code
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Indicates if this is the primary address
        /// </summary>
        public bool IsPrimaryAddress { get; set; }

        /// <summary>
        /// Type of address (Billing, Shipping, Primary)
        /// </summary>
        public string AddressType { get; set; }

        /// <summary>
        /// Date when address was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        public AddressEntity()
        {
            CreatedDate = DateTime.UtcNow;
        }
    }
}
