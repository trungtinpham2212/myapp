using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Services.BM;

public class CreateOrderRequest
{
    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("shipping_address")]
    public string? ShippingAddress { get; set; }

    [JsonPropertyName("shipping_phone")]
    public string? ShippingPhone { get; set; }
}

public class CreateOrderResponseDto
{
    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("order_status")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("payment_status")]
    public string? PaymentStatus { get; set; }

    [JsonPropertyName("final_amount")]
    public decimal FinalAmount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("qr_code_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? QrCodeUrl { get; set; }
}

public class OrderDto
{
    [JsonPropertyName("order_id")]
    public long OrderId { get; set; }

    [JsonPropertyName("order_status")]
    public string? OrderStatus { get; set; }

    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("payment_status")]
    public string? PaymentStatus { get; set; }

    [JsonPropertyName("provisional_amount")]
    public decimal ProvisionalAmount { get; set; }

    [JsonPropertyName("shipping_fee")]
    public decimal? ShippingFee { get; set; }

    [JsonPropertyName("final_amount")]
    public decimal FinalAmount { get; set; }

    [JsonPropertyName("shipping_address")]
    public string? ShippingAddress { get; set; }

    [JsonPropertyName("shipping_phone")]
    public string? ShippingPhone { get; set; }

    [JsonPropertyName("details")]
    public List<OrderDetailDto> Details { get; set; } = new();
}

public class OrderDetailDto
{
    [JsonPropertyName("order_detail_id")]
    public long OrderDetailId { get; set; }

    [JsonPropertyName("product_variant_id")]
    public long ProductVariantId { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("price_at_purchase")]
    public decimal PriceAtPurchase { get; set; }
}
