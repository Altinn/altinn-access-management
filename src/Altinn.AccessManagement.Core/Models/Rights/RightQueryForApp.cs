namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Queries for rights delegable by the resource itself those must be defined in the policy with subject set to the resource.
/// </summary>
public class RightQueryForApp
{
    /// <summary>
    /// The resource to check for delegable rights
    /// </summary>
    public IEnumerable<AttributeMatch> OwnerApp { get; set; }

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for identifying the resource the rights 
    /// </summary>
    public List<AttributeMatch> Resource { get; set; }
}