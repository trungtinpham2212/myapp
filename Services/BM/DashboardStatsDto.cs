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

public class RevenueByCategoryDto
{
    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }

    [JsonPropertyName("category_name")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("percentage")]
    public decimal Percentage { get; set; }
}
