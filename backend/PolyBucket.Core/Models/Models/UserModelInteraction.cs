using Core.Models.Users;
using System;

namespace Core.Models.Models
{
    public class UserModelInteraction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ModelId { get; set; }
        public InteractionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }

        public Model Model { get; set; }
    }

    public enum InteractionType
    {
        View = 0,
        Like = 1,
        Dislike = 2,
        Favorite = 3,
        Download = 4,
        Report = 5
    }
}