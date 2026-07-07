using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Repositories.UnitOfWork;
using Services.BM;
using myapp.Hubs;

namespace myapp.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<PaymentHub> _hubContext;

    public WebhooksController(IUnitOfWork unitOfWork, IConfiguration configuration, IHubContext<PaymentHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _hubContext = hubContext;
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

        // Tìm nội dung chuyển khoản chứa cú pháp DH{OrderId} (Ví dụ: DH1005)
        var match = Regex.Match(payload.Content, @"DH(\d+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            // Trả về 200 OK để Sepay không gọi lại webhook nữa nếu giao dịch này không thuộc về hệ thống mình
            return Ok(new { success = true, message = "Not an order payment, ignoring" });
        }

        if (!long.TryParse(match.Groups[1].Value, out long orderId))
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

        // Gửi SignalR tới FE thông báo đơn hàng đã thanh toán thành công
        await _hubContext.Clients.All.SendAsync("PaymentSuccess", orderId);

        return Ok(new { success = true, message = "Thanh toán thành công" });
    }
}
