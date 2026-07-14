using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public OrderService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<CreateOrderResponseDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new ApiResponse<CreateOrderResponseDto> { Success = false, Message = "Giỏ hàng trống" };
            }

            var cartItems = await _unitOfWork.CartItemRepository.GetCartItemsWithDetailsAsync(cart.CartId);

            if (!cartItems.Any())
            {
                return new ApiResponse<CreateOrderResponseDto> { Success = false, Message = "Giỏ hàng trống" };
            }

            decimal provisionalAmount = 0;
            foreach (var ci in cartItems)
            {
                var variant = ci.ProductVariant;
                if (variant == null)
                {
                    return new ApiResponse<CreateOrderResponseDto> { Success = false, Message = "Sản phẩm trong giỏ không tồn tại" };
                }

                int qty = ci.Quantity ?? 0;
                if ((variant.StockQuantity ?? 0) < qty)
                {
                    return new ApiResponse<CreateOrderResponseDto>
                    {
                        Success = false,
                        Message = $"Biến thể sản phẩm (ID: {variant.ProductVariantId}) không đủ số lượng tồn kho"
                    };
                }

                provisionalAmount += qty * variant.SalePrice;
            }

            decimal shippingFee = cart.ShippingFee ?? 0;
            decimal finalAmount = provisionalAmount + shippingFee;

            bool isCod = (request.PaymentMethod ?? "COD") == "COD";

            var order = new Order
            {
                UserId = userId,
                OrderStatus = isCod ? "Success" : "Processing",
                PaymentMethod = request.PaymentMethod ?? "COD",
                PaymentStatus = isCod ? "Success" : "Pending",
                ProvisionalAmount = provisionalAmount,
                ShippingFee = shippingFee,
                FinalAmount = finalAmount,
                ShippingAddress = request.ShippingAddress,
                ShippingPhone = request.ShippingPhone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.OrderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            string? qrCodeUrl = null;
            if (order.PaymentMethod == "BankTransfer")
            {
                qrCodeUrl = $"https://vietqr.app/img?bank=MBBank&acc=55501012004&template=compact&showinfo=true&holder=PHAM TRUNG TIN&store=Myapp&amount={(long)finalAmount}&des=DH{order.OrderId}";
                
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "BankTransfer",
                    Amount = finalAmount,
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.PaymentRepository.AddAsync(payment);
            }
            else
            {
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = order.PaymentMethod ?? "COD",
                    Amount = finalAmount,
                    PaymentStatus = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await _unitOfWork.PaymentRepository.AddAsync(payment);
            }

            foreach (var ci in cartItems)
            {
                var variant = ci.ProductVariant;
                int qty = ci.Quantity ?? 0;

                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductVariantId = variant.ProductVariantId,
                    Quantity = qty,
                    PriceAtPurchase = variant.SalePrice,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.OrderDetailRepository.AddAsync(detail);

                variant.StockQuantity = (variant.StockQuantity ?? 0) - qty;
                variant.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.ProductVariantRepository.Update(variant);

                _unitOfWork.CartItemRepository.Delete(ci);
            }

            cart.TotalItems = 0;
            cart.ProvisionalAmount = 0;
            cart.FinalAmount = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CartRepository.Update(cart);

            await _unitOfWork.SaveChangesAsync();

            // Push Notification cho đơn hàng COD
            if (order.PaymentMethod == "COD")
            {
                // Cho Admin
                await _notificationService.PushNotificationAsync(
                    userId: null,
                    title: "Đơn đặt hàng mới (COD)",
                    content: $"Khách hàng vừa đặt đơn hàng #{order.OrderId} với phương thức thanh toán COD.",
                    type: "info",
                    targetType: "Order",
                    targetId: order.OrderId.ToString()
                );

                // Cho Khách hàng
                await _notificationService.PushNotificationAsync(
                    userId: userId,
                    title: "Đặt hàng thành công",
                    content: $"Đơn hàng #{order.OrderId} của bạn đã được ghi nhận thành công (Thanh toán khi nhận hàng).",
                    type: "success",
                    targetType: "Order",
                    targetId: order.OrderId.ToString()
                );
            }

            // Push Notification cảnh báo tồn kho
            foreach (var ci in cartItems)
            {
                var variant = ci.ProductVariant;
                if (variant != null && variant.StockQuantity < 5)
                {
                    string productName = variant.Product?.Name ?? $"Variant {variant.ProductVariantId}";
                    
                    await _notificationService.PushNotificationAsync(
                        userId: null,
                        title: "Sản phẩm sắp hết hàng",
                        content: $"{productName} ({variant.Color}, {variant.Storage}) (SL: {variant.StockQuantity})",
                        type: "warning",
                        targetType: "Product",
                        targetId: variant.ProductId.ToString()
                    );
                }
            }

            await transaction.CommitAsync();

            return new ApiResponse<CreateOrderResponseDto>
            {
                Success = true,
                Message = "Đặt hàng thành công!",
                Data = new CreateOrderResponseDto
                {
                    OrderId = order.OrderId,
                    OrderStatus = order.OrderStatus,
                    PaymentStatus = order.PaymentStatus,
                    FinalAmount = order.FinalAmount,
                    CreatedAt = order.CreatedAt,
                    QrCodeUrl = qrCodeUrl
                }
            };
        }
        catch (Exception ex)
        {
            try { await transaction.RollbackAsync(); } catch { }
            return new ApiResponse<CreateOrderResponseDto>
            {
                Success = false,
                Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
            };
        }
        finally
        {
            try { transaction.Dispose(); } catch { }
        }
    }

    public async Task<ApiResponse<OrderDto>> GetOrderDetailsAsync(long orderId)
    {
        var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsAsync(orderId);
        if (order == null)
        {
            return new ApiResponse<OrderDto> { Success = false, Message = "Không tìm thấy đơn hàng" };
        }

        var detailDtos = order.OrderDetails.Select(d => new OrderDetailDto
        {
            OrderDetailId = d.OrderDetailId,
            ProductVariantId = d.ProductVariantId,
            Quantity = d.Quantity,
            PriceAtPurchase = d.PriceAtPurchase
        }).ToList();

        var orderDto = new OrderDto
        {
            OrderId = order.OrderId,
            OrderStatus = order.OrderStatus,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            ProvisionalAmount = order.ProvisionalAmount,
            ShippingFee = order.ShippingFee,
            FinalAmount = order.FinalAmount,
            ShippingAddress = order.ShippingAddress,
            ShippingPhone = order.ShippingPhone,
            Details = detailDtos
        };

        return new ApiResponse<OrderDto>
        {
            Success = true,
            Data = orderDto
        };
    }

    public async Task<ApiResponse<string>> ProcessWebhookPaymentAsync(string orderCode, decimal transferAmount, string referenceCode)
    {
        string orderIdStr = orderCode.Substring(2); // Cắt chữ DH đi để lấy số
        if (!long.TryParse(orderIdStr, out long orderId))
        {
            return new ApiResponse<string> { Success = false, Message = "Invalid order ID format" };
        }

        var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return new ApiResponse<string> { Success = false, Message = "Order not found" };
        }

        if (order.PaymentStatus == "Success")
        {
            return new ApiResponse<string> { Success = false, Message = "Order already paid" };
        }

        if (transferAmount < order.FinalAmount)
        {
            return new ApiResponse<string> { Success = false, Message = "Transfer amount is less than order amount" };
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
            payment.TransactionReference = referenceCode;
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

        return new ApiResponse<string> { Success = true, Message = "Thanh toán thành công", Data = orderId.ToString() };
    }
}
