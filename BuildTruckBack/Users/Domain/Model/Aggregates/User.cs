using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BuildTruckBack.Users.Domain.Model.Commands;
using BuildTruckBack.Users.Domain.Model.ValueObjects;
using EntityFrameworkCore.CreatedUpdatedDate.Contracts;

namespace BuildTruckBack.Users.Domain.Model.Aggregates;

/// <summary>
/// User Aggregate Root
/// </summary>
/// <remarks>
/// Represents a user in the BuildTruck platform with Value Objects
/// </remarks>
public partial class User
{
    public int Id { get; }
    public PersonName Name { get; private set; }
    public EmailAddress CorporateEmail { get; private set; } // ✅ Email corporativo
    public ContactInfo ContactInfo { get; private set; }     // ✅ Email personal + teléfono
    public UserRole Role { get; private set; }
    
    [JsonIgnore] 
    public string PasswordHash { get; private set; }
    
    public string? ProfileImageUrl { get; private set; }
    public int? ProjectId { get; private set; } // Para supervisores asignados
    public bool IsActive { get; private set; }
    public DateTime? LastLogin { get; private set; }

    // ✅ Propiedades calculadas (tu frontend ya no las necesita)
    public string FullName => Name.FullName;
    public string Initials => Name.Initials;
    public string Email => CorporateEmail.Address; // Para compatibilidad
    public string? PersonalEmail => ContactInfo.PersonalEmailAddress; // ✅ Corregido
    public string? Phone => ContactInfo.Phone;

    public User()
    {
        Name = new PersonName();
        CorporateEmail = new EmailAddress();
        ContactInfo = new ContactInfo();
        Role = new UserRole();
        PasswordHash = string.Empty;
        IsActive = true;
    }

    public User(string firstName, string lastName, string role, string passwordHash, string? personalEmail = null, string? phone = null)
    {
        Name = new PersonName(firstName, lastName);
        CorporateEmail = EmailAddress.GenerateCorporateEmail(Name);
        ContactInfo = new ContactInfo(personalEmail, phone);
        Role = new UserRole(role);
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public User(CreateUserCommand command, string passwordHash)
    {
        Name = new PersonName(command.Name, command.LastName);
        CorporateEmail = EmailAddress.GenerateCorporateEmail(Name);
        ContactInfo = new ContactInfo(command.PersonalEmail, command.Phone);
        Role = new UserRole(command.Role);
        PasswordHash = passwordHash;
        IsActive = true;
    }

    /// <summary>
    /// Update user information with optional parameters
    /// Corporate email is regenerated if name changes
    /// </summary>
    public User UpdateInfo(string? firstName = null, string? lastName = null, string? personalEmail = null, string? phone = null, string? role = null)
    {
        // ✅ Solo actualizar si se proporciona un valor
        if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
        {
            var newFirstName = !string.IsNullOrWhiteSpace(firstName) ? firstName : Name.FirstName;
            var newLastName = !string.IsNullOrWhiteSpace(lastName) ? lastName : Name.LastName;
        
            Name = new PersonName(newFirstName, newLastName);
            CorporateEmail = EmailAddress.GenerateCorporateEmail(Name); // ✅ Regenerar email corporativo
        }
    
        // ✅ Actualizar contacto si se proporciona
        if (personalEmail != null || phone != null)
        {
            var newPersonalEmail = personalEmail ?? ContactInfo.PersonalEmailAddress;
            var newPhone = phone ?? ContactInfo.Phone;
            ContactInfo = new ContactInfo(newPersonalEmail, newPhone);
        }
    
        // ✅ Actualizar rol si se proporciona
        if (!string.IsNullOrWhiteSpace(role))
        {
            Role = new UserRole(role);
        }
    
        return this;
    }

    /**
     * <summary>
     *     Update password hash
     * </summary>
     */
    public User UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }

    /**
     * <summary>
     *     Update profile image URL
     * </summary>
     */
    public User UpdateProfileImage(string profileImageUrl)
    {
        ProfileImageUrl = profileImageUrl;
        return this;
    }

    /**
     * <summary>
     *     Assign project to supervisor
     * </summary>
     */
    public User AssignToProject(int projectId)
    {
        if (!Role.CanBeAssignedToProject)
            throw new InvalidOperationException("Only supervisors can be assigned to projects");
        
        ProjectId = projectId;
        return this;
    }

    /**
     * <summary>
     *     Unassign from project
     * </summary>
     */
    public User UnassignFromProject()
    {
        ProjectId = null;
        return this;
    }

    /**
     * <summary>
     *     Update last login timestamp
     * </summary>
     */
    public User UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        return this;
    }

    /**
     * <summary>
     *     Deactivate user
     * </summary>
     */
    public User Deactivate()
    {
        IsActive = false;
        return this;
    }

    /**
     * <summary>
     *     Activate user
     * </summary>
     */
    public User Activate()
    {
        IsActive = true;
        return this;
    }
}