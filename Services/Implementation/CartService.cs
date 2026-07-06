using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<CartDto>> GetCartAsync(Guid userId)
    {
        var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                TotalItems = 0,
                ProvisionalAmount = 0,
                ShippingFee = 0,
                FinalAmount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        var cartItems = await _unitOfWork.CartItemRepository.GetCartItemsWithDetailsAsync(cart.CartId);

        var itemDtos = new List<CartItemDto>();
        int totalItems = 0;
        decimal provisionalAmount = 0;

        foreach (var ci in cartItems)
        {
            int qty = ci.Quantity ?? 0;
            decimal price = ci.ProductVariant?.SalePrice ?? 0;
            string? prodName = ci.ProductVariant?.Product?.Name ?? "Sản phẩm";

            totalItems += qty;
            provisionalAmount += qty * price;

            itemDtos.Add(new CartItemDto
            {
                CartItemId = ci.CartItemId,
                ProductVariantId = ci.ProductVariantId,
                ProductName = prodName,
                Color = ci.ProductVariant?.Color,
                Storage = ci.ProductVariant?.Storage,
                SalePrice = price,
                Quantity = qty,
                AddedAt = ci.AddedAt
            });
        }

        decimal shippingFee = cart.ShippingFee ?? 0;
        decimal finalAmount = provisionalAmount + shippingFee;

        cart.TotalItems = totalItems;
        cart.ProvisionalAmount = provisionalAmount;
        cart.FinalAmount = finalAmount;
        cart.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CartRepository.Update(cart);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<CartDto>
        {
            Success = true,
            Data = new CartDto
            {
                CartId = cart.CartId,
                TotalItems = totalItems,
                ProvisionalAmount = provisionalAmount,
                ShippingFee = shippingFee,
                FinalAmount = finalAmount,
                Items = itemDtos
            }
        };
    }

    public async Task<ApiResponse<CartDto>> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        if (request.Quantity <= 0)
        {
            return new ApiResponse<CartDto> { Success = false, Message = "Số lượng phải lớn hơn 0" };
        }

        var variant = await _unitOfWork.ProductVariantRepository.GetByIdAsync(request.ProductVariantId);
        if (variant == null)
        {
            return new ApiResponse<CartDto> { Success = false, Message = "Biến thể sản phẩm không tồn tại" };
        }

        if ((variant.StockQuantity ?? 0) < request.Quantity)
        {
            return new ApiResponse<CartDto> { Success = false, Message = "Số lượng trong kho không đủ" };
        }

        var cart = await _unitOfWork.CartRepository.GetCartByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                TotalItems = 0,
                ProvisionalAmount = 0,
                ShippingFee = 0,
                FinalAmount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        var existingItem = await _unitOfWork.CartItemRepository.GetCartItemByCartAndVariantAsync(cart.CartId, request.ProductVariantId);

        if (existingItem != null)
        {
            existingItem.Quantity = (existingItem.Quantity ?? 0) + request.Quantity;
            _unitOfWork.CartItemRepository.Update(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.CartId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                AddedAt = DateTime.UtcNow
            };
            await _unitOfWork.CartItemRepository.AddAsync(newItem);
        }

        await _unitOfWork.SaveChangesAsync();

        return await GetCartAsync(userId);
    }

    public async Task<ApiResponse<object>> RemoveFromCartAsync(Guid userId, long cartItemId)
    {
        var item = await _unitOfWork.CartItemRepository.GetByIdAsync(cartItemId);

        if (item == null)
        {
            return new ApiResponse<object> { Success = false, Message = "Sản phẩm không có trong giỏ hàng" };
        }

        _unitOfWork.CartItemRepository.Delete(item);
        await _unitOfWork.SaveChangesAsync();

        await GetCartAsync(userId);

        return new ApiResponse<object>
        {
            Success = true,
            Message = "Đã xóa sản phẩm khỏi giỏ hàng"
        };
    }
}
