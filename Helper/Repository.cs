using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WalletApp.Model;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DBClass _dbContext;

    public Repository(DBClass dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> SaveAsync(T entity)
    {
        if (_dbContext.Entry(entity).State == EntityState.Detached)
        {
            _dbContext.Set<T>().Add(entity);
        }
        else
        {
            _dbContext.Set<T>().Update(entity);
        }
        try {
            await _dbContext.SaveChangesAsync();
        } catch (Exception ex) {
            var message = ex.Message;
        }
       
        return entity;
    }

    public async Task<bool> DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        return (await _dbContext.SaveChangesAsync() > 0);
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _dbContext.Set<T>().FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbContext.Set<T>().ToListAsync();
    }


    public async Task<bool> ExecuteSQL(string sql)
    {
        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
            return true;
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            return false;
        }
    }
}
