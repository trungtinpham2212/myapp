using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace myapp.Hubs;

public class PaymentHub : Hub
{
    // Clients có thể gọi hàm này hoặc chỉ dùng để server push về client
}
