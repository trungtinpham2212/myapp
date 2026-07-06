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

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

            var order = new Order
            {
                UserId = userId,
                OrderStatus = "Chờ xử lý",
                PaymentMethod = request.PaymentMethod ?? "COD",
                PaymentStatus = "Chưa thanh toán",
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
                    CreatedAt = order.CreatedAt
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
