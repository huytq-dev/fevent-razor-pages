using System.Linq.Expressions;

namespace Infrastructure;

public class GenericRepository<T> : IGenericRepository<T> where T : EntityBase<Guid>
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _set;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _set.AsNoTracking().ToListAsync();

    public IQueryable<T> GetQueryable()
        => _set.AsQueryable();

    public async Task<T?> GetByIdAsync(Guid id)
        => await _set.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
           // ?? throw new NotFoundException(ExceptionMessages.NotFound(typeof(T).Name, id));

    public async Task<T> AddAsync(T entity)
    {
        await _set.AddAsync(entity);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
        => await _set.AddRangeAsync(entities);

    public void Update(T entity)
        => _set.Update(entity);

    public async Task RemoveAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            _set.Remove(entity);
        }
    }
    public void RemoveRange(IEnumerable<T> entities)
        => _set.RemoveRange(entities);

    public async Task<bool> IsExistAsync(Guid id)
        => await _set.AsNoTracking().AnyAsync(e => e.Id == id);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await _set.AsNoTracking().AnyAsync(predicate);
}