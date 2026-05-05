namespace Services.Utilities;

public static class BusinessDateTime
{
    public const string DefaultTimeZoneId = "America/Argentina/Buenos_Aires";

    public static DateTime GetBusinessDate(DateTime utcNow, string? timeZoneId)
    {
        var timeZone = ResolveTimeZone(timeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), timeZone);
        return DateTime.SpecifyKind(local.Date, DateTimeKind.Unspecified);
    }

    public static DateTime ConvertBusinessDateToUtc(DateTime businessDate, string? timeZoneId)
    {
        var timeZone = ResolveTimeZone(timeZoneId);
        var localDate = DateTime.SpecifyKind(businessDate.Date, DateTimeKind.Unspecified);
        var utcDate = TimeZoneInfo.ConvertTimeToUtc(localDate, timeZone);
        return DateTime.SpecifyKind(utcDate.Date, DateTimeKind.Utc);
    }

    public static (DateTime StartUtc, DateTime EndUtc) GetUtcRangeForBusinessDate(DateTime businessDate, string? timeZoneId)
    {
        var timeZone = ResolveTimeZone(timeZoneId);
        var localStart = DateTime.SpecifyKind(businessDate.Date, DateTimeKind.Unspecified);
        var localEnd = localStart.AddDays(1);

        return (
            TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone),
            TimeZoneInfo.ConvertTimeToUtc(localEnd, timeZone).AddTicks(-1)
        );
    }

    /// <summary>
    /// Calcula próxima fecha de billing sumando período en timezone local.
    /// Evita errores de 1 día al sumar meses en UTC.
    /// </summary>
    /// <param name="currentPeriodEnd">Fecha de fin del período actual (UTC)</param>
    /// <param name="monthsToAdd">Meses a sumar (típicamente 1 para mensual)</param>
    /// <param name="timeZoneId">Timezone de la empresa</param>
    /// <returns>Próxima fecha de billing en UTC</returns>
    public static DateTime CalculateNextBillingDate(DateTime currentPeriodEnd, int monthsToAdd, string? timeZoneId)
    {
        var localEnd = GetBusinessDate(currentPeriodEnd, timeZoneId);
        var nextLocal = localEnd.AddMonths(monthsToAdd);
        return ConvertBusinessDateToUtc(nextLocal, timeZoneId);
    }

    /// <summary>
    /// Verifica si trial expiró en timezone de la empresa.
    /// Compara fechas de negocio, no UTC directo.
    /// </summary>
    /// <param name="trialEndsAt">Fecha de expiración del trial (UTC)</param>
    /// <param name="timeZoneId">Timezone de la empresa</param>
    /// <returns>True si el trial ya expiró en la zona horaria local</returns>
    public static bool IsTrialExpired(DateTime? trialEndsAt, string? timeZoneId)
    {
        if (!trialEndsAt.HasValue) return false;

        var now = DateTime.UtcNow;
        var localNow = GetBusinessDate(now, timeZoneId);
        var localExpiry = GetBusinessDate(trialEndsAt.Value, timeZoneId);

        return localNow > localExpiry;
    }

    /// <summary>
    /// Verifica si período de suscripción está activo en timezone de la empresa.
    /// Compara fechas de negocio, no UTC directo.
    /// </summary>
    /// <param name="periodEnd">Fecha de fin del período (UTC)</param>
    /// <param name="timeZoneId">Timezone de la empresa</param>
    /// <returns>True si el período todavía está activo en la zona horaria local</returns>
    public static bool IsPeriodActive(DateTime periodEnd, string? timeZoneId)
    {
        var now = DateTime.UtcNow;
        var localNow = GetBusinessDate(now, timeZoneId);
        var localEnd = GetBusinessDate(periodEnd, timeZoneId);

        return localNow <= localEnd;
    }

    private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
    {
        var resolvedId = string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZoneId : timeZoneId;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(resolvedId);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}
