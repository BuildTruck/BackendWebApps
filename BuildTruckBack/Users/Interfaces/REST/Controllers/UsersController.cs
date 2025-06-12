using System.Net.Mime;
using BuildTruckBack.Users.Domain.Model.Queries;
using BuildTruckBack.Users.Domain.Services;
using BuildTruckBack.Users.Interfaces.REST.Resources;
using BuildTruckBack.Users.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using BuildTruckBack.Users.Domain.Model.Commands;


namespace BuildTruckBack.Users.Interfaces.REST.Controllers;

/// <summary>
/// Users Controller
/// </summary>
/// <remarks>
/// REST API controller for managing users in BuildTruck platform
/// Only admins can access these endpoints
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User Management endpoints")]
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService) : ControllerBase
{
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createUserResource">The user creation data</param>
    /// <returns>The created user</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new user",
        Description = "Creates a new user in the BuildTruck platform. Corporate email is generated automatically.",
        OperationId = "CreateUser")]
    [SwaggerResponse(StatusCodes.Status201Created, "The user was created successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid user data")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already exists")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserResource createUserResource)
    {
        try
        {
            // ✅ Convert DTO to Command
            var createUserCommand = CreateUserCommandFromResourceAssembler.ToCommandFromResource(createUserResource);
            
            // ✅ Execute business logic
            var user = await userCommandService.Handle(createUserCommand);
            
            // ✅ Convert Entity to DTO
            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return Conflict($"Conflict: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get user by ID",
        Description = "Retrieves a user by its ID",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> GetUserById(int id)
    {
        try
        {
            var getUserByIdQuery = new GetUserByIdQuery(id);
            var user = await userQueryService.Handle(getUserByIdQuery);

            if (user == null)
                return NotFound($"User with ID {id} not found");

            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            return Ok(userResource);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Retrieves all users in the system",
        OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "Users retrieved successfully", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var getAllUsersQuery = new GetAllUsersQuery();
            var users = await userQueryService.Handle(getAllUsersQuery);

            var userResources = UserResourceFromEntityAssembler.ToResourceFromEntity(users);
            return Ok(userResources);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="changePasswordRequest">The password change data</param>
    /// <returns>Success message</returns>
    [HttpPut("{id}/password")]
    [SwaggerOperation(
        Summary = "Change user password", 
        Description = "Allows a user to change their password by providing current and new password",
        OperationId = "ChangeUserPassword")]
    [SwaggerResponse(StatusCodes.Status200OK, "Password changed successfully")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid password data")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Current password is incorrect")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest changePasswordRequest)
    {
        try
        {
            // ✅ Convert DTO to Command
            var changePasswordCommand = new ChangePasswordCommand(id, changePasswordRequest.CurrentPassword, changePasswordRequest.NewPassword);
    
            // ✅ Execute business logic
            await userCommandService.Handle(changePasswordCommand);
    
            return Ok("Password changed successfully");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    /// <summary>
    /// Delete user (physical deletion)
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete user",
        Description = "Permanently delete a user from the system. This action cannot be undone.",
        OperationId = "DeleteUser")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "User deleted successfully")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // ✅ Convert to Command
            var deleteUserCommand = new DeleteUserCommand(id);
            
            // ✅ Execute business logic
            await userCommandService.Handle(deleteUserCommand);
            
            return NoContent(); // 204 - Success with no content
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound($"User with ID {id} not found");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    /// <summary>
    /// Upload or update user profile image
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="file">Image file (JPG, PNG, max 5MB)</param>
    /// <returns>Updated user with new profile image</returns>
    [HttpPost("{id}/profile-image")]
    [SwaggerOperation(
        Summary = "Upload user profile image",
        Description = "Upload or update a user's profile image. Automatically replaces existing image.",
        OperationId = "UploadUserProfileImage")]
    [SwaggerResponse(StatusCodes.Status200OK, "Profile image uploaded successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid image file")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> UploadProfileImage(int id, IFormFile file)
    {
        try
        {
            // ✅ Validate file
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // ✅ Convert to bytes
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            // ✅ Create command and execute
            var uploadCommand = new UploadProfileImageCommand(id, imageBytes, file.FileName);
            var user = await userCommandService.Handle(uploadCommand);

            // ✅ Return updated user
            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            return Ok(userResource);
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete user profile image
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>Updated user without profile image</returns>
    [HttpDelete("{id}/profile-image")]
    [SwaggerOperation(
        Summary = "Delete user profile image",
        Description = "Delete the current profile image for a user.",
        OperationId = "DeleteUserProfileImage")]
    [SwaggerResponse(StatusCodes.Status200OK, "Profile image deleted successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> DeleteProfileImage(int id)
    {
        try
        {
            // ✅ Create command and execute
            var deleteCommand = new DeleteProfileImageCommand(id);
            var user = await userCommandService.Handle(deleteCommand);

            // ✅ Return updated user
            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            return Ok(userResource);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound($"User with ID {id} not found");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    /// <summary>
    /// Update user basic information
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="request">Update user request</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update user information",
        Description = "Update user's basic information. Corporate email is auto-generated if name changes.",
        OperationId = "UpdateUser")]
    [SwaggerResponse(StatusCodes.Status200OK, "User updated successfully", typeof(UserResource))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid data provided")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            // ✅ Create command
            var updateCommand = new UpdateUserCommand(
                id,
                request.Name,
                request.LastName,
                request.PersonalEmail,
                request.Role
            );

            // ✅ Execute command
            var user = await userCommandService.Handle(updateCommand);

            // ✅ Return updated user
            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            return Ok(userResource);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound($"User with ID {id} not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest($"Invalid data: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest($"Operation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}