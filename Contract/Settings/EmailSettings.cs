namespace Contract;

public interface EmailSettings
{
    public string SenderEmail { get; set; }
    public string SenderName { get; set; }
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string AppPassword { get; set; }
}
