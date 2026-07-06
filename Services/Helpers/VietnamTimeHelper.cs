namespace Services.Helpers;
public static class VietnamTimeHelper
{
    public static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    public static DateTime Now()
    {
        var vn = DateTimeOffset.UtcNow.ToOffset(VietnamOffset);
        return DateTime.SpecifyKind(vn.DateTime, DateTimeKind.Unspecified);
    }
    
    public static DateTime FromUtc(DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc)
            utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var dto = new DateTimeOffset(utc).ToOffset(VietnamOffset);
        return DateTime.SpecifyKind(dto.DateTime, DateTimeKind.Unspecified);
    }
}
