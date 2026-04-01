namespace Contract;

public sealed class SignUpRequest
{
    public required string Fullname { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string StudentId { get; set; }
    public required string Major { get; set; }
    public string Username { get; set; } = null;
    public string Role { get; set; } = null;
}
