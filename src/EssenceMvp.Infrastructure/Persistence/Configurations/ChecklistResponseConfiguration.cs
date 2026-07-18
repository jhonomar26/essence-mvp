using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class ChecklistResponseConfiguration : IEntityTypeConfiguration<ChecklistResponse>
{
    public void Configure(EntityTypeBuilder<ChecklistResponse> e)
    {
        e.ToTable("checklist_response");
        e.Property(r => r.Id).HasColumnName("id");
        e.Property(r => r.ProjectId).HasColumnName("project_id");
        e.Property(r => r.StateChecklistId).HasColumnName("state_checklist_id");
        e.Property(r => r.IsAchieved).HasColumnName("is_achieved");
        e.Property(r => r.Notes).HasColumnName("notes");
        e.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        e.HasOne(r => r.Project).WithMany(p => p.ChecklistResponses).HasForeignKey(r => r.ProjectId);
        e.HasOne(r => r.StateChecklist).WithMany(c => c.Responses).HasForeignKey(r => r.StateChecklistId);
    }
}
