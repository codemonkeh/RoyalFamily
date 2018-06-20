using RoyalFamily.Common.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace RoyalFamily.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // In a real-world scenario this mapping would be placed in a separate class per entity

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Spouse)
                .WithOne()
                .HasForeignKey<Person>(p => p.SpouseId);

            modelBuilder.Entity<FamilialRelationship>()
                .HasKey(fr => new {fr.ParentId, fr.ChildId});

            modelBuilder.Entity<FamilialRelationship>()
                .HasOne(fr => fr.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(fr => fr.ParentId);

            modelBuilder.Entity<FamilialRelationship>()
                .HasOne(fr => fr.Child)
                .WithMany(c => c.Parents)
                .HasForeignKey(fr => fr.ChildId);
        }
    }
}
