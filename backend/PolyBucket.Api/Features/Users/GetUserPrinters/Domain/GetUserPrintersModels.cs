using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetUserPrinters.Domain;

public class GetUserPrintersQuery
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class GetUserPrintersResult
{
    public IEnumerable<UserPrinterListItemDto> Printers { get; set; } = new List<UserPrinterListItemDto>();
    public IEnumerable<UserFilamentListItemDto> Filaments { get; set; } = new List<UserFilamentListItemDto>();
    public int TotalPrinterCount { get; set; }
    public int TotalFilamentCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserPrinterListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public class UserFilamentListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Diameter { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
