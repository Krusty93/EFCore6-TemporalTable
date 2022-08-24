using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore6.TemporalTables.API;

public class ProductDataConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .ToTable("Products", b => b.IsTemporal());

        builder.HasKey(x => x.Id);

        builder
            .Property(o => o.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Price).IsRequired();
    }
}
