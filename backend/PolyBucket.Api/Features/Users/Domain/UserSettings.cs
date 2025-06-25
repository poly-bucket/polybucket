using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;
using System.Collections.Generic;

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
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }
} 