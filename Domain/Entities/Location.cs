using Domain;

public class Location : EntityBase<Guid>
{
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? ImageUrl { get; set; } // Ảnh sơ đồ phòng/tòa nhà
    public string? MapUrl { get; set; }   // Link Google Map
    public int Capacity { get; set; }

    // Nếu muốn làm map chuyên sâu
    // public double? Latitude { get; set; }
    // public double? Longitude { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}