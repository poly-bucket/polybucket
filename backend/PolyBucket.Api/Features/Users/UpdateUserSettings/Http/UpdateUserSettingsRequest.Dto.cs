using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.UpdateUserSettings.Http;

public class UpdateUserSettingsRequest
{
    public string? Language { get; set; }
    public string? Theme { get; set; }
    public bool? EmailNotifications { get; set; }
    public string? DefaultPrinterId { get; set; }
    public string? MeasurementSystem { get; set; }
    public string? TimeZone { get; set; }
    public bool? AutoRotateModels { get; set; }
    public string? DashboardViewType { get; set; }
    public string? CardSize { get; set; }
    public string? CardSpacing { get; set; }
    public int? GridColumns { get; set; }
    public Dictionary<string, string>? CustomSettings { get; set; }
}
