namespace BasicWebNovelAPI.Enum
{
    /// <summary>
    /// Represents the issuer or provider of authentication for a user.
    /// This enum is used to track how a user was authenticated in the system.
    /// </summary>
    public enum AuthIssuer
    {
        /// <summary>
        /// Traditional JWT authentication using email/password
        /// </summary>
        JWT,
        
        /// <summary>
        /// Authentication via Facebook OAuth
        /// </summary>
        FACEBOOK
    }
}
