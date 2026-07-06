using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Services.BM;

public class ProductDto
{
    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("discount_percentage")]
    public int? DiscountPercentage { get; set; }

    [JsonPropertyName("min_price")]
    public decimal? MinPrice { get; set; }

    [JsonPropertyName("max_price")]
    public decimal? MaxPrice { get; set; }

    [JsonPropertyName("average_rating")]
    public decimal? AverageRating { get; set; }

    [JsonPropertyName("total_reviews")]
    public int? TotalReviews { get; set; }

    [JsonPropertyName("is_featured")]
    public bool? IsFeatured { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class ProductQuickViewDto
{
    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("variants")]
    public List<VariantDto> Variants { get; set; } = new();

    [JsonPropertyName("images")]
    public List<ImageDto> Images { get; set; } = new();
}

public class VariantDto
{
    [JsonPropertyName("product_variant_id")]
    public long ProductVariantId { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("storage")]
    public string Storage { get; set; } = string.Empty;

    [JsonPropertyName("original_price")]
    public decimal OriginalPrice { get; set; }

    [JsonPropertyName("discount_percentage")]
    public int? DiscountPercentage { get; set; }

    [JsonPropertyName("sale_price")]
    public decimal SalePrice { get; set; }

    [JsonPropertyName("stock_quantity")]
    public int? StockQuantity { get; set; }
}

public class ImageDto
{
    [JsonPropertyName("product_image_id")]
    public long ProductImageId { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("sort_order")]
    public int? SortOrder { get; set; }
}
