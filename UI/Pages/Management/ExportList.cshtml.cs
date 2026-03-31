using Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UI.Models.Events;

namespace UI.Pages.Management;

public class ExportListModel : PageModel
{
    private readonly IEventsService _eventsService;

    public ExportListModel(IEventsService eventsService)
    {
        _eventsService = eventsService;
    }

    [BindProperty]
    public ExportListViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? eventId)
    {
        if (eventId == null || eventId == Guid.Empty)
        {
            // Seed mock data for UI testing if no actual ID is provided
            ViewModel = new ExportListViewModel
            {
                EventId = Guid.Empty,
                EventTitle = "Orientation Day 2023",
                EventDate = new DateTime(2023, 10, 12),
                TotalParticipants = 1245,
                RecentExports = new List<ExportHistoryViewModel>
                {
                    new ExportHistoryViewModel 
                    { 
                        ExportTime = new DateTime(2023, 10, 12, 14, 30, 0), 
                        RequestedBy = "You", 
                        RequestedByAvatar = "https://lh3.googleusercontent.com/aida-public/AB6AXuBaW9zHH9ZdHLBwiTaWo4t4F8X6oa_zIbty2BROTg4FQDTz1Q5ejBdCoij3TIxenBoTEwsEYsJEnrnqKWHp5EGv5l8aZWGGVqL5QLHl-B8avn20piGHkBiohJz1XSPcWhQiIOU0aY4wdWwbCJInWXk352RSV1-hOs_6VhIKiUQL-cpPVgmFGQwYvbSo4CbJLhGTV0OH4iZeB-QX0uEqHw_mKFY8FxmW9jFxm8RwNK0KY8lvZ1Da0NtLK8oMUe9T9DThPzwh27nR9jXI",
                        Format = "Excel", 
                        Status = "Completed" 
                    },
                    new ExportHistoryViewModel 
                    { 
                        ExportTime = new DateTime(2023, 10, 10, 9, 15, 0), 
                        RequestedBy = "Sarah J.", 
                        RequestedByAvatar = "https://lh3.googleusercontent.com/aida-public/AB6AXuDWqvMZXc2E4ED6g61FSNeIy9ylAPZPSi6g7GZdZw2j9Ke_g9LUda6XcZKtqLyX4vKBNaqbcfbF3ckCMVgNfSbTVFXfrgXzH7tIoMaTUzEQblbwD8CLbna7U4COHvFhmrxaZ3fXmNYl4UWfNMoPKc9H4DSgKMBjGQyHANt7cF1IKLN37bvKJ2FoElTuZhIcF11D9yFMaEWBlbxvLEJ2VzGDKZPRWneh4Z5P5EKgKxF-oBNt5vgispJ5KWQFvEuHuuMCqKFzrLM357n5",
                        Format = "PDF", 
                        Status = "Completed" 
                    }
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
        ViewModel.EventDate = response.Data.StartTime.LocalDateTime;
        ViewModel.TotalParticipants = response.Data.RegisteredCount;

        // In a real application, you would call a service to get the recent exports.
        ViewModel.RecentExports = new List<ExportHistoryViewModel>();

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadAsync()
    {
        // Placeholder for file generation logic
        // Depending on ViewModel.SelectedFormat and ViewModel.SelectedFields
        
        TempData["SuccessMessage"] = "Export started. Your file will be ready shortly.";
        return Page();
    }
}
