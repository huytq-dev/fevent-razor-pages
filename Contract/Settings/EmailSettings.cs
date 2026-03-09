namespace Contract;

public class EmailSettings
{
    public required string SenderEmail { get; set; }
    public required string SenderName { get; set; }
    public required string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public required string AppPassword { get; set; }
}
