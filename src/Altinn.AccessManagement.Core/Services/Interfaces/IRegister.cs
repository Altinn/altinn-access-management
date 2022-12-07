﻿using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for register
    /// </summary>
    public interface IRegister
    {
        /// <summary>
        /// Gets an organisaiton for an organisation number
        /// </summary>
        /// <param name="organisationNumber">the organisation number</param>
        /// <returns>organisation information</returns>
        public Task<Party> GetOrganisation(string organisationNumber);
    }
}