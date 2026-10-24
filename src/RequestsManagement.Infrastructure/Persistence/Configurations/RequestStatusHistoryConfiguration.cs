using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RequestsManagement.Domain.Requests;

namespace RequestsManagement.Infrastructure.Persistence.Configurations;

internal sealed class RequestStatusHistoryConfiguration : IEntityTypeConfiguration<RequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<RequestStatusHistory> builder)
    {
        builder.ToTable("RequestStatusHistory");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.PreviousStatus).IsRequired();

        builder.Property(entry => entry.NewStatus).IsRequired();

        builder.Property(entry => entry.ChangedAt).IsRequired();

        builder.Property(entry => entry.ChangedBy)
            .HasMaxLength(RequestConstraints.UserIdentifierMaxLength)
            .IsRequired();

        builder.HasOne<Request>()
            .WithMany()
            .HasForeignKey(entry => entry.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entry => new { entry.RequestId, entry.ChangedAt })
            .HasDatabaseName("IX_RequestStatusHistory_RequestId_ChangedAt");
    }
}
