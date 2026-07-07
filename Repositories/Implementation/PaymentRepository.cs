using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(myappContext dbContext) : base(dbContext)
    {
    }
}
