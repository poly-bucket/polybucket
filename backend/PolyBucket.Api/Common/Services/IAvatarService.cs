using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Common.Services
{
    public interface IAvatarService
    {
        /// <summary>
        /// Generates an SVG avatar for the given seed (ID or username)
        /// </summary>
        /// <param name="seed">The seed to generate the avatar from (usually a GUID or username)</param>
        /// <param name="saturation">Saturation percentage (0-100)</param>
        /// <param name="lightness">Lightness percentage (0-100)</param>
        /// <returns>SVG string</returns>
        string GenerateAvatar(string seed, int saturation = 50, int lightness = 50);

                        /// <summary>
                /// Ensures an avatar exists for the given entity, generating one if it doesn't exist
                /// </summary>
                /// <param name="entityId">The entity ID</param>
                /// <param name="currentAvatar">Current avatar SVG or null</param>
                /// <param name="saturation">Saturation percentage (0-100)</param>
                /// <param name="lightness">Lightness percentage (0-100)</param>
                /// <returns>SVG string</returns>
                string EnsureAvatar(Guid entityId, string? currentAvatar, int saturation = 50, int lightness = 50);

                /// <summary>
                /// Generates an avatar for a user with a custom salt
                /// </summary>
                /// <param name="userId">The user ID</param>
                /// <param name="salt">Custom salt to append to the user ID</param>
                /// <param name="saturation">Saturation percentage (0-100)</param>
                /// <param name="lightness">Lightness percentage (0-100)</param>
                /// <returns>SVG string</returns>
                string GenerateUserAvatar(Guid userId, string? salt, int saturation = 50, int lightness = 50);
    }
} 