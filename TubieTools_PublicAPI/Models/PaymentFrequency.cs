namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Payment frequency structure for care providers
    /// </summary>
    public enum PaymentFrequency
    {
        /// <summary>
        /// Single upfront payment
        /// </summary>
        Lump = 1,

        /// <summary>
        /// Monthly recurring billing
        /// </summary>
        Monthly = 2,

        /// <summary>
        /// Quarterly recurring billing
        /// </summary>
        Quarterly = 3
    }
}
