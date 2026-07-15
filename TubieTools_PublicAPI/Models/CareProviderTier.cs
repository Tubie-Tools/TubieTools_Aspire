namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Care provider tier classification for volumetric expectations and pricing
    /// </summary>
    public enum CareProviderTier
    {
        /// <summary>
        /// Small day care facilities - 50+ orders per year
        /// </summary>
        DayCare = 1,

        /// <summary>
        /// Medium elderly home facilities - 100+ orders per year
        /// </summary>
        ElderlyHome = 2,

        /// <summary>
        /// Large healthcare provider/hospital - 500+ orders per year
        /// </summary>
        HealthcareProvider = 3
    }
}
