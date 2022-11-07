﻿namespace Altinn.AccessManagement.Integration.Configuration
{
    /// <summary>
    /// General configuration settings
    /// </summary>
    public class PlatformSettings
    {
        /// <summary>
        /// Open Id Connect Well known endpoint
        /// </summary>
        public string? OpenIdWellKnownEndpoint { get; set; }

        /// <summary>
        /// Name of the cookie for where JWT is stored
        /// </summary>
        public string? JwtCookieName { get; set; }
    }
}
