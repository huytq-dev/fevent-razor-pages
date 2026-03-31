namespace Application;

[RegisterService(typeof(ICatalogService))]
public class CatalogService(IUnitOfWork unitOfWork) : ICatalogService
{
    public async Task<IReadOnlyList<CategoryFilterItem>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var items = await unitOfWork.Categories.GetAllAsync();
        return items
            .OrderBy(c => c.Name)
            .Select(c => new CategoryFilterItem(c.Id, c.Name))
            .ToList();
    }

    public async Task<IReadOnlyList<LocationFilterItem>> GetLocationsAsync(CancellationToken ct = default)
    {
        var items = await unitOfWork.Locations.GetAllAsync();
        return items
            .OrderBy(l => l.Name)
            .Select(l => new LocationFilterItem(l.Id, l.Name))
            .ToList();
    }

    public async Task<IReadOnlyList<MajorFilterItem>> GetMajorsAsync(CancellationToken ct = default)
    {
        var items = await unitOfWork.Majors.GetAllAsync();
        return items
            .OrderBy(m => m.Name)
            .Select(m => new MajorFilterItem(m.Id, m.Name, m.Code))
            .ToList();
    }
}
