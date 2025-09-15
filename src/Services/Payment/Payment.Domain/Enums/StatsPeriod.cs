namespace Payment.Domain.Enums;

/// <summary>
/// Périodes pour les statistiques
/// </summary>
public enum StatsPeriod
{
    /// <summary>
    /// Aujourd'hui
    /// </summary>
    Today = 0,

    /// <summary>
    /// Cette semaine
    /// </summary>
    ThisWeek = 1,

    /// <summary>
    /// Ce mois
    /// </summary>
    ThisMonth = 2,

    /// <summary>
    /// Ce trimestre
    /// </summary>
    ThisQuarter = 3,

    /// <summary>
    /// Cette année
    /// </summary>
    ThisYear = 4,

    /// <summary>
    /// Les 7 derniers jours
    /// </summary>
    Last7Days = 5,

    /// <summary>
    /// Les 30 derniers jours
    /// </summary>
    Last30Days = 6,

    /// <summary>
    /// Les 90 derniers jours
    /// </summary>
    Last90Days = 7,

    /// <summary>
    /// Les 12 derniers mois
    /// </summary>
    Last12Months = 8,

    /// <summary>
    /// Période personnalisée
    /// </summary>
    Custom = 9
}

/// <summary>
/// Extensions pour StatsPeriod
/// </summary>
public static class StatsPeriodExtensions
{
    /// <summary>
    /// Obtenir la plage de dates pour la période
    /// </summary>
    public static (DateTime startDate, DateTime endDate) GetDateRange(this StatsPeriod period, DateTime? customStartDate = null, DateTime? customEndDate = null)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return period switch
        {
            StatsPeriod.Today => (today, today.AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.ThisWeek => (today.AddDays(-(int)today.DayOfWeek), today.AddDays(7 - (int)today.DayOfWeek).AddMilliseconds(-1)),
            StatsPeriod.ThisMonth => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.ThisQuarter => GetQuarterRange(now),
            StatsPeriod.ThisYear => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31).AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.Last7Days => (today.AddDays(-7), today.AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.Last30Days => (today.AddDays(-30), today.AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.Last90Days => (today.AddDays(-90), today.AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.Last12Months => (today.AddMonths(-12), today.AddDays(1).AddMilliseconds(-1)),
            StatsPeriod.Custom => (customStartDate ?? today, customEndDate ?? today.AddDays(1).AddMilliseconds(-1)),
            _ => (today, today.AddDays(1).AddMilliseconds(-1))
        };
    }

    /// <summary>
    /// Obtenir la plage du trimestre actuel
    /// </summary>
    private static (DateTime startDate, DateTime endDate) GetQuarterRange(DateTime date)
    {
        var quarter = (date.Month - 1) / 3 + 1;
        var startMonth = (quarter - 1) * 3 + 1;
        var startDate = new DateTime(date.Year, startMonth, 1);
        var endDate = startDate.AddMonths(3).AddMilliseconds(-1);
        return (startDate, endDate);
    }

    /// <summary>
    /// Obtenir le nom affiché de la période
    /// </summary>
    public static string GetDisplayName(this StatsPeriod period) => period switch
    {
        StatsPeriod.Today => "Aujourd'hui",
        StatsPeriod.ThisWeek => "Cette semaine",
        StatsPeriod.ThisMonth => "Ce mois",
        StatsPeriod.ThisQuarter => "Ce trimestre",
        StatsPeriod.ThisYear => "Cette année",
        StatsPeriod.Last7Days => "7 derniers jours",
        StatsPeriod.Last30Days => "30 derniers jours",
        StatsPeriod.Last90Days => "90 derniers jours",
        StatsPeriod.Last12Months => "12 derniers mois",
        StatsPeriod.Custom => "Période personnalisée",
        _ => "Inconnu"
    };
}