using System.Text.Json.Serialization;

namespace TubieTools_PublicAPI.Models
{
    /// <summary>
    /// Represents the response from Okta token introspection endpoint
    /// </summary>
    public class TokenIntrospectionResponse
    {
        /// <summary>
        /// Boolean indicator of whether or not the presented token is currently active
        /// </summary>
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        /// <summary>
        /// Scope associated with the token
        /// </summary>
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        /// <summary>
        /// Integer timestamp, measured in the number of seconds since January 1 1970 UTC
        /// indicating when this token will expire
        /// </summary>
        [JsonPropertyName("exp")]
        public long? ExpirationTime { get; set; }

        /// <summary>
        /// Integer timestamp, measured in the number of seconds since January 1 1970 UTC
        /// indicating when this token was originally issued
        /// </summary>
        [JsonPropertyName("iat")]
        public long? IssuedAt { get; set; }

        /// <summary>
        /// The subject of the token (user ID)
        /// </summary>
        [JsonPropertyName("sub")]
        public string? Subject { get; set; }

        /// <summary>
        /// The intended audience of the token
        /// </summary>
        [JsonPropertyName("aud")]
        public string? Audience { get; set; }

        /// <summary>
        /// The identifier of the entity that issued the token
        /// </summary>
        [JsonPropertyName("iss")]
        public string? Issuer { get; set; }

        /// <summary>
        /// The client ID
        /// </summary>
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        /// <summary>
        /// The client identity id
        /// </summary>
        [JsonPropertyName("cid")]
        public string? Cid { get; set; }

        /// <summary>
        /// The name associated with the token
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The preferred username associated with the token
        /// </summary>
        [JsonPropertyName("preferred_username")]
        public string? PreferredUsername { get; set; }

        /// <summary>
        /// The email associated with the token
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Gets a value indicating whether the token is still valid (active and not expired)
        /// </summary>
        public bool IsValid()
        {
            if (!Active)
                return false;

            if (ExpirationTime.HasValue)
            {
                var expirationDateTime = UnixTimeStampToDateTime(ExpirationTime.Value);
                if (DateTime.UtcNow > expirationDateTime)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the token has the required scopes
        /// </summary>
        public bool HasRequiredScopes(params string[] requiredScopes)
        {
            if (string.IsNullOrEmpty(Scope) || requiredScopes.Length == 0)
                return true;

            var tokenScopes = Scope.Split(' ');
            return requiredScopes.All(required => tokenScopes.Contains(required));
        }

        /// <summary>
        /// Gets the token expiration as a DateTime
        /// </summary>
        public DateTime? GetExpirationDateTime()
        {
            return ExpirationTime.HasValue ? UnixTimeStampToDateTime(ExpirationTime.Value) : null;
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}
