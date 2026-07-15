namespace TubieTools_PublicAPI.Data.Entities
{
    /// <summary>
    /// Entity Framework model for Contact information
    /// </summary>
    public class ContactEntity
    {
        /// <summary>
        /// Primary key - Database ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Job title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Department
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Date when contact was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        public ContactEntity()
        {
            CreatedDate = DateTime.UtcNow;
        }
    }
}
