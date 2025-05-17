namespace Core.Entities;

public abstract class Auditable : BaseEntity
{
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? DeletedById { get; set; }
    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}