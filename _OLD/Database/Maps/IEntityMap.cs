using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps
{
    public interface IEntityMap<TEntity> : Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<TEntity> where TEntity : class
    {
    }
}