namespace PolyBucket.Api.Features.SystemSettings.DeleteAllFileSettings.Domain
{
    public class DeleteAllFileSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int DeletedCount { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
