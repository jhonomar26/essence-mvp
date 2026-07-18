using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class StateChecklistConfiguration : IEntityTypeConfiguration<StateChecklist>
{
    public void Configure(EntityTypeBuilder<StateChecklist> e)
    {
        e.ToTable("state_checklist");
        e.Property(c => c.Id).HasColumnName("id");
        e.Property(c => c.AlphaStateId).HasColumnName("alpha_state_id");
        e.Property(c => c.CriterionText).HasColumnName("criterion_text");
        e.Property(c => c.IsMandatory).HasColumnName("is_mandatory");
        e.HasOne(c => c.AlphaState).WithMany(s => s.Checklists).HasForeignKey(c => c.AlphaStateId);
    }
}
