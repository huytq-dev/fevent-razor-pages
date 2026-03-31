namespace Infrastructure;

public static class EventData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var createdAt = new DateTimeOffset(2026, 2, 1, 9, 0, 0, TimeSpan.FromHours(7));

        // Seed Majors
        var majorData = new[]
        {
            ("SE",  "Software Engineering"),
            ("AI",  "Artificial Intelligence"),
            ("IA",  "Information Assurance"),
            ("GD",  "Graphic Design"),
            ("MKT", "Digital Marketing"),
            ("BA",  "Business Administration"),
            ("FIN", "Finance"),
            ("EN",  "English Language"),
            ("JA",  "Japanese Language"),
            ("KO",  "Korean Language"),
            ("CN",  "Chinese Language"),
            ("MDA", "Multimedia & Communication"),
        };

        var existingCodes = await context.Majors.Select(m => m.Code).ToListAsync();
        foreach (var (code, name) in majorData)
        {
            if (!existingCodes.Contains(code))
            {
                await context.Majors.AddAsync(new Major
                {
                    Id = GuidHelper.From($"Major.{code}"),
                    Code = code,
                    Name = name,
                    CreatedAt = createdAt
                });
            }
        }
        await context.SaveChangesAsync();

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
            new { Name = "Alpha Hall, FPT Hanoi",        Address = "FPT University, Hoa Lac, Hanoi" },
            new { Name = "Student Center, FPT Hanoi",    Address = "FPT University, Hoa Lac, Hanoi" },
            new { Name = "Main Auditorium, FPT HCM",     Address = "FPT University, Linh Trung, Thu Duc, HCM" },
            new { Name = "Main Hall, FPT Can Tho",       Address = "FPT University, Can Tho Campus" },
            new { Name = "Main Hall, FPT Da Nang",       Address = "FPT University, Da Nang Campus" },
            new { Name = "Main Hall, FPT Quy Nhon",      Address = "FPT University, Quy Nhon Campus" },
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

        var majorMap = await context.Majors.ToDictionaryAsync(m => m.Code!, m => m.Id);

        if (await context.Events.AnyAsync())
        {
            return;
        }

        var events = new[]
        {
            // SE
            new Event
            {
                Id = GuidHelper.From("Event.SE-Hackathon-2026"),
                Title = "FPT Hackathon 2026",
                Description = "48-hour coding marathon. Build a real-world solution with your team and pitch to a panel of industry judges.",
                ThumbnailUrl = "https://picsum.photos/seed/hackathon/800/600",
                StartTime = new DateTimeOffset(2026, 10, 3, 8, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 5, 8, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 200,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Workshop"],
                LocationId = locationMap["Alpha Hall, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("SE"),
                CreatedAt = createdAt
            },
            // AI
            new Event
            {
                Id = GuidHelper.From("Event.AI-Revolution"),
                Title = "AI Revolution: LLMs & Future Tech",
                Description = "A deep dive into large language models, agentic workflows, and product demos from leading AI researchers.",
                ThumbnailUrl = "https://picsum.photos/seed/ai-tech/800/600",
                StartTime = new DateTimeOffset(2026, 10, 12, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 12, 11, 30, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 280,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Workshop"],
                LocationId = locationMap["Alpha Hall, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("AI"),
                CreatedAt = createdAt
            },
            // IA
            new Event
            {
                Id = GuidHelper.From("Event.CyberSec-CTF"),
                Title = "Capture The Flag: CyberSec Challenge",
                Description = "Compete in a jeopardy-style CTF covering web exploitation, reverse engineering, forensics, and cryptography.",
                ThumbnailUrl = "https://picsum.photos/seed/ctf-security/800/600",
                StartTime = new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 18, 17, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 150,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Academic"],
                LocationId = locationMap["Alpha Hall, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("IA"),
                CreatedAt = createdAt
            },
            // GD
            new Event
            {
                Id = GuidHelper.From("Event.UIUX-Design-Workshop"),
                Title = "UI/UX Design Sprint Workshop",
                Description = "Learn the end-to-end design process — from user research and wireframing to prototyping in Figma.",
                ThumbnailUrl = "https://picsum.photos/seed/ux-design/800/600",
                StartTime = new DateTimeOffset(2026, 10, 24, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 24, 16, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 80,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Workshop"],
                LocationId = locationMap["Student Center, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("GD"),
                CreatedAt = createdAt
            },
            // MKT
            new Event
            {
                Id = GuidHelper.From("Event.Digital-Marketing-Bootcamp"),
                Title = "Digital Marketing Bootcamp",
                Description = "Hands-on sessions on SEO, content strategy, paid ads, and analytics. Featuring case studies from real campaigns.",
                ThumbnailUrl = "https://picsum.photos/seed/marketing/800/600",
                StartTime = new DateTimeOffset(2026, 11, 7, 8, 30, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 11, 7, 17, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 120,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Workshop"],
                LocationId = locationMap["Student Center, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("MKT"),
                CreatedAt = createdAt
            },
            // BA
            new Event
            {
                Id = GuidHelper.From("Event.Business-Case-Competition"),
                Title = "FPT Business Case Competition 2026",
                Description = "Teams of 4 analyze a real business problem and present solutions to a jury of executives and entrepreneurs.",
                ThumbnailUrl = "https://picsum.photos/seed/business-case/800/600",
                StartTime = new DateTimeOffset(2026, 11, 14, 8, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 11, 14, 17, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 100,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Academic"],
                LocationId = locationMap["Main Auditorium, FPT HCM"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("BA"),
                CreatedAt = createdAt
            },
            // FIN
            new Event
            {
                Id = GuidHelper.From("Event.Personal-Finance-Talk"),
                Title = "Personal Finance & Stock Market Talk",
                Description = "Expert speakers cover budgeting, investing in stocks and ETFs, and building wealth as a student.",
                ThumbnailUrl = "https://picsum.photos/seed/finance/800/600",
                StartTime = new DateTimeOffset(2026, 11, 21, 14, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 11, 21, 17, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 200,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Academic"],
                LocationId = locationMap["Main Auditorium, FPT HCM"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("FIN"),
                CreatedAt = createdAt
            },
            // EN
            new Event
            {
                Id = GuidHelper.From("Event.English-Debate-Competition"),
                Title = "FPT English Debate Championship",
                Description = "British Parliamentary format debate open to all students. Sharpen your critical thinking and public speaking skills.",
                ThumbnailUrl = "https://picsum.photos/seed/debate/800/600",
                StartTime = new DateTimeOffset(2026, 10, 31, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 10, 31, 17, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 60,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Academic"],
                LocationId = locationMap["Student Center, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("EN"),
                CreatedAt = createdAt
            },
            // JA
            new Event
            {
                Id = GuidHelper.From("Event.Japan-Culture-Day"),
                Title = "Japan Culture Day 2026",
                Description = "Celebrate Japanese culture with tea ceremony, calligraphy, origami, cosplay, and traditional food stalls.",
                ThumbnailUrl = "https://picsum.photos/seed/japan-culture/800/600",
                StartTime = new DateTimeOffset(2026, 11, 28, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 11, 28, 18, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 300,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Club"],
                LocationId = locationMap["Main Auditorium, FPT HCM"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("JA"),
                CreatedAt = createdAt
            },
            // KO
            new Event
            {
                Id = GuidHelper.From("Event.Korean-Wave-Festival"),
                Title = "Korean Wave Festival",
                Description = "K-pop cover dance competition, Korean street food, language exchange, and cultural showcase.",
                ThumbnailUrl = "https://picsum.photos/seed/kpop/800/600",
                StartTime = new DateTimeOffset(2026, 12, 5, 10, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 12, 5, 20, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 350,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Club"],
                LocationId = locationMap["Student Center, FPT Hanoi"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("KO"),
                CreatedAt = createdAt
            },
            // MDA
            new Event
            {
                Id = GuidHelper.From("Event.Short-Film-Festival"),
                Title = "FPT Short Film & Media Festival",
                Description = "Student-produced short films, photography exhibitions, and a panel discussion on storytelling in digital media.",
                ThumbnailUrl = "https://picsum.photos/seed/filmfest/800/600",
                StartTime = new DateTimeOffset(2026, 12, 12, 9, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 12, 12, 21, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 250,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Club"],
                LocationId = locationMap["Main Auditorium, FPT HCM"],
                OrganizerId = organizerId,
                MajorId = majorMap.GetValueOrDefault("MDA"),
                CreatedAt = createdAt
            },
            // No major (open to all)
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
                MajorId = null,
                CreatedAt = createdAt
            },
            new Event
            {
                Id = GuidHelper.From("Event.Career-Fair-2026"),
                Title = "FPT Career Fair 2026",
                Description = "Meet recruiters from 50+ companies, explore internships, and attend career growth talks.",
                ThumbnailUrl = "https://picsum.photos/seed/career/800/600",
                StartTime = new DateTimeOffset(2026, 11, 1, 8, 0, 0, TimeSpan.FromHours(7)),
                EndTime = new DateTimeOffset(2026, 11, 1, 16, 0, 0, TimeSpan.FromHours(7)),
                MaxParticipants = 400,
                Status = EventStatus.Approved,
                IsPrivate = false,
                CategoryId = categoryMap["Academic"],
                LocationId = locationMap["Main Auditorium, FPT HCM"],
                OrganizerId = organizerId,
                MajorId = null,
                CreatedAt = createdAt
            }
        };

        await context.Events.AddRangeAsync(events);

        var ticketTypes = events.Select(e => new TicketType
        {
            Id = GuidHelper.From($"TicketType.{e.Id}.Standard"),
            EventId = e.Id,
            Name = "Standard",
            Price = 0,
            Quantity = e.MaxParticipants,
            SoldCount = 0,
            CreatedAt = createdAt
        }).ToArray();

        await context.TicketTypes.AddRangeAsync(ticketTypes);
        await context.SaveChangesAsync();
    }
}
