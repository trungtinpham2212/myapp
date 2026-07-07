using System.Text.Json.Serialization;

namespace Services.BM;

public class DashboardStatsDto
{
    [JsonPropertyName("total_new_customers")]
    public int TotalNewCustomers { get; set; }

    [JsonPropertyName("total_revenue")]
    public decimal TotalRevenue { get; set; }

    [JsonPropertyName("total_successful_orders")]
    public int TotalSuccessfulOrders { get; set; }
}

public class RevenueByDayDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }
}
