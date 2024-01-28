using AuthenticationMicroservice.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthenticationMicroservice.Infrastructure;

public class AuthenticationContext : DbContext
{
    public AuthenticationContext(DbContextOptions<AuthenticationContext> options) : base(options)
    {
    }
    public DbSet<User> User { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
    }
}

internal class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.Password).IsRequired();
    }
}
