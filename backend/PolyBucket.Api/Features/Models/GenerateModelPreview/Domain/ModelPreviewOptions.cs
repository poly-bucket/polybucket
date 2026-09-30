namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;

public class ModelPreviewOptions
{
    public const string SectionName = "ModelPreviews";

    public bool WorkerEnabled { get; set; } = true;
    public int PollSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 2;
    public int LockSeconds { get; set; } = 600;
    public int MaxAttempts { get; set; } = 3;
    public string DefaultSize { get; set; } = "thumbnail";
    public string? BrowserExecutablePath { get; set; }
}
