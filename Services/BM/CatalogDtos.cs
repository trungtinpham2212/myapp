using System.Text.Json.Serialization;

namespace Services.BM;

public class CategoryDto
{
    [JsonPropertyName("category_id")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class CreateUpdateCategoryDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class BrandDto
{
    [JsonPropertyName("brand_id")]
    public int BrandId { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }
}

public class CreateUpdateBrandDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }
}
