using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Services.BM;

public class CartDto
{
    [JsonPropertyName("cart_id")]
    public long CartId { get; set; }

    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }

    [JsonPropertyName("provisional_amount")]
    public decimal ProvisionalAmount { get; set; }

    [JsonPropertyName("shipping_fee")]
    public decimal ShippingFee { get; set; }

    [JsonPropertyName("final_amount")]
    public decimal FinalAmount { get; set; }

    [JsonPropertyName("items")]
    public List<CartItemDto> Items { get; set; } = new();
}

public class CartItemDto
{
    [JsonPropertyName("cart_item_id")]
    public long CartItemId { get; set; }

    [JsonPropertyName("product_variant_id")]
    public long ProductVariantId { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("storage")]
    public string? Storage { get; set; }

    [JsonPropertyName("sale_price")]
    public decimal SalePrice { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("added_at")]
    public DateTime? AddedAt { get; set; }
}

public class AddToCartRequest
{
    [JsonPropertyName("product_variant_id")]
    public long ProductVariantId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;
}
