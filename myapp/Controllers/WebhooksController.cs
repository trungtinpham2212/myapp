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
    private readonly IOrderService _orderService;
    private readonly Microsoft.AspNetCore.SignalR.IHubContext<Hubs.PaymentHub> _hubContext;

    public WebhooksController(
        IOrderService orderService,
        Microsoft.AspNetCore.SignalR.IHubContext<Hubs.PaymentHub> hubContext)
    {
        _orderService = orderService;
        _hubContext = hubContext;
    }

    [HttpPost("sepay")]
    public async Task<IActionResult> SePayWebhook([FromBody] Services.BM.SepayWebhookDto payload)
    {
        var apiKey = Request.Headers["Authorization"].ToString().Replace("Apikey ", "");
        var configApiKey = "R0NDQVRIS0ZPVFNFWklIRkJITkhIRUFTVks0UDJUVE9RTVRQQUk4Tk9LQTFVUjdP";
        if (apiKey != configApiKey)
        {
            return Unauthorized(new { success = false, message = "Invalid API Key" });
        }

        string orderCode = payload.Content; 
        if (string.IsNullOrEmpty(orderCode) || !orderCode.StartsWith("DH"))
        {
            return Ok(new { success = true, message = "Not an order payment, ignoring" });
        }

        string referenceCode = payload.ReferenceCode ?? payload.Code ?? payload.Id.ToString();
        var result = await _orderService.ProcessWebhookPaymentAsync(orderCode, payload.TransferAmount, referenceCode);

        if (result.Success)
        {
            if (long.TryParse(result.Data, out long orderId))
            {
                await _hubContext.Clients.All.SendAsync("PaymentSuccess", orderId);
            }
            return Ok(new { success = true, message = result.Message });
        }
        
        return Ok(new { success = true, message = result.Message });
    }
}
