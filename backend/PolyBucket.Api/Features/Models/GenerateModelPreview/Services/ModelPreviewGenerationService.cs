using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PuppeteerSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Services
{
    public class ModelPreviewGenerationService : IModelPreviewGenerationService
    {
        private readonly IStorageService _storageService;
        private readonly ILogger<ModelPreviewGenerationService> _logger;
        private readonly string[] _supportedFormats = { ".stl", ".obj", ".fbx", ".gltf", ".glb", ".ply", ".3mf", ".step", ".stp" };

        public ModelPreviewGenerationService(
            IStorageService storageService,
            ILogger<ModelPreviewGenerationService> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<bool> IsSupportedFileTypeAsync(string fileType)
        {
            var extension = fileType.ToLowerInvariant();
            if (!extension.StartsWith("."))
                extension = "." + extension;
            
            return Array.Exists(_supportedFormats, format => format.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ModelPreview> GeneratePreviewAsync(
            Guid modelId,
            string modelFileUrl,
            string fileType,
            string size,
            PreviewGenerationSettings settings,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting preview generation for model {ModelId} with size {Size}", modelId, size);

            try
            {
                // Download and install browser if needed
                await new BrowserFetcher().DownloadAsync();

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });

                using var page = await browser.NewPageAsync();
                
                // Set viewport
                await page.SetViewportAsync(new ViewPortOptions
                {
                    Width = settings.Width,
                    Height = settings.Height,
                    DeviceScaleFactor = 1
                });

                // Generate HTML for 3D model viewer
                var html = GenerateModelViewerHtml(modelFileUrl, fileType, settings);

                // Set content and wait for model to load
                await page.SetContentAsync(html);
                
                // Wait for the model to load and render
                await page.WaitForSelectorAsync("#model-container", new WaitForSelectorOptions { Timeout = 30000 });
                await page.WaitForFunctionAsync("() => window.modelLoaded === true", new WaitForFunctionOptions { Timeout = 30000 });

                // Additional wait for rendering to complete
                await Task.Delay(2000, cancellationToken);

                // Take screenshot
                var screenshotBytes = await page.ScreenshotDataAsync(new ScreenshotOptions
                {
                    Type = ScreenshotType.Png,
                    FullPage = false
                });

                // Process image if needed
                var processedImage = await ProcessImageAsync(screenshotBytes, size, cancellationToken);

                // Upload to storage
                var objectKey = $"previews/{modelId}/{size}_{Guid.NewGuid()}.png";
                using var imageStream = new MemoryStream(processedImage);
                await _storageService.UploadAsync(objectKey, imageStream, "image/png", cancellationToken);

                // Get presigned URL
                var previewUrl = await _storageService.GetPresignedUrlAsync(objectKey, TimeSpan.FromHours(24), cancellationToken);

                _logger.LogInformation("Preview generation completed for model {ModelId} with size {Size}", modelId, size);

                return new ModelPreview
                {
                    Id = Guid.NewGuid(),
                    ModelId = modelId,
                    Size = size,
                    PreviewUrl = previewUrl,
                    StorageKey = objectKey,
                    Status = PreviewStatus.Completed,
                    GeneratedAt = DateTime.UtcNow,
                    Width = settings.Width,
                    Height = settings.Height,
                    FileSizeBytes = processedImage.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preview for model {ModelId} with size {Size}", modelId, size);
                
                return new ModelPreview
                {
                    Id = Guid.NewGuid(),
                    ModelId = modelId,
                    Size = size,
                    Status = PreviewStatus.Failed,
                    ErrorMessage = ex.Message,
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }

        private string GenerateModelViewerHtml(string modelFileUrl, string fileType, PreviewGenerationSettings settings)
        {
            var isSTL = fileType.ToLowerInvariant().EndsWith(".stl");
            var isGLTF = fileType.ToLowerInvariant().EndsWith(".gltf") || fileType.ToLowerInvariant().EndsWith(".glb");

            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Model Preview</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { margin: 0; padding: 0; background-color: " + settings.BackgroundColor + "; }");
            html.AppendLine("#model-container { width: 100%; height: 100vh; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div id='model-container'></div>");
            
            // Include Three.js
            html.AppendLine("<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>");
            html.AppendLine("<script src='https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/STLLoader.js'></script>");
            html.AppendLine("<script src='https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/GLTFLoader.js'></script>");
            html.AppendLine("<script src='https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/OBJLoader.js'></script>");
            
            html.AppendLine("<script>");
            html.AppendLine("window.modelLoaded = false;");
            html.AppendLine("let scene, camera, renderer, model;");
            
            html.AppendLine("function init() {");
            html.AppendLine("  scene = new THREE.Scene();");
            html.AppendLine("  scene.background = new THREE.Color('" + settings.BackgroundColor + "');");
            
            // Camera setup
            html.AppendLine("  camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 1000);");
            html.AppendLine("  camera.position.set(0, 0, " + settings.CameraDistance + ");");
            
            // Renderer setup
            html.AppendLine("  renderer = new THREE.WebGLRenderer({ antialias: true });");
            html.AppendLine("  renderer.setSize(window.innerWidth, window.innerHeight);");
            html.AppendLine("  renderer.shadowMap.enabled = true;");
            html.AppendLine("  renderer.shadowMap.type = THREE.PCFSoftShadowMap;");
            html.AppendLine("  document.getElementById('model-container').appendChild(renderer.domElement);");
            
            // Lighting setup
            html.AppendLine("  const ambientLight = new THREE.AmbientLight('" + settings.LightColor + "', 0.4);");
            html.AppendLine("  scene.add(ambientLight);");
            
            html.AppendLine("  const directionalLight = new THREE.DirectionalLight('" + settings.LightColor + "', " + settings.LightIntensity + ");");
            html.AppendLine("  directionalLight.position.set(10, 10, 5);");
            html.AppendLine("  directionalLight.castShadow = true;");
            html.AppendLine("  scene.add(directionalLight);");
            
            // Load model based on file type
            if (isSTL)
            {
                html.AppendLine("  const loader = new THREE.STLLoader();");
                html.AppendLine("  loader.load('" + modelFileUrl + "', function (geometry) {");
                html.AppendLine("    const material = new THREE.MeshStandardMaterial({");
                html.AppendLine("      color: '" + settings.ModelColor + "',");
                html.AppendLine("      metalness: " + settings.Metalness + ",");
                html.AppendLine("      roughness: " + settings.Roughness + ",");
                html.AppendLine("      wireframe: " + (settings.ViewMode == "wireframe").ToString().ToLower() + "");
                html.AppendLine("    });");
                html.AppendLine("    model = new THREE.Mesh(geometry, material);");
                html.AppendLine("    scene.add(model);");
                html.AppendLine("    centerModel(model);");
                html.AppendLine("    window.modelLoaded = true;");
                html.AppendLine("  });");
            }
            else if (isGLTF)
            {
                html.AppendLine("  const loader = new THREE.GLTFLoader();");
                html.AppendLine("  loader.load('" + modelFileUrl + "', function (gltf) {");
                html.AppendLine("    model = gltf.scene;");
                html.AppendLine("    scene.add(model);");
                html.AppendLine("    centerModel(model);");
                html.AppendLine("    window.modelLoaded = true;");
                html.AppendLine("  });");
            }
            else
            {
                html.AppendLine("  const loader = new THREE.OBJLoader();");
                html.AppendLine("  loader.load('" + modelFileUrl + "', function (object) {");
                html.AppendLine("    model = object;");
                html.AppendLine("    scene.add(model);");
                html.AppendLine("    centerModel(model);");
                html.AppendLine("    window.modelLoaded = true;");
                html.AppendLine("  });");
            }
            
            html.AppendLine("  animate();");
            html.AppendLine("}");
            
            html.AppendLine("function centerModel(object) {");
            html.AppendLine("  const box = new THREE.Box3().setFromObject(object);");
            html.AppendLine("  const center = box.getCenter(new THREE.Vector3());");
            html.AppendLine("  const size = box.getSize(new THREE.Vector3());");
            html.AppendLine("  const maxDim = Math.max(size.x, size.y, size.z);");
            html.AppendLine("  const scale = 1 / maxDim;");
            html.AppendLine("  object.scale.setScalar(scale);");
            html.AppendLine("  object.position.sub(center.multiplyScalar(scale));");
            html.AppendLine("}");
            
            html.AppendLine("function animate() {");
            if (settings.AutoRotate)
            {
                html.AppendLine("  if (model) model.rotation.y += 0.01;");
            }
            html.AppendLine("  renderer.render(scene, camera);");
            html.AppendLine("  requestAnimationFrame(animate);");
            html.AppendLine("}");
            
            html.AppendLine("window.addEventListener('load', init);");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        private async Task<byte[]> ProcessImageAsync(byte[] originalImage, string size, CancellationToken cancellationToken)
        {
            using var inputStream = new MemoryStream(originalImage);
            using var image = await Image.LoadAsync(inputStream, cancellationToken);
            
            // Resize based on size parameter
            var targetWidth = size switch
            {
                "thumbnail" => 300,
                "medium" => 600,
                "large" => 1200,
                _ => 800
            };
            
            var targetHeight = (int)(targetWidth * (double)image.Height / image.Width);
            
            image.Mutate(x => x.Resize(targetWidth, targetHeight));
            
            using var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new PngEncoder(), cancellationToken);
            return outputStream.ToArray();
        }
    }
} 