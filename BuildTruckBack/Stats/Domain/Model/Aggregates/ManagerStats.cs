namespace BuildTruckBack.Stats.Domain.Model.Aggregates;

using BuildTruckBack.Stats.Domain.Model.ValueObjects;

/// <summary>
/// ManagerStats Aggregate Root
/// Represents comprehensive statistics for a manager across all their projects
/// </summary>
public partial class ManagerStats
{
    public int Id { get; private set; }
    public int ManagerId { get; private set; }
    public StatsPeriod Period { get; private set; } = null!;
    
    // Core metrics
    public ProjectMetrics ProjectMetrics { get; private set; } = null!;
    public PersonnelMetrics PersonnelMetrics { get; private set; } = null!;
    public IncidentMetrics IncidentMetrics { get; private set; } = null!;
    public MaterialMetrics MaterialMetrics { get; private set; } = null!;
    public MachineryMetrics MachineryMetrics { get; private set; } = null!;
    
    // Performance indicators
    public decimal OverallPerformanceScore { get; private set; }
    public string PerformanceGrade { get; private set; } = string.Empty;
    public List<string> Alerts { get; private set; } = new();
    public List<string> Recommendations { get; private set; } = new();
    
    // Metadata
    public DateTime CalculatedAt { get; private set; }
    public bool IsCurrentPeriod { get; private set; }
    public string CalculationSource { get; private set; } = string.Empty;

    // Constructor for EF Core
    protected ManagerStats() { }

    public ManagerStats(
        int managerId,
        StatsPeriod period,
        ProjectMetrics projectMetrics,
        PersonnelMetrics personnelMetrics,
        IncidentMetrics incidentMetrics,
        MaterialMetrics materialMetrics,
        MachineryMetrics machineryMetrics)
    {
        ManagerId = managerId > 0 ? managerId : throw new ArgumentException("ManagerId must be greater than 0", nameof(managerId));
        Period = period ?? throw new ArgumentNullException(nameof(period));
        ProjectMetrics = projectMetrics ?? throw new ArgumentNullException(nameof(projectMetrics));
        PersonnelMetrics = personnelMetrics ?? throw new ArgumentNullException(nameof(personnelMetrics));
        IncidentMetrics = incidentMetrics ?? throw new ArgumentNullException(nameof(incidentMetrics));
        MaterialMetrics = materialMetrics ?? throw new ArgumentNullException(nameof(materialMetrics));
        MachineryMetrics = machineryMetrics ?? throw new ArgumentNullException(nameof(machineryMetrics));
        
        CalculatedAt = DateTime.UtcNow.AddHours(-5); // Peru timezone
        IsCurrentPeriod = period.IsCurrentPeriod();
        CalculationSource = "REAL_TIME";
        
        CalculatePerformanceScore();
        GenerateAlertsAndRecommendations();
    }

    // Business methods
    public void RecalculateMetrics(
        ProjectMetrics projectMetrics,
        PersonnelMetrics personnelMetrics,
        IncidentMetrics incidentMetrics,
        MaterialMetrics materialMetrics,
        MachineryMetrics machineryMetrics)
    {
        ProjectMetrics = projectMetrics ?? throw new ArgumentNullException(nameof(projectMetrics));
        PersonnelMetrics = personnelMetrics ?? throw new ArgumentNullException(nameof(personnelMetrics));
        IncidentMetrics = incidentMetrics ?? throw new ArgumentNullException(nameof(incidentMetrics));
        MaterialMetrics = materialMetrics ?? throw new ArgumentNullException(nameof(materialMetrics));
        MachineryMetrics = machineryMetrics ?? throw new ArgumentNullException(nameof(machineryMetrics));
        
        CalculatedAt = DateTime.UtcNow.AddHours(-5);
        CalculationSource = "RECALCULATED";
        
        CalculatePerformanceScore();
        GenerateAlertsAndRecommendations();
    }

    public void UpdatePeriod(StatsPeriod newPeriod)
    {
        Period = newPeriod ?? throw new ArgumentNullException(nameof(newPeriod));
        IsCurrentPeriod = newPeriod.IsCurrentPeriod();
    }

    // Performance calculation
    private void CalculatePerformanceScore()
    {
        var weights = new Dictionary<string, decimal>
        {
            ["projects"] = 0.25m,
            ["personnel"] = 0.20m,
            ["safety"] = 0.30m,
            ["materials"] = 0.15m,
            ["machinery"] = 0.10m
        };

        var projectScore = ProjectMetrics.GetCompletionRate();
        var personnelScore = PersonnelMetrics.GetEfficiencyScore();
        var safetyScore = IncidentMetrics.GetSafetyScore();
        var materialScore = MaterialMetrics.GetInventoryHealthScore();
        var machineryScore = MachineryMetrics.GetEfficiencyScore();

        OverallPerformanceScore = Math.Round(
            (projectScore * weights["projects"]) +
            (personnelScore * weights["personnel"]) +
            (safetyScore * weights["safety"]) +
            (materialScore * weights["materials"]) +
            (machineryScore * weights["machinery"]), 2);

        PerformanceGrade = OverallPerformanceScore switch
        {
            >= 90m => "A",
            >= 80m => "B",
            >= 70m => "C",
            >= 60m => "D",
            _ => "F"
        };
    }

    // Alerts and recommendations generation
    private void GenerateAlertsAndRecommendations()
    {
        var alerts = new List<string>();
        var recommendations = new List<string>();

        // Critical incidents alert
        if (IncidentMetrics.HasCriticalIncidents())
        {
            alerts.Add($"🚨 {IncidentMetrics.CriticalIncidents} incidente(s) crítico(s) requieren atención inmediata");
            recommendations.Add("Revisar protocolos de seguridad y implementar medidas correctivas");
        }

        // Material stock alerts
        if (MaterialMetrics.NeedsRestocking())
        {
            alerts.Add($"📦 {MaterialMetrics.MaterialsOutOfStock} materiales agotados y {MaterialMetrics.MaterialsLowStock} con bajo stock");
            recommendations.Add("Planificar reabastecimiento de materiales críticos");
        }

        // Machinery maintenance alerts
        if (MachineryMetrics.NeedsMaintenance())
        {
            alerts.Add($"🔧 {MachineryMetrics.InMaintenanceMachinery} máquinas en mantenimiento ({MachineryMetrics.GetMaintenanceRate():F1}%)");
            recommendations.Add("Optimizar calendario de mantenimiento preventivo");
        }

        // Personnel efficiency
        if (PersonnelMetrics.GetActiveRate() < 80m)
        {
            alerts.Add($"👥 Solo {PersonnelMetrics.GetActiveRate():F1}% del personal está activo");
            recommendations.Add("Revisar asignaciones de personal y optimizar distribución");
        }

        // Project completion
        if (ProjectMetrics.GetCompletionRate() < 60m)
        {
            alerts.Add($"📋 Baja tasa de completación de proyectos ({ProjectMetrics.GetCompletionRate():F1}%)");
            recommendations.Add("Evaluar cronogramas y recursos asignados a proyectos");
        }

        // Performance-based recommendations
        if (OverallPerformanceScore < 70m)
        {
            recommendations.Add("Implementar reuniones semanales de seguimiento");
            recommendations.Add("Considerar capacitación adicional para el equipo");
        }

        Alerts = alerts;
        Recommendations = recommendations;
    }

    // Query methods
    public bool HasCriticalAlerts()
    {
        return IncidentMetrics.HasCriticalIncidents() || 
               MaterialMetrics.GetOutOfStockRate() > 20m ||
               MachineryMetrics.GetActiveRate() < 50m;
    }

    public string GetOverallStatus()
    {
        return OverallPerformanceScore switch
        {
            >= 90m => "Excelente",
            >= 80m => "Bueno",
            >= 70m => "Regular",
            >= 60m => "Bajo",
            _ => "Crítico"
        };
    }

    public Dictionary<string, decimal> GetScoreBreakdown()
    {
        return new Dictionary<string, decimal>
        {
            ["Proyectos"] = ProjectMetrics.GetCompletionRate(),
            ["Personal"] = PersonnelMetrics.GetEfficiencyScore(),
            ["Seguridad"] = IncidentMetrics.GetSafetyScore(),
            ["Materiales"] = MaterialMetrics.GetInventoryHealthScore(),
            ["Maquinaria"] = MachineryMetrics.GetEfficiencyScore(),
            ["General"] = OverallPerformanceScore
        };
    }

    public string GetSummaryReport()
    {
        var report = $"Reporte de Gestión - {Period.DisplayName}\n";
        report += $"Rendimiento General: {OverallPerformanceScore:F1}% (Grado {PerformanceGrade})\n\n";
        
        report += "📊 Métricas Clave:\n";
        report += $"• Proyectos: {ProjectMetrics.GetStatusSummary()}\n";
        report += $"• Personal: {PersonnelMetrics.GetPersonnelSummary()}\n";
        report += $"• Seguridad: {IncidentMetrics.GetIncidentSummary()}\n";
        report += $"• Materiales: {MaterialMetrics.GetMaterialSummary()}\n";
        report += $"• Maquinaria: {MachineryMetrics.GetMachinerySummary()}\n\n";

        if (Alerts.Any())
        {
            report += "⚠️ Alertas:\n";
            foreach (var alert in Alerts)
            {
                report += $"• {alert}\n";
            }
            report += "\n";
        }

        if (Recommendations.Any())
        {
            report += "💡 Recomendaciones:\n";
            foreach (var recommendation in Recommendations)
            {
                report += $"• {recommendation}\n";
            }
        }

        return report;
    }

    // Comparison methods
    public ManagerStats CompareTo(ManagerStats previousStats)
    {
        // This would return a new ManagerStats with comparison data
        // Implementation would calculate differences and trends
        throw new NotImplementedException("Comparison functionality to be implemented");
    }

    public bool IsImprovement(ManagerStats previousStats)
    {
        if (previousStats == null) return true;
        return OverallPerformanceScore > previousStats.OverallPerformanceScore;
    }

    // Validation
    public bool IsValid()
    {
        return ManagerId > 0 &&
               Period != null &&
               ProjectMetrics != null &&
               PersonnelMetrics != null &&
               IncidentMetrics != null &&
               MaterialMetrics != null &&
               MachineryMetrics != null;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (ManagerId <= 0)
            errors.Add("ManagerId must be greater than 0");

        if (Period == null)
            errors.Add("Period is required");

        if (ProjectMetrics == null)
            errors.Add("ProjectMetrics is required");

        if (PersonnelMetrics == null)
            errors.Add("PersonnelMetrics is required");

        if (IncidentMetrics == null)
            errors.Add("IncidentMetrics is required");

        if (MaterialMetrics == null)
            errors.Add("MaterialMetrics is required");

        if (MachineryMetrics == null)
            errors.Add("MachineryMetrics is required");

        return errors;
    }

    public override string ToString()
    {
        return $"ManagerStats[{ManagerId}] {Period.DisplayName} - Score: {OverallPerformanceScore:F1}% ({PerformanceGrade})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ManagerStats other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id > 0 && other.Id > 0 && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id > 0 ? Id.GetHashCode() : base.GetHashCode();
    }
}