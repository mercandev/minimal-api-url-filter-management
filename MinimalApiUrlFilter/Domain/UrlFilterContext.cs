using Microsoft.EntityFrameworkCore;

namespace MinimalApiUrlFilter.Domain;

public class UrlFilterContext : Microsoft.EntityFrameworkCore.DbContext
{
    public UrlFilterContext(DbContextOptions<UrlFilterContext> options) : base(options)
    {
        
    }
    
    public DbSet<UrlFilter> UrlFilter { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<UrlFilter>().ToTable("urlfilter");
    }
}