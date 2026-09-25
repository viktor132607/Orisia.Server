using Microsoft.EntityFrameworkCore;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<GalleryAlbum> GalleryAlbums => Set<GalleryAlbum>();
    public DbSet<GalleryMedia> GalleryMedia => Set<GalleryMedia>();
    public DbSet<SiteReview> SiteReviews => Set<SiteReview>();
    public DbSet<Dance> Dances => Set<Dance>();
    public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
