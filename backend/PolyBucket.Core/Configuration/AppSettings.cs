namespace Core.Configuration
{
    public class AppSettings
    {
        public SecuritySettings Security { get; set; } = new SecuritySettings();
        public EmailSettings Email { get; set; } = new EmailSettings();
        public FrontendSettings Frontend { get; set; } = new FrontendSettings();
        public StorageSettings Storage { get; set; } = new StorageSettings();
    }

    public class SecuritySettings
    {
        public string JwtSecret { get; set; } = "your-super-secret-key-with-at-least-32-characters";
        public string JwtIssuer { get; set; } = "polybucket";
        public string JwtAudience { get; set; } = "polybucket-api";
        public int AccessTokenExpiryMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 7;
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
    }

    public class EmailSettings
    {
        public bool EnableEmailVerification { get; set; } = false;
        public bool RequireEmailVerification { get; set; } = false;
        public string SmtpServer { get; set; } = "smtp.example.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = "user@example.com";
        public string SmtpPassword { get; set; } = "password";
        public bool UseSsl { get; set; } = true;
        public string FromEmail { get; set; } = "noreply@polybucket.com";
        public string FromName { get; set; } = "PolyBucket";
    }

    public class FrontendSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:3000";
    }

    public class StorageSettings
    {
        public string BasePath { get; set; } = "uploads";
        public long MaxFileSize { get; set; } = 1073741824; // 1GB
        public string[] AllowedExtensions { get; set; } = new[] { ".stl", ".obj", ".fbx", ".3ds", ".glb", ".gltf", ".ply", ".step", ".stp", ".iges", ".igs", ".brep", ".png", ".jpg", ".jpeg", ".gif" };
    }
} 