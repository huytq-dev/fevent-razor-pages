using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public static class EventData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var createdAt = new DateTimeOffset(2026, 2, 1, 9, 0, 0, TimeSpan.FromHours(7));

        var categoryNames = new[] { "Workshop", "Club", "Academic" };
        var categoryMap = await context.Categories
            .Where(c => categoryNames.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        foreach (var name in categoryNames)
        {
            if (categoryMap.ContainsKey(name))
            {
                continue;
            }

            var category = new Category
            {
                Id = GuidHelper.From($"Category.{name}"),
                Name = name,
                Description = $"{name} events",
                CreatedAt = createdAt
            };

            await context.Categories.AddAsync(category);
            categoryMap[name] = category.Id;
        }

        var locations = new[]
        {
            new
            {
                Name = "Alpha Hall, FPT Hanoi",
                Address = "FPT University, Hoa Lac, Hanoi"
            },
            new
            {
                Name = "Student Center, FPT Hanoi",
                Address = "FPT University, Hoa Lac, Hanoi"
            },
            new
            {
                Name = "Main Auditorium, FPT HCM",
                Address = "FPT University, HCM Campus"
            }
        };

        var locationMap = await context.Locations
            .Where(l => locations.Select(x => x.Name).Contains(l.Name))
            .ToDictionaryAsync(l => l.Name, l => l.Id);

        foreach (var location in locations)
        {
            if (locationMap.ContainsKey(location.Name))
            {
                continue;
            }

            var entity = new Location
            {
                Id = GuidHelper.From($"Location.{location.Name}"),
                Name = location.Name,
                Address = location.Address,
                Capacity = 300,
                CreatedAt = createdAt
            };

            await context.Locations.AddAsync(entity);
            locationMap[location.Name] = entity.Id;
        }

        const string organizerEmail = "events@fevent.local";
        var organizer = await context.Users.FirstOrDefaultAsync(u => u.Email == organizerEmail);
        var organizerId = organizer?.Id ?? GuidHelper.From("User.EventsOrganizer");

        if (organizer is null)
        {
            organizer = new User
            {
                Id = organizerId,
                FullName = "FEvent Organizer",
                Username = "fevent.organizer",
                Email = organizerEmail,
                IsVerified = true,
                RefreshToken = string.Empty,
                RefreshTokenExpiryTime = DateTime.UtcNow.AddYears(1),
                CreatedAt = createdAt
            };

            await context.Users.AddAsync(organizer);
        }

        await context.SaveChangesAsync();

        if (await context.Events.AnyAsync())
        {
            return;
        }

        var events = new[]
        {
            new Event
            {
                Id = GuidHelper.From("Event.AI-Revolution"),
                Title = "AI Revolution: LLMs & Future Tech",
                Description = "A deep dive into large language models, agentic workflows, and product demos.",
                ThumbnailUrl = "https://picsum.photos/seed/ai-tech/800/600",
                StartTime = new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 12, 11, 30, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 280,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Workshop"],
                LocationId = locationMap["Alpha Hall, FPT Hanoi"],
                OrganizerId = organizerId,
                CreatedAt = createdAt
            },
            new Event
            {
                Id = GuidHelper.From("Event.Guitar-Open-Mic"),
                Title = "Guitar Club: Open Mic Night",
                Description = "An acoustic evening for all levels. Bring your guitar and enjoy live performances.",
                ThumbnailUrl = "https://picsum.photos/seed/guitar/800/600",
                StartTime = new DateTimeOffset(2026, 10, 14, 18, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 14, 21, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 120,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Club"],
                LocationId = locationMap["Student Center, FPT Hanoi"],
                OrganizerId = organizerId,
                CreatedAt = createdAt
            },
            new Event
            {
                Id = GuidHelper.From("Event.Career-Fair-2024"),
                Title = "FPT Career Fair 2024",
                Description = "Meet recruiters, explore internships, and attend career growth talks.",
                ThumbnailUrl = "https://picsum.photos/seed/career/800/600",
                StartTime = new DateTimeOffset(2026, 11, 1, 8, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 11, 1, 16, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 400,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Academic"],
                LocationId = locationMap["Main Auditorium, FPT HCM"],
                OrganizerId = organizerId,
                CreatedAt = createdAt
            }
        };

        await context.Events.AddRangeAsync(events);
        
        var ticketTypes = new[]
        {
            new TicketType
            {
                Id = GuidHelper.From("TicketType.AI-Revolution.Standard"),
                EventId = GuidHelper.From("Event.AI-Revolution"),
                Name = "Standard",
                Price = 0,
                Quantity = 120,
                SoldCount = 0,
                CreatedAt = createdAt
            },
            new TicketType
            {
                Id = GuidHelper.From("TicketType.Guitar-Open-Mic.Standard"),
                EventId = GuidHelper.From("Event.Guitar-Open-Mic"),
                Name = "Standard",
                Price = 0,
                Quantity = 120,
                SoldCount = 0,
                CreatedAt = createdAt
            },
            new TicketType
            {
                Id = GuidHelper.From("TicketType.Career-Fair-2024.Standard"),
                EventId = GuidHelper.From("Event.Career-Fair-2024"),
                Name = "Standard",
                Price = 0,
                Quantity = 120,
                SoldCount = 0,
                CreatedAt = createdAt
            }
        };

        await context.TicketTypes.AddRangeAsync(ticketTypes);
        await context.SaveChangesAsync();
    }
}
