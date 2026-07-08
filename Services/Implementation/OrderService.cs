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
        using var transaction = await _unitOfWork.BeginTransactionAsync();
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
            await transaction.CommitAsync();

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
                    var product = await _unitOfWork.ProductRepository.GetByIdAsync(variant.ProductId);
                    string productName = product != null ? product.Name : $"Variant {variant.ProductVariantId}";
                    
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
            await transaction.RollbackAsync();
            return new ApiResponse<CreateOrderResponseDto>
            {
                Success = false,
                Message = $"Lỗi khi tạo đơn hàng: {ex.Message}"
            };
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
}
