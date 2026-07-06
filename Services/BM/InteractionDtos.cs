using System;
using System.Text.Json.Serialization;

namespace Services.BM;

public class CreateReviewRequest
{
    [JsonPropertyName("rating_stars")]
    public int RatingStars { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

public class ToggleWishlistRequest
{
    [JsonPropertyName("product_id")]
    public long ProductId { get; set; }
}

public class ToggleWishlistResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("is_favorite")]
    public bool IsFavorite { get; set; }
}

public class ReviewDto
{
    [JsonPropertyName("review_id")]
    public long ReviewId { get; set; }

    [JsonPropertyName("rating_stars")]
    public int? RatingStars { get; set; }

    [JsonPropertyName("comment")]
    public string? Comment { get; set; }

    [JsonPropertyName("user_name")]
    public string? UserName { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}
