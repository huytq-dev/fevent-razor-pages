namespace Infrastructure;

[RegisterService(typeof(ICategoriesRepository))]
public class CategoriesRepository : GenericRepository<Category>, ICategoriesRepository
{
    public CategoriesRepository(ApplicationDbContext context) : base(context)
    {
    }
}
