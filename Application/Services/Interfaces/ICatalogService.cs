namespace Application;

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryFilterItem>> GetCategoriesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LocationFilterItem>> GetLocationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MajorFilterItem>> GetMajorsAsync(CancellationToken ct = default);
}
