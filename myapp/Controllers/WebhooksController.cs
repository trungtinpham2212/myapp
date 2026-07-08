using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;
using myapp.Hubs;

namespace myapp.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<PaymentHub> _hubContext;
    private readonly INotificationService _notificationService;

    public WebhooksController(IUnitOfWork unitOfWork, IConfiguration configuration, IHubContext<PaymentHub> hubContext, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    [HttpPost("sepay")]
    public async Task<IActionResult> SepayWebhook([FromBody] SepayWebhookDto payload)
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        var expectedApiKey = _configuration["Sepay:ApiKey"];

        if (string.IsNullOrEmpty(authHeader) || authHeader != $"Apikey {expectedApiKey}")
        {
            return Unauthorized(new { success = false, message = "Unauthorized webhook request" });
        }

        if (payload == null || string.IsNullOrEmpty(payload.Content))
        {
            return BadRequest(new { success = false, message = "Invalid payload" });
        }

        // Lấy mã đơn hàng từ Code của Sepay (ưu tiên) hoặc dùng Regex bóc từ Content
        string orderCode = payload.Code;
        if (string.IsNullOrEmpty(orderCode) && !string.IsNullOrEmpty(payload.Content))
        {
            var match = Regex.Match(payload.Content, @"DH(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                orderCode = match.Value; // DH123
            }
        }

        if (string.IsNullOrEmpty(orderCode) || !orderCode.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
        {
            // Trả về 200 OK để Sepay không gọi lại webhook nữa nếu giao dịch này không thuộc về hệ thống mình
            return Ok(new { success = true, message = "Not an order payment, ignoring" });
        }

        string orderIdStr = orderCode.Substring(2); // Cắt chữ DH đi để lấy số
        if (!long.TryParse(orderIdStr, out long orderId))
        {
            return Ok(new { success = true, message = "Invalid order ID format" });
        }

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return Ok(new { success = true, message = "Order not found" });
        }

        if (order.PaymentStatus == "Success")
        {
            return Ok(new { success = true, message = "Order already paid" });
        }

        // Kiểm tra xem số tiền chuyển khoản có đủ với tổng tiền đơn hàng không
        if (payload.TransferAmount < order.FinalAmount)
        {
            return Ok(new { success = true, message = "Transfer amount is less than order amount" });
        }

        // Cập nhật trạng thái Order
        order.PaymentStatus = "Success";
        order.OrderStatus = "Success"; 
        order.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.OrderRepository.Update(order);

        // Cập nhật trạng thái Payment
        var payments = await _unitOfWork.PaymentRepository.GetAllAsync();
        var payment = payments.FirstOrDefault(p => p.OrderId == orderId && p.PaymentMethod == "BankTransfer");
        
        if (payment != null)
        {
            payment.PaymentStatus = "Success";
            payment.TransactionReference = payload.ReferenceCode ?? payload.Code ?? payload.Id.ToString();
            payment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PaymentRepository.Update(payment);
        }

        await _unitOfWork.SaveChangesAsync();

        // Push Notification to Admin (Đơn hàng đã thanh toán)
        await _notificationService.PushNotificationAsync(
            userId: null,
            title: "Đơn hàng đã thanh toán",
            content: $"Đơn hàng #{order.OrderId} đã được thanh toán thành công qua chuyển khoản.",
            type: "success",
            targetType: "Order",
            targetId: order.OrderId.ToString()
        );

        // Push Notification to User (nếu có UserId)
        if (order.UserId != Guid.Empty)
        {
            await _notificationService.PushNotificationAsync(
                userId: order.UserId,
                title: "Thanh toán thành công",
                content: $"Đơn hàng #{order.OrderId} của bạn đã được thanh toán thành công.",
                type: "success",
                targetType: "Order",
                targetId: order.OrderId.ToString()
            );
        }

        // Logic check Low Stock (sản phẩm sắp hết hàng)
        var orderWithDetails = await _unitOfWork.OrderRepository.GetOrderWithDetailsAsync(order.OrderId);
        if (orderWithDetails != null && orderWithDetails.OrderDetails != null)
        {
            foreach (var detail in orderWithDetails.OrderDetails)
            {
                var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(detail.ProductVariantId);
                if (variant != null && variant.StockQuantity < 5)
                {
                    var product = await _unitOfWork.ProductRepository.GetByIdAsync(variant.ProductId);
                    string productName = product != null ? product.Name : $"Variant {variant.ProductVariantId}";
                    
                    await _notificationService.PushNotificationAsync(
                        userId: null, // Admin only
                        title: "Sản phẩm sắp hết hàng",
                        content: $"{productName} ({variant.Color}, {variant.Storage}) (SL: {variant.StockQuantity})",
                        type: "warning",
                        targetType: "Product",
                        targetId: variant.ProductId.ToString()
                    );
                }
            }
        }

        // Gửi SignalR tới FE thông báo đơn hàng đã thanh toán thành công
        await _hubContext.Clients.All.SendAsync("PaymentSuccess", orderId);

        return Ok(new { success = true, message = "Thanh toán thành công" });
    }
}
