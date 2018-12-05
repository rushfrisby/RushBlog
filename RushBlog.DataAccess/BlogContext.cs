using Microsoft.EntityFrameworkCore;
using RushBlog.Common;

namespace RushBlog.DataAccess
{
	internal class BlogContext : DbContext
	{
		public DbSet<BlogPost> BlogPosts {get;set;}

		public DbSet<Tag> Tags { get; set; }

		public DbSet<TemplateSection> TemplateSections { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=RushBlog;Trusted_Connection=True;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<TemplateSection>().HasData(
			   new TemplateSection { Id = 1, Name = "Header", Content = "" },
			   new TemplateSection { Id = 2, Name = "Footer", Content = "" },
			   new TemplateSection { Id = 3, Name = "Default Title", Content = "" },
			   new TemplateSection { Id = 4, Name = "Post Footer", Content = "" },
			   new TemplateSection { Id = 5, Name = "Unpublished Post", Content = "" }
			  );
		}
	}
}
