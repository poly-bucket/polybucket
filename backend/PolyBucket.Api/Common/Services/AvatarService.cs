using System;
using System.Security.Cryptography;
using System.Text;

namespace PolyBucket.Api.Common.Services
{
    public class AvatarService : IAvatarService
    {
        public string GenerateAvatar(string seed, int saturation = 50, int lightness = 50)
        {
            // Simple hash function to convert string to number
            var hash = GetHash(seed);
            
            // Use the hash to generate a 5x5 pixel matrix with vertical symmetry
            var matrix = GenerateMatrix(hash);
            
            // Generate color from hash
            var hue = hash % 360;
            
            // Create SVG
            return GenerateSvg(matrix, hue, saturation, lightness);
        }

        public string EnsureAvatar(Guid entityId, string? currentAvatar, int saturation = 50, int lightness = 50)
        {
            if (!string.IsNullOrEmpty(currentAvatar))
            {
                return currentAvatar;
            }

            return GenerateAvatar(entityId.ToString(), saturation, lightness);
        }

        public string GenerateUserAvatar(Guid userId, string? salt, int saturation = 50, int lightness = 50)
        {
            var seed = string.IsNullOrEmpty(salt) ? userId.ToString() : $"{userId}-{salt}";
            return GenerateAvatar(seed, saturation, lightness);
        }

        private int GetHash(string input)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hash = BitConverter.ToInt32(hashBytes, 0);
            return Math.Abs(hash);
        }

        private bool[,] GenerateMatrix(int hash)
        {
            var matrix = new bool[5, 5];
            
            // Use the last 15 bits of the hash to determine which squares to fill
            // We only need to fill the left half (3 columns) due to vertical symmetry
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    var bitIndex = row * 3 + col;
                    var bit = (hash >> bitIndex) & 1;
                    matrix[row, col] = bit == 1;
                    // Mirror to the right side for vertical symmetry
                    matrix[row, 4 - col] = bit == 1;
                }
            }
            
            return matrix;
        }

        private string GenerateSvg(bool[,] matrix, int hue, int saturation, int lightness)
        {
            var color = $"hsl({hue}, {saturation}%, {lightness}%)";
            var svg = new StringBuilder();
            
            svg.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 5 5\">");
            svg.AppendLine($"<rect width=\"5\" height=\"5\" fill=\"{color}\"/>");
            
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 5; col++)
                {
                    if (matrix[row, col])
                    {
                        svg.AppendLine($"<rect x=\"{col}\" y=\"{row}\" width=\"1\" height=\"1\" fill=\"white\"/>");
                    }
                }
            }
            
            svg.AppendLine("</svg>");
            return svg.ToString();
        }
    }
} 