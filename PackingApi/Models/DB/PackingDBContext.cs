using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PackingApi.Models.DB
{
    public partial class PackingDBContext : DbContext
    {
        public PackingDBContext()
        {
        }

        public PackingDBContext(DbContextOptions<PackingDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TbmUser> TbmUser { get; set; }
        public virtual DbSet<TbtInvoice> TbtInvoice { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
          
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<TbmUser>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("tbm_User");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UserName).HasMaxLength(50);
            });

            modelBuilder.Entity<TbtInvoice>(entity =>
            {
                entity.ToTable("tbt_Invoice");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DocDate).HasColumnType("datetime");

                entity.Property(e => e.DocDueDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ItemCode)
                    .IsRequired()
                    .HasMaxLength(50);
            });
        }
    }
}
