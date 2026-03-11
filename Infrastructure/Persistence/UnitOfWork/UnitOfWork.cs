using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure;

[RegisterService(typeof(IUnitOfWork))]
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    private readonly Dictionary<Type, Func<ApplicationDbContext, object>> _repositoryFactories;
    private IDbContextTransaction? _transaction;


    #region System Repositories

    public IUsersRepository Users => (IUsersRepository)Repository<User>();
    public IRolesRepository Roles => (IRolesRepository)Repository<Role>();
    public IEventsRepository Events => (IEventsRepository)Repository<Event>();
    public IEventRegistrationsRepository EventRegistrations => (IEventRegistrationsRepository)Repository<EventRegistration>();
    public IEventReviewsRepository EventReviews => (IEventReviewsRepository)Repository<EventReview>();
    public ICategoriesRepository Categories => (ICategoriesRepository)Repository<Category>();
    public ILocationsRepository Locations => (ILocationsRepository)Repository<Location>();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
        _repositoryFactories = new Dictionary<Type, Func<ApplicationDbContext, object>>
        {
            { typeof(User), ctx => new UsersRepository(ctx) },
            { typeof(Role), ctx => new RolesRepository(ctx) },
            { typeof(Event), ctx => new EventsRepository(ctx) },
            { typeof(EventRegistration), ctx => new EventRegistrationsRepository(ctx) },
            { typeof(EventReview), ctx => new EventReviewsRepository(ctx) },
            { typeof(Category), ctx => new CategoriesRepository(ctx) },
            { typeof(Location), ctx => new LocationsRepository(ctx) },
        };
    }

    public IGenericRepository<T> Repository<T>() where T : EntityBase<Guid>
    {
        var type = typeof(T);

        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = _repositoryFactories.TryGetValue(type, out var factory)
                ? factory(_context)
                : new GenericRepository<T>(_context);
        }

        return (IGenericRepository<T>)_repositories[type];
    }

#endregion

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _context.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
