using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Infrastructure.Persistence.Configurations;

internal sealed class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("Requests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Title)
            .HasMaxLength(RequestConstraints.TitleMaxLength)
            .IsRequired();

        builder.Property(request => request.OrganizationName)
            .HasMaxLength(RequestConstraints.OrganizationNameMaxLength)
            .IsRequired();

        builder.Property(request => request.AssignedTo)
            .HasMaxLength(RequestConstraints.UserIdentifierMaxLength);

        builder.Property(request => request.Status).IsRequired();

        builder.Property(request => request.Priority).IsRequired();

        builder.Property(request => request.CreatedAt).IsRequired();

        builder.Property(request => request.UpdatedAt).IsRequired();

        builder.Property(request => request.RowVersion).IsRowVersion();

        builder.HasIndex(request => request.CreatedAt)
            .IsDescending()
            .HasDatabaseName("IX_Requests_CreatedAt");

        builder.HasIndex(request => new { request.Status, request.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Requests_Status_CreatedAt");

        builder.HasIndex(request => new { request.Priority, request.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Requests_Priority_CreatedAt");

        builder.HasIndex(request => new { request.AssignedTo, request.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Requests_AssignedTo_CreatedAt");

        builder.HasIndex(request => new { request.OrganizationName, request.CreatedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_Requests_OrganizationName_CreatedAt");
    }
}
