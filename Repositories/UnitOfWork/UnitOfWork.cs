using Microsoft.EntityFrameworkCore.Storage;
using Repositories.DBContext;
using Repositories.Implementation;
using Repositories.Interface;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Repositories.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly myappContext _dbContext;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    private IBrandRepository _brandRepository;
    private ICartRepository _cartRepository;
    private ICartItemRepository _cartItemRepository;
    private ICategoryRepository _categoryRepository;
    private IOrderRepository _orderRepository;
    private IOrderDetailRepository _orderDetailRepository;
    private IProductRepository _productRepository;
    private IProductImageRepository _productImageRepository;
    private IProductVariantRepository _productVariantRepository;
    private IReviewRepository _reviewRepository;
    private IUserRepository _userRepository;
    private IWishlistRepository _wishlistRepository;
    private IPaymentRepository _paymentRepository;

    public UnitOfWork(myappContext dbContext)
    {
        _dbContext = dbContext;
    }

    public myappContext DbContext => _dbContext;

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var type = typeof(TEntity);
        if (_repositories.ContainsKey(type))
        {
            return (IGenericRepository<TEntity>)_repositories[type];
        }

        var repository = new GenericRepository<TEntity>(_dbContext);
        _repositories[type] = repository;
        return repository;
    }

    public IBrandRepository BrandRepository => _brandRepository ??= new BrandRepository(_dbContext);
    public ICartRepository CartRepository => _cartRepository ??= new CartRepository(_dbContext);
    public ICartItemRepository CartItemRepository => _cartItemRepository ??= new CartItemRepository(_dbContext);
    public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_dbContext);
    public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(_dbContext);
    public IOrderDetailRepository OrderDetailRepository => _orderDetailRepository ??= new OrderDetailRepository(_dbContext);
    public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_dbContext);
    public IProductImageRepository ProductImageRepository => _productImageRepository ??= new ProductImageRepository(_dbContext);
    public IProductVariantRepository ProductVariantRepository => _productVariantRepository ??= new ProductVariantRepository(_dbContext);
    public IReviewRepository ReviewRepository => _reviewRepository ??= new ReviewRepository(_dbContext);
    public IUserRepository UserRepository => _userRepository ??= new UserRepository(_dbContext);
    public IWishlistRepository WishlistRepository => _wishlistRepository ??= new WishlistRepository(_dbContext);
    public IPaymentRepository PaymentRepository => _paymentRepository ??= new PaymentRepository(_dbContext);

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _dbContext.Database.BeginTransactionAsync();
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
