using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.RegistrationNumber).IsRequired().HasMaxLength(20);
        builder.Property(s => s.GPA).HasPrecision(3, 2);
        builder.Property(s => s.IsActive).IsRequired();
        builder.Property(s => s.IsDeleted).IsRequired().HasDefaultValue(false);

        // Shadow property (no CLR property on Student, but column exists in DB)
        builder.Property<DateTime>("LastUpdated")
            .HasColumnType("timestamp without time zone");

        // Concurrency token
        builder.Property(s => s.Version)
            .IsConcurrencyToken();

        // EX-9: Task 1: Global query filter — soft-deleted are invisible to normal queries.
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}








