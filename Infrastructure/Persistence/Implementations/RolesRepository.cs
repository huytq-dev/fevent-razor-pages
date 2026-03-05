namespace Infrastructure;

[RegisterService(typeof(IRolesRepository))]
public class RolesRepository : GenericRepository<Role>, IRolesRepository
{
    public RolesRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken ct = default)
        => await _set.AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleName == roleName);
            // ?? throw new NotFoundException(ExceptionMessages.NotFoundField("Role", "Role Name", roleName));
}
