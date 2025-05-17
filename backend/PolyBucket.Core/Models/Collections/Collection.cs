using Core.Entities;
using Core.Enumerations;
using Core.Models.Models;
using System;
using System.Collections.Generic;

namespace Core.Models.Collections;

public class Collection : Auditable
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public PrivacySettings Privacy { get; set; }

    #region Navigation Properties

    public User User { get; set; }

    public List<Model> Models { get; set; } = new List<Model>();

    #endregion Navigation Properties
}