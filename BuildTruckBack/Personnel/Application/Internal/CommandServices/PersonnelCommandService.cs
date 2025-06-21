using BuildTruckBack.Personnel.Application.ACL.Services;
using BuildTruckBack.Personnel.Domain.Model.Aggregates;
using BuildTruckBack.Personnel.Domain.Model.Commands;
using BuildTruckBack.Personnel.Domain.Repositories;
using BuildTruckBack.Personnel.Domain.Services;
using BuildTruckBack.Shared.Domain.Repositories;

namespace BuildTruckBack.Personnel.Application.Internal.CommandServices;

public class PersonnelCommandService : IPersonnelCommandService
{
    private readonly IPersonnelRepository _personnelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectContextService _projectContextService;
    private readonly ICloudinaryService _cloudinaryService;

    public PersonnelCommandService(
        IPersonnelRepository personnelRepository,
        IUnitOfWork unitOfWork,
        IProjectContextService projectContextService,
        ICloudinaryService cloudinaryService)
    {
        _personnelRepository = personnelRepository;
        _unitOfWork = unitOfWork;
        _projectContextService = projectContextService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Domain.Model.Aggregates.Personnel?> Handle(CreatePersonnelCommand command)
    {
        // Validate project exists
        var projectExists = await _projectContextService.ProjectExistsAsync(command.ProjectId);
        if (!projectExists)
            throw new ArgumentException($"Project with ID {command.ProjectId} does not exist");

        // Validate unique document number
        var documentExists = await _personnelRepository.ExistsByDocumentNumberAsync(
            command.DocumentNumber, command.ProjectId);
        if (documentExists)
            throw new InvalidOperationException($"Document number {command.DocumentNumber} already exists in this project");

        // Validate unique email if provided
        if (!string.IsNullOrEmpty(command.Email))
        {
            var emailExists = await _personnelRepository.ExistsByEmailAsync(
                command.Email, command.ProjectId);
            if (emailExists)
                throw new InvalidOperationException($"Email {command.Email} already exists in this project");
        }

        // Create personnel aggregate
        var personnel = new Domain.Model.Aggregates.Personnel(
            command.ProjectId,
            command.Name,
            command.Lastname,
            command.DocumentNumber,
            command.Position,
            command.Department,
            command.PersonnelType,
            command.Status);

        // Update additional information
        personnel.UpdateFinancialInfo(
            command.MonthlyAmount,
            command.Discount,
            command.Bank,
            command.AccountNumber);

        personnel.UpdateContactInfo(command.Phone, command.Email);
        
        personnel.UpdateContractInfo(
            command.StartDate,
            command.EndDate,
            command.Status);

        // Set avatar URL if provided
        if (!string.IsNullOrEmpty(command.AvatarUrl))
        {
            personnel.UpdateAvatar(command.AvatarUrl);
        }

        try
        {
            await _personnelRepository.AddAsync(personnel);
            await _unitOfWork.CompleteAsync();
            return personnel;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create personnel: {ex.Message}");
        }
    }

    public async Task<Domain.Model.Aggregates.Personnel?> Handle(UpdatePersonnelCommand command)
    {
        var personnel = await _personnelRepository.FindByIdAsync(command.PersonnelId);
        if (personnel == null)
            throw new ArgumentException($"Personnel with ID {command.PersonnelId} not found");

        // Validate unique email if changed and provided
        if (!string.IsNullOrEmpty(command.Email) && command.Email != personnel.Email)
        {
            var emailExists = await _personnelRepository.ExistsByEmailAsync(
                command.Email, personnel.ProjectId, command.PersonnelId);
            if (emailExists)
                throw new InvalidOperationException($"Email {command.Email} already exists in this project");
        }

        try
        {
            // Update personnel information
            personnel.UpdateBasicInfo(
                command.Name,
                command.Lastname,
                command.Position,
                command.Department);

            personnel.UpdateFinancialInfo(
                command.MonthlyAmount,
                command.Discount,
                command.Bank,
                command.AccountNumber);

            personnel.UpdateContactInfo(command.Phone, command.Email);
            
            personnel.UpdateContractInfo(
                command.StartDate,
                command.EndDate,
                command.Status);

            personnel.UpdatePersonnelType(command.PersonnelType);

            // Update avatar URL if provided
            if (!string.IsNullOrEmpty(command.AvatarUrl))
            {
                personnel.UpdateAvatar(command.AvatarUrl);
            }

            _personnelRepository.Update(personnel);
            await _unitOfWork.CompleteAsync();

            return personnel;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update personnel: {ex.Message}");
        }
    }

    public async Task<bool> Handle(UpdateAttendanceCommand command)
    {
        if (!command.PersonnelAttendances.Any())
            return true;

        var personnelIds = command.PersonnelAttendances.Select(p => p.PersonnelId).ToList();
        var personnelList = new List<Domain.Model.Aggregates.Personnel>();

        // Load all personnel
        foreach (var personnelId in personnelIds)
        {
            var personnel = await _personnelRepository.FindByIdAsync(personnelId);
            if (personnel == null)
                throw new ArgumentException($"Personnel with ID {personnelId} not found");
            
            personnelList.Add(personnel);
        }

        try
        {
            // Update attendance for each personnel
            foreach (var attendanceUpdate in command.PersonnelAttendances)
            {
                var personnel = personnelList.First(p => p.Id == attendanceUpdate.PersonnelId);
                
                // Initialize month attendance if needed
                personnel.InitializeMonthAttendance(attendanceUpdate.Year, attendanceUpdate.Month);
                
                // Auto-mark Sundays first
                personnel.AutoMarkSundays(attendanceUpdate.Year, attendanceUpdate.Month);
                
                // Update daily attendance
                foreach (var dailyAttendance in attendanceUpdate.DailyAttendance)
                {
                    personnel.SetDayAttendance(
                        attendanceUpdate.Year,
                        attendanceUpdate.Month,
                        dailyAttendance.Key,
                        dailyAttendance.Value);
                }
                
                // Calculate totals
                personnel.CalculateMonthlyTotals(attendanceUpdate.Year, attendanceUpdate.Month);
            }

            // Batch update
            var success = await _personnelRepository.UpdateAttendanceBatchAsync(personnelList);
            if (success)
            {
                await _unitOfWork.CompleteAsync();
            }

            return success;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to update attendance: {ex.Message}");
        }
    }

    public async Task<bool> Handle(DeletePersonnelCommand command)
    {
        var personnel = await _personnelRepository.FindByIdAsync(command.PersonnelId);
        if (personnel == null)
            return false;

        try
        {
            // Soft delete
            personnel.SoftDelete();
            _personnelRepository.Update(personnel);
            await _unitOfWork.CompleteAsync();

            // Optionally delete avatar from cloud storage
            if (!string.IsNullOrEmpty(personnel.AvatarUrl))
            {
                try
                {
                    await _cloudinaryService.DeletePersonnelPhotoAsync(personnel.AvatarUrl);
                }
                catch
                {
                    // Log but don't fail the operation
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete personnel: {ex.Message}");
        }
    }
}