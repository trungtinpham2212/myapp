using Microsoft.EntityFrameworkCore.Storage;
using Repositories.DBContext;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    myappContext DbContext { get; }
    
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    IBrandRepository BrandRepository { get; }
    ICartRepository CartRepository { get; }
    ICartItemRepository CartItemRepository { get; }
    ICategoryRepository CategoryRepository { get; }
    IOrderRepository OrderRepository { get; }
    IOrderDetailRepository OrderDetailRepository { get; }
    IProductRepository ProductRepository { get; }
    IProductImageRepository ProductImageRepository { get; }
    IProductVariantRepository ProductVariantRepository { get; }
    IReviewRepository ReviewRepository { get; }
    IUserRepository UserRepository { get; }
    IWishlistRepository WishlistRepository { get; }
    IPaymentRepository PaymentRepository { get; }

    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<int> SaveChangesAsync();
}
