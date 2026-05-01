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
