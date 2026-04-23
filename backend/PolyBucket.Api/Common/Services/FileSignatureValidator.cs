using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyBucket.Api.Common.Services
{
    public class FileSignatureValidator
    {
        private const int BinaryStlHeaderLength = 80;
        private const int BinaryStlTriangleCountLength = 4;
        private const int BinaryStlMinimumLength = BinaryStlHeaderLength + BinaryStlTriangleCountLength;
        private const int BinaryStlFacetLength = 50;

        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } } },
            { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 }, new byte[] { 0x57, 0x45, 0x42, 0x50 } } },
            { ".bmp", new[] { new byte[] { 0x42, 0x4D } } },
            { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".stl", new[] { new byte[] { 0x73, 0x6F, 0x6C, 0x69, 0x64 }, new byte[] { 0x53, 0x54, 0x4C } } },
            { ".obj", new[] { new byte[] { 0x6F, 0x62, 0x6A } } },
            { ".glb", new[] { new byte[] { 0x67, 0x6C, 0x54, 0x46 } } },
            { ".gltf", new[] { new byte[] { 0x7B } } },
            { ".3mf", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".fbx", new[] { new byte[] { 0x4B, 0x61, 0x79, 0x64, 0x61, 0x72, 0x61 } } },
        };

        public static async Task<bool> ValidateFileSignatureAsync(IFormFile file, string expectedExtension)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) && !string.IsNullOrWhiteSpace(expectedExtension))
            {
                extension = expectedExtension.ToLowerInvariant();
            }

            if (extension == ".stl")
            {
                return await ValidateStlSignatureAsync(file);
            }
            
            if (!FileSignatures.ContainsKey(extension))
            {
                return true;
            }

            var signatures = FileSignatures[extension];
            var maxSignatureLength = signatures.Max(s => s.Length);

            try
            {
                using var stream = file.OpenReadStream();
                var buffer = new byte[maxSignatureLength];
                var bytesRead = await stream.ReadAsync(buffer, 0, maxSignatureLength);
                
                if (bytesRead < maxSignatureLength)
                {
                    return false;
                }

                foreach (var signature in signatures)
                {
                    if (buffer.Take(signature.Length).SequenceEqual(signature))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> ValidateStlSignatureAsync(IFormFile file)
        {
            try
            {
                await using var stream = file.OpenReadStream();

                var asciiSignature = FileSignatures[".stl"][0];
                var asciiBuffer = new byte[asciiSignature.Length];
                var asciiBytesRead = await stream.ReadAsync(asciiBuffer, 0, asciiBuffer.Length);
                if (asciiBytesRead == asciiSignature.Length && asciiBuffer.SequenceEqual(asciiSignature))
                {
                    return true;
                }

                if (file.Length < BinaryStlMinimumLength)
                {
                    return false;
                }

                stream.Position = BinaryStlHeaderLength;
                var triangleCountBuffer = new byte[BinaryStlTriangleCountLength];
                var triangleCountBytesRead = await stream.ReadAsync(triangleCountBuffer, 0, triangleCountBuffer.Length);
                if (triangleCountBytesRead != BinaryStlTriangleCountLength)
                {
                    return false;
                }

                var triangleCount = BitConverter.ToUInt32(triangleCountBuffer, 0);
                var expectedLength = BinaryStlMinimumLength + ((long)triangleCount * BinaryStlFacetLength);

                if (expectedLength == file.Length)
                {
                    return true;
                }

                stream.Position = 0;
                using var reader = new StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 256, leaveOpen: true);
                var headerText = await reader.ReadLineAsync();
                return headerText != null && headerText.TrimStart().StartsWith("solid", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsTextFile(string extension)
        {
            var textExtensions = new[] { ".txt", ".md", ".markdown", ".obj" };
            return textExtensions.Contains(extension.ToLowerInvariant());
        }
    }
}

