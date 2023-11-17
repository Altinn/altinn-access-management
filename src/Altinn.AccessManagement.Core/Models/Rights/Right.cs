namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// This model describes a single right
/// </summary>
public class Right
{
    /// <summary>
    /// Gets or sets the right key
    /// </summary>
    public string RightKey
    {
        get
        {
            return string.Join(",", Resource.OrderBy(m => m.Id).Select(m => m.Value)) + ":" + Action.Value;
        }
    }

    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    public List<AttributeMatch> Resource { get; set; }

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
    /// </summary>
    public AttributeMatch Action { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user or party has the right
    /// </summary>
    public bool? HasPermit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user or party is permitted to delegate the right to others
    /// </summary>
    public bool? CanDelegate { get; set; }

    /// <summary>
    /// Gets or sets the set of identified sources providing the right
    /// </summary>
    public List<RightSource> RightSources { get; set; }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is Right right && string.Equals(RightKey, right.RightKey, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return RightKey.GetHashCode();
    }
}
