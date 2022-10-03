﻿namespace Altinn.AuthorizationAdmin.Core.Models.ResourceRegistry
{
    /// <summary>
    /// Enum representation of the different types of resources supported by the resource registry
    /// </summary>
    public enum ResourceType
    {
        Default = 0,
        
        Systemresource = 1, 
        
        Altinn2 = 2,
        
        Altinn3 = 3,
        
        Apischema = 4,

        Api = 5
    }
}