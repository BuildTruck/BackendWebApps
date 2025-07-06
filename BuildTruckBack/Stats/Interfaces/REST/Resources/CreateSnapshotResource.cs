namespace BuildTruckBack.Stats.Interfaces.REST.Resources;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Resource for creating manual snapshots
/// </summary>
public record CreateSnapshotResource(
    [Required]
    [StringLength(1000, MinimumLength = 1, ErrorMessage = "Notes must be between 1 and 1000 characters")]
    string Notes
)
{
    /// <summary>
    /// Validate the request
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Notes) && Notes.Length <= 1000;
    }
};