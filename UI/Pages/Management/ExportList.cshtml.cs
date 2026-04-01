namespace UI;

public class ExportListModel : PageModel
{
    private readonly IEventsService _eventsService;
    private readonly IEventRegistrationsService _registrationsService;

    public ExportListModel(IEventsService eventsService, IEventRegistrationsService registrationsService)
    {
        _eventsService = eventsService;
        _registrationsService = registrationsService;
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

    public async Task<IActionResult> OnPostDownloadAsync(CancellationToken ct)
    {
        if (ViewModel.EventId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Event ID is required for export.";
            return RedirectToPage(new { eventId = ViewModel.EventId });
        }

        var userIdStr = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Authentications/Login");
        }

        var eventResult = await _eventsService.GetDetailAsync(ViewModel.EventId, ct);
        if (!eventResult.IsSuccess || eventResult.Data is null)
        {
            TempData["ErrorMessage"] = "Event not found.";
            return RedirectToPage(new { eventId = ViewModel.EventId });
        }

        if (eventResult.Data.OrganizerId != userId)
        {
            return Forbid();
        }

        var participants = await _registrationsService.GetByEventAsync(ViewModel.EventId, ct);
        var selectedFields = (ViewModel.SelectedFields ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (selectedFields.Count == 0)
        {
            selectedFields = ["FullName", "StudentId", "Email", "Status"];
        }

        var columns = selectedFields
            .Select(GetColumnDefinition)
            .Where(c => c is not null)
            .Select(c => c!.Value)
            .ToList();

        if (columns.Count == 0)
        {
            TempData["ErrorMessage"] = "Please select at least one valid field to export.";
            return RedirectToPage(new { eventId = ViewModel.EventId });
        }

        IEnumerable<ParticipantSummaryResponse> filtered = participants;
        if (string.Equals(ViewModel.StatusFilter, "Attended Only", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p => (Domain.RegistrationStatus)p.Status == Domain.RegistrationStatus.CheckedIn);
        }
        else if (string.Equals(ViewModel.StatusFilter, "Registered but Absent", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p => (Domain.RegistrationStatus)p.Status == Domain.RegistrationStatus.Confirmed);
        }
        else if (string.Equals(ViewModel.StatusFilter, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(p => (Domain.RegistrationStatus)p.Status == Domain.RegistrationStatus.Cancelled);
        }

        filtered = (ViewModel.SortBy ?? string.Empty) switch
        {
            "Student ID" => filtered.OrderBy(p => p.StudentId ?? string.Empty, StringComparer.OrdinalIgnoreCase),
            "Check-in Time (Latest)" => filtered.OrderByDescending(p => p.CheckInTime),
            _ => filtered.OrderBy(p => p.FullName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
        };

        var list = filtered.ToList();
        var safeEventTitle = string.Join("-", eventResult.Data.Title.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        if (string.IsNullOrWhiteSpace(safeEventTitle))
        {
            safeEventTitle = ViewModel.EventId.ToString("N");
        }

        if (string.Equals(ViewModel.SelectedFormat, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var lines = new List<string>
            {
                string.Join(",", columns.Select(c => QuoteCsv(c.Header)))
            };

            lines.AddRange(list.Select(p => string.Join(",", columns.Select(c => QuoteCsv(c.Selector(p))))));

            var csv = string.Join(Environment.NewLine, lines);
            var fileName = $"participants-{safeEventTitle}-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        if (!string.Equals(ViewModel.SelectedFormat, "excel", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Only Excel and CSV export are supported right now.";
            return RedirectToPage(new { eventId = ViewModel.EventId });
        }

        var rows = list.Select(p => columns.Select(c => c.Selector(p)));
        var workbook = ExcelExportHelper.BuildWorkbook(columns.Select(c => c.Header), rows, "Participants");
        var excelFileName = $"participants-{safeEventTitle}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
        return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
    }

    private static (string Header, Func<ParticipantSummaryResponse, string?> Selector)? GetColumnDefinition(string field)
    {
        return field.Trim() switch
        {
            "FullName" => ("Full Name", p => p.FullName),
            "StudentId" => ("Student ID", p => p.StudentId),
            "Email" => ("Email", p => p.Email),
            "Phone" => ("Phone", p => p.PhoneNumber),
            "Status" => ("Status", p => GetStatusLabel(p.Status)),
            "Time" => ("Check-in Time", p => p.CheckInTime?.LocalDateTime.ToString("yyyy-MM-dd HH:mm")),
            _ => null
        };
    }

    private static string GetStatusLabel(int status) => (Domain.RegistrationStatus)status switch
    {
        Domain.RegistrationStatus.Confirmed => "Registered",
        Domain.RegistrationStatus.CheckedIn => "Checked-in",
        Domain.RegistrationStatus.Cancelled => "Cancelled",
        Domain.RegistrationStatus.PendingPayment => "Pending Payment",
        Domain.RegistrationStatus.Paid => "Paid",
        _ => "Unknown"
    };

    private static string QuoteCsv(string? value)
    {
        var text = value ?? string.Empty;
        return $"\"{text.Replace("\"", "\"\"")}\"";
    }
}
