namespace Infrastructure;

[RegisterService(typeof(ILocationsRepository))]
public class LocationsRepository : GenericRepository<Location>, ILocationsRepository
{
    public LocationsRepository(ApplicationDbContext context) : base(context)
    {
    }
}
