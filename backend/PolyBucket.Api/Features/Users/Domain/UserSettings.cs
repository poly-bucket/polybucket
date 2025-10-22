using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolyBucket.Api.Features.Users.Domain
{
    public class UserSettings : BaseEntity
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public string Language { get; set; } = "en";
        public string Theme { get; set; } = "dark";
        public bool EmailNotifications { get; set; } = true;
        public Guid? DefaultPrinterId { get; set; }
        public string MeasurementSystem { get; set; } = "metric";
        public string TimeZone { get; set; } = "UTC";
        public bool AutoRotateModels { get; set; } = true;
        
        // Dashboard Layout Settings
        public string DashboardViewType { get; set; } = "grid"; // "grid" or "list"
        public string CardSize { get; set; } = "medium"; // "small", "medium", "large"
        public string CardSpacing { get; set; } = "normal"; // "compact", "normal", "spacious"
        public int GridColumns { get; set; } = 4; // Number of columns in grid view
        
        // Profile settings
        public bool ShowProfileInSearch { get; set; } = true;
        public bool AllowDirectMessages { get; set; } = true;
        public bool ShowActivityStatus { get; set; } = true;
        public bool NotifyOnMentions { get; set; } = true;
        public bool NotifyOnFollows { get; set; } = true;
        public bool NotifyOnLikes { get; set; } = true;
        public bool NotifyOnComments { get; set; } = true;
        
        [NotMapped]
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }
} 