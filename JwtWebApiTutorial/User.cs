namespace JwtWebApiTutorial;

public class User
{
    public string Username { get; set; } = String.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    
}