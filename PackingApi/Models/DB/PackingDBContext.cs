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

        public virtual DbSet<TbmRunNo> TbmRunNo { get; set; }
        public virtual DbSet<TbmUser> TbmUser { get; set; }
        public virtual DbSet<TbtInvoice> TbtInvoice { get; set; }
        public virtual DbSet<TbtPick> TbtPick { get; set; }
        public virtual DbSet<TbtPickInvoice> TbtPickInvoice { get; set; }
  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<TbmRunNo>(entity =>
            {
                entity.ToTable("tbm_RunNo");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Type).HasMaxLength(50);
            });

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

            modelBuilder.Entity<TbtPick>(entity =>
            {
                entity.HasKey(e => e.PickNo);

                entity.ToTable("tbt_Pick");

                entity.Property(e => e.PickNo)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<TbtPickInvoice>(entity =>
            {
                entity.ToTable("tbt_Pick_Invoice");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DocDate).HasColumnType("datetime");

                entity.Property(e => e.DocDueDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ItemCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PickNo).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });
        }
    }
}
