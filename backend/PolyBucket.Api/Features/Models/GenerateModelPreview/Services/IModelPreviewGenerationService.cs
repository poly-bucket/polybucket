using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Services
{
    public interface IModelPreviewGenerationService
    {
        Task<ModelPreview> GeneratePreviewAsync(
            Guid modelId, 
            string modelFileUrl, 
            string fileType, 
            string size, 
            PreviewGenerationSettings settings,
            CancellationToken cancellationToken = default);
        
        Task<bool> IsSupportedFileTypeAsync(string fileType);
    }

    public class PreviewGenerationSettings
    {
        public int Width { get; set; } = 800;
        public int Height { get; set; } = 600;
        public string BackgroundColor { get; set; } = "#1a1a1a";
        public string ModelColor { get; set; } = "#888888";
        public double Metalness { get; set; } = 0.5;
        public double Roughness { get; set; } = 0.5;
        public bool AutoRotate { get; set; } = false;
        public double CameraDistance { get; set; } = 2.5;
        public string Lighting { get; set; } = "studio"; // studio, outdoor, indoor
        public string ViewMode { get; set; } = "solid"; // solid, wireframe, points
        public double LightIntensity { get; set; } = 1.0;
        public string LightColor { get; set; } = "#ffffff";
    }
} 