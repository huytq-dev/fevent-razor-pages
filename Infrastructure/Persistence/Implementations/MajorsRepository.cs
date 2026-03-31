namespace Infrastructure;

[RegisterService(typeof(IMajorsRepository))]
public class MajorsRepository : GenericRepository<Major>, IMajorsRepository
{
    public MajorsRepository(ApplicationDbContext context) : base(context)
    {
    }
}
