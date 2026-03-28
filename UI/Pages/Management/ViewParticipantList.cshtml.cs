using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Models.Events;

namespace UI.Pages.Management;

public class ViewParticipantListModel : PageModel
{
    private readonly IEventsService _eventsService;

    public ViewParticipantListModel(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [BindProperty(SupportsGet = true)]
    public ParticipantListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? eventId)
    {
        if (eventId == null || eventId == Guid.Empty)
        {
            // Seed mock data for UI testing if no actual ID is provided
            ViewModel = new ParticipantListViewModel
            {
                EventId = Guid.Empty,
                EventTitle = "Tech Talk 2024",
                TotalRegistered = 124,
                Participants = new List<ParticipantViewModel>
                {
                    new ParticipantViewModel { Order = 1, FullName = "Nguyen Van An", Mssv = "SE150001", Major = "Software Engineering", RegistrationDate = new DateTime(2023, 10, 24, 10, 0, 0), Status = "Registered" },
                    new ParticipantViewModel { Order = 2, FullName = "Tran Thi Binh", Mssv = "SS160244", Major = "Digital Marketing", RegistrationDate = new DateTime(2023, 10, 25, 9, 15, 0), Status = "Checked-in" },
                    new ParticipantViewModel { Order = 3, FullName = "Le Van Cuong", Mssv = "SE170999", Major = "Artificial Intelligence", RegistrationDate = new DateTime(2023, 10, 26, 14, 30, 0), Status = "Registered" },
                    new ParticipantViewModel { Order = 4, FullName = "Pham Thi Dung", Mssv = "GD140555", Major = "Graphic Design", RegistrationDate = new DateTime(2023, 10, 26, 16, 0, 0), Status = "Cancelled" },
                    new ParticipantViewModel { Order = 5, FullName = "Hoang Tuan", Mssv = "SE162002", Major = "Software Engineering", RegistrationDate = new DateTime(2023, 10, 27, 8, 45, 0), Status = "Checked-in" }
                }
            };
            return Page();
        }

        var response = await _eventsService.GetDetailAsync(eventId.Value);
        if (response == null || response.Data == null)
        {
            return NotFound();
        }

        ViewModel.EventId = eventId.Value;
        ViewModel.EventTitle = response.Data.Title;
        ViewModel.TotalRegistered = response.Data.RegisteredCount;

        // In a real application, you would call a service to get the participants.
        // For now, we'll continue using mock data for participants tied to this real EventTitle.
        ViewModel.Participants = new List<ParticipantViewModel>
        {
            new ParticipantViewModel { Order = 1, FullName = "Nguyen Van An", Mssv = "SE150001", Major = "Software Engineering", RegistrationDate = DateTime.Now.AddDays(-2), Status = "Registered" },
            new ParticipantViewModel { Order = 2, FullName = "Tran Thi Binh", Mssv = "SS160244", Major = "Digital Marketing", RegistrationDate = DateTime.Now.AddDays(-1), Status = "Checked-in" }
        };

        return Page();
    }
}
