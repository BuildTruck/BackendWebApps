using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BuildTruckBack.Personnel.Interfaces.REST.Resources;

/// <summary>
/// Resource for updating existing personnel with image upload support
/// </summary>
public class UpdatePersonnelResource
{
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Lastname must be between 2 and 100 characters")]
    public string Lastname { get; init; } = string.Empty;

    [Required]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Position must be between 2 and 150 characters")]
    public string Position { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 100 characters")]
    public string Department { get; init; } = string.Empty;

    [Required]
    public string PersonnelType { get; init; } = string.Empty; // Will be converted to enum

    [Required]
    public string Status { get; init; } = string.Empty; // Will be converted to enum

    [Range(0, double.MaxValue, ErrorMessage = "Monthly amount must be greater than or equal to 0")]
    public decimal MonthlyAmount { get; init; }

    [Range(0, double.MaxValue, ErrorMessage = "Discount must be greater than or equal to 0")]
    public decimal Discount { get; init; }

    [StringLength(50, ErrorMessage = "Bank name cannot exceed 50 characters")]
    public string? Bank { get; init; }

    [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
    public string? AccountNumber { get; init; }

    public DateTime? StartDate { get; init; }

    public DateTime? EndDate { get; init; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; init; }

    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; init; }

    /// <summary>
    /// New avatar image file for personnel (max 5MB, JPG/PNG/WebP)
    /// </summary>
    public IFormFile? ImageFile { get; init; }

    /// <summary>
    /// Flag to remove existing avatar
    /// </summary>
    public bool RemoveImage { get; init; }

    /// <summary>
    /// Validation method for business rules
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Validate personnel type
        var validPersonnelTypes = new[] { "TECHNICAL", "SPECIALIST", "ADMINISTRATIVE", "RENTED_OPERATOR", "LABORER" };
        if (!validPersonnelTypes.Contains(PersonnelType))
        {
            errors.Add($"Invalid personnel type. Valid types: {string.Join(", ", validPersonnelTypes)}");
        }

        // Validate status
        var validStatuses = new[] { "ACTIVE", "INACTIVE", "SUSPENDED", "TERMINATED" };
        if (!validStatuses.Contains(Status))
        {
            errors.Add($"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}");
        }

        // Validate dates
        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
        {
            errors.Add("Start date cannot be later than end date");
        }

        // Validate image file
        if (ImageFile != null)
        {
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (ImageFile.Length > maxFileSize)
                errors.Add("Image file size cannot exceed 5MB");

            var extension = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                errors.Add("Image file must be JPG, PNG, or WebP format");
        }
        

        return errors;
    }

    public bool IsValid() => GetValidationErrors().Count == 0;

    public bool HasChanges()
    {
        return !string.IsNullOrWhiteSpace(Name) ||
               !string.IsNullOrWhiteSpace(Lastname) ||
               !string.IsNullOrWhiteSpace(Position) ||
               !string.IsNullOrWhiteSpace(Department) ||
               !string.IsNullOrWhiteSpace(PersonnelType) ||
               !string.IsNullOrWhiteSpace(Status) ||
               MonthlyAmount > 0 ||
               Discount > 0 ||
               !string.IsNullOrWhiteSpace(Bank) ||
               !string.IsNullOrWhiteSpace(AccountNumber) ||
               StartDate.HasValue ||
               EndDate.HasValue ||
               !string.IsNullOrWhiteSpace(Phone) ||
               !string.IsNullOrWhiteSpace(Email) ||
               ImageFile != null ||
               RemoveImage;
    }
}