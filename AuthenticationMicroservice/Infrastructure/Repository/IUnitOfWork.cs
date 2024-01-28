using Microsoft.EntityFrameworkCore;

namespace AuthenticationMicroservice.Infrastructure.Repository;

public interface IUnitOfWork
{
    DbContext GetContext { get; }

    IGenericRepository<T> Repository<T>() where T : class;
    void SaveChanges();
    Task<int> SaveChangesAsync();
}
