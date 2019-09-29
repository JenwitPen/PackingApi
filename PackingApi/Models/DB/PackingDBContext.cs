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

        public virtual DbSet<TbmItemGroup> TbmItemGroup { get; set; }
        public virtual DbSet<TbmRunNo> TbmRunNo { get; set; }
        public virtual DbSet<TbmUser> TbmUser { get; set; }
        public virtual DbSet<TbtInvoice> TbtInvoice { get; set; }
        public virtual DbSet<TbtOrder> TbtOrder { get; set; }
        public virtual DbSet<TbtPackItem> TbtPackItem { get; set; }
        public virtual DbSet<TbtPickItem> TbtPickItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<TbmItemGroup>(entity =>
            {
                entity.HasKey(e => e.ItemGrpCode);

                entity.ToTable("tbm_ItemGroup");

                entity.Property(e => e.ItemGrpCode)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.ItemGrpName).HasMaxLength(50);

                entity.Property(e => e.ItemGrpPrefix).HasMaxLength(50);
            });

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

                entity.Property(e => e.Isbn)
                    .HasColumnName("ISBN")
                    .HasMaxLength(50);

                entity.Property(e => e.ItemCode)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TbtOrder>(entity =>
            {
                entity.ToTable("tbt_Order");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.DocDate).HasColumnType("datetime");

                entity.Property(e => e.DocDueDate).HasColumnType("datetime");

                entity.Property(e => e.DocNum)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Isbn)
                    .HasColumnName("ISBN")
                    .HasMaxLength(50);

                entity.Property(e => e.ItemCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<TbtPackItem>(entity =>
            {
                entity.HasKey(e => new { e.ItemCode, e.DocNum });

                entity.ToTable("tbt_Pack_Item");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.DocNum).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.IsbnRecheck)
                    .HasColumnName("ISBN_Recheck")
                    .HasMaxLength(50);

                entity.Property(e => e.PackNo).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<TbtPickItem>(entity =>
            {
                entity.HasKey(e => new { e.ItemCode, e.DocNum })
                    .HasName("PK_tbt_PickItem");

                entity.ToTable("tbt_Pick_Item");

                entity.Property(e => e.ItemCode).HasMaxLength(50);

                entity.Property(e => e.DocNum).HasMaxLength(50);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.PickNo).HasMaxLength(50);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            });
        }
    }
}
