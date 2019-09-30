using Newtonsoft.Json;
using RushBlog.Common;
using RushBlog.DataAccess;
using RushBlog.Logic;
using System;
using System.IO;
using System.Linq;

namespace RushBlog.ConversionTool
{
    class Program
    {
        private static readonly DirectoryInfo BaseDirectory = new DirectoryInfo(@"C:\dev\github\rushfrisby.com2\_brush\");

        static void Main(string[] args)
        {
            IBlogService svc = new BlogService(new Database());
            CreateTemplateSections(svc);
            CreateBlogPosts(svc);
            CreateTags(svc);
        }

        private static void CreateTemplateSections(IBlogService svc)
        {
            var templateDirectory = BaseDirectory.CreateSubdirectory("templates");

            var templateSections = svc.GetAllTemplateSections();

            foreach (var item in templateSections)
            {
                var fileName = Path.Combine(templateDirectory.FullName, $"{item.Name.Replace(" ", "_")}.html");
                File.WriteAllText(fileName, item.Content);
            }
        }

        private static void CreateBlogPosts(IBlogService svc)
        {
            var postsDirectory = BaseDirectory.CreateSubdirectory("posts");
            var items = svc.GetAllBlogPosts();
            var tags = svc.GetAllTags();

            var data = items.Select(x => new Post
            {
                IsPage = x.IsPage,
                Title = x.Title,
                IsPublished = x.IsPublished,
                PublishedOn = x.PublishedOn,
                Slug = x.Slug,
                Tags = tags.Where(y => y.BlogPostId == x.Id).Select(y => y.Slug).ToArray()
            })
            .OrderBy(x => x.Slug);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var jsonFileName = Path.Combine(BaseDirectory.FullName, "posts.json");
            File.WriteAllText(jsonFileName, json);

            foreach (var item in items)
            {
                var fileName = Path.Combine(postsDirectory.FullName, $"{item.Slug}.md");
                File.WriteAllText(fileName, item.MarkdownContent);
            }
        }

        private static void CreateTags(IBlogService svc)
        {
            var tags = svc.GetAllTags();
            var data = tags.OrderBy(x => x.Slug).ToDictionary(x => x.Slug, x => x.Name);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var jsonFileName = Path.Combine(BaseDirectory.FullName, "tags.json");
            File.WriteAllText(jsonFileName, json);
        }
    }

    public class Post
    {
        public string Title { get; set; }
        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedOn { get; set; }
        public string Slug { get; set; }
        public string[] Tags { get; set; }
        public bool IsPage { get; set; }
    }
}
