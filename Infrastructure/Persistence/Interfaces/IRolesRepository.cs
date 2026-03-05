namespace Infrastructure;

public interface IRolesRepository : IGenericRepository<Role>
{
    Task<Role?> GetByNameAsync(string roleName, CancellationToken ct = default);
}
