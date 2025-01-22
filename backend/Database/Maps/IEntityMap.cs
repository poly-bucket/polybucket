using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Maps
{
    public interface IEntityMap<TEntity> where TEntity : class
    {
        public void Configure(EntityTypeBuilder<TEntity> entity);
    }
}