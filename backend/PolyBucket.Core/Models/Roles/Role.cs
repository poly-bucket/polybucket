using Core.Entities;
using System;

namespace Core.Models.Roles;

public class Role : Auditable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsSystemRole { get; set; } = false;

    //TOOD: Add permissions
}