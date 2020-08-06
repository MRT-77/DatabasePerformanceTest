using Microsoft.EntityFrameworkCore;

namespace DatabasePerformanceTest.DB.MS_SQL
{
    public class MsDbCtx : DbContext
    {
        public readonly string ConnectionString;

        public MsDbCtx(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbSet<Person> Person { get; set; }
        public DbSet<PersonData> PersonData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.Property(e => e.Name).IsRequired().HasMaxLength(7).IsUnicode(false);
            });
            
            modelBuilder.Entity<PersonData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.HasOne(d => d.Person).WithMany(p => p.PersonData)
                    .HasForeignKey(d => d.PersonId).HasPrincipalKey(p => p.Id).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
