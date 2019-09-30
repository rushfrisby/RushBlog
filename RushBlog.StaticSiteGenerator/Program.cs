using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RushBlog.StaticSiteGenerator
{
    class Program
    {
        private const string BrushDirectoryName = "_brush";
        private const string TemplatesDirectoryName = "templates";
        private const string PostsDirectoryName = "posts";
        private const string TitleMarker = "{Title}";
        private const string DefaultMarkdownContentFormat = "[//]: # ({0})\r\n";

        private static string HeaderContent;
		private static string FooterContent;
        private static DirectoryInfo BaseDirectory;
        private static DirectoryInfo BrushDirectory;
        private static DirectoryInfo TemplatesDirectory;
        private static DirectoryInfo PostsDirectory;

        static async Task Main(string[] args)
		{
            if (args != null && args.Length > 0)
            {
                if (string.Equals("help", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    PrintHelp();
                    return;
                }
                if (string.Equals("new", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    await CreateNewPost(args);
                    return;
                }
                if (string.Equals("gen", args[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    await GenerateSite(args);
                    return;
                }
            }
            PrintHelp();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine($"\t new \t Adds an entry to posts.json and creates a new Markdown file.");
            Console.WriteLine($"\t gen \t Generates the site from source within the `{BrushDirectoryName}` directory.");
        }

        private static async Task CreateNewPost(string[] args)
        {
            var baseDirectoryPath = Directory.GetCurrentDirectory();
            if (args.Length > 1)
            {
                baseDirectoryPath = args[1];
            }

            BaseDirectory = new DirectoryInfo(baseDirectoryPath);
            if (!BaseDirectory.Exists)
            {
                Console.WriteLine($"{BaseDirectory.FullName} doesn't exist");
                return;
            }

            BrushDirectory = new DirectoryInfo(Path.Combine(BaseDirectory.FullName, BrushDirectoryName));
            if (!BrushDirectory.Exists)
            {
                Console.WriteLine($"{BrushDirectory.FullName} doesn't exist");
                return;
            }

            var blogPost = new BlogPost
            {
                PublishedOn = DateTimeOffset.UtcNow,
                IsPublished = false,
                IsPage = false,
                Tags = new string[0]
            };

            Console.Write("Post Title: ");
            blogPost.Title = Console.ReadLine();
            blogPost.Slug = GenerateSlug(blogPost.Title);

            var postsJsonFileName = Path.Combine(BrushDirectory.FullName, "posts.json");
            var postsJson = await File.ReadAllTextAsync(postsJsonFileName);
            var allBlogPosts = JsonConvert.DeserializeObject<List<BlogPost>>(postsJson);

            allBlogPosts.Insert(0, blogPost);

            postsJson = JsonConvert.SerializeObject(allBlogPosts, Formatting.Indented);
            await File.WriteAllTextAsync(postsJsonFileName, postsJson);

            PostsDirectory = new DirectoryInfo(Path.Combine(BrushDirectory.FullName, PostsDirectoryName));
            var mdFileName = Path.Combine(PostsDirectory.FullName, $"{blogPost.Slug}.md");
            await File.WriteAllTextAsync(mdFileName, string.Format(DefaultMarkdownContentFormat, blogPost.Title));
        }

        private static string GenerateSlug(string phrase)
        {
            var slug = phrase.ToLower();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", ""); // invalid chars
            slug = Regex.Replace(slug, @"\s+", " ").Trim(); // convert multiple spaces into one space
            slug = Regex.Replace(slug, @"\s", "-"); // hyphens   
            return slug;
        }

        private static async Task GenerateSite(string[] args)
        {
            var baseDirectoryPath = Directory.GetCurrentDirectory();
            if (args.Length > 1)
            {
                baseDirectoryPath = args[1];
            }

            BaseDirectory = new DirectoryInfo(baseDirectoryPath);
            if (!BaseDirectory.Exists)
            {
                Console.WriteLine($"{BaseDirectory.FullName} doesn't exist");
                return;
            }

            BrushDirectory = new DirectoryInfo(Path.Combine(BaseDirectory.FullName, BrushDirectoryName));
            if (!BrushDirectory.Exists)
            {
                Console.WriteLine($"{BrushDirectory.FullName} doesn't exist");
                return;
            }

            TemplatesDirectory = new DirectoryInfo(Path.Combine(BrushDirectory.FullName, TemplatesDirectoryName));
            PostsDirectory = new DirectoryInfo(Path.Combine(BrushDirectory.FullName, PostsDirectoryName));

            var postsJsonFileName = Path.Combine(BrushDirectory.FullName, "posts.json");
            var postsJson = await File.ReadAllTextAsync(postsJsonFileName);
            var allBlogPosts = JsonConvert.DeserializeObject<IEnumerable<BlogPost>>(postsJson);

            var tagsJsonFileName = Path.Combine(BrushDirectory.FullName, "tags.json");
            var tagsJson = await File.ReadAllTextAsync(tagsJsonFileName);
            var tags = JsonConvert.DeserializeObject<Dictionary<string, string>>(tagsJson);

            var publishedBlogPosts = from x in allBlogPosts
                                     where x.IsPublished
                                     where x.PublishedOn.HasValue
                                     where x.PublishedOn.Value <= DateTimeOffset.Now
                                     select x;

            var unpublishedBlogPosts = from x in allBlogPosts
                                       where !x.IsPublished || !x.PublishedOn.HasValue || x.PublishedOn.Value > DateTimeOffset.Now
                                       select x;

            HeaderContent = await GetTemplateContent("Header");

            var footerContent = await GetTemplateContent("Footer");
            FooterContent = footerContent.Replace("{Latest}", GetLatestArticlesSidebox(publishedBlogPosts));

            var title = await GetTemplateContent("Default_Title");
            var postFooter = await GetTemplateContent("Post_Footer");
            var unpublishedTemplate = await GetTemplateContent("Unpublished_Post");

            await CreateBlogPosts(publishedBlogPosts, postFooter);
            await CreateIndexPage(publishedBlogPosts, title);
            await CreateMainTagPage(tags, publishedBlogPosts);
            await CreateDatePage(publishedBlogPosts);
            await CreateUnpublishedRedirects(unpublishedTemplate, unpublishedBlogPosts);
            await Create404Page(title);
        }

		private static async Task CreateBlogPosts(IEnumerable<BlogPost> blogPosts, string postFooter)
		{
			foreach (var blogPost in blogPosts)
			{
				var sb = new StringBuilder();
				sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(blogPost.Title)));
				sb.AppendLine(await GetPostContainer(blogPost, postFooter));
				sb.AppendLine(FooterContent);

				var fileName = $"{blogPost.Slug}.html";
				var fullFileName = Path.Combine(BaseDirectory.FullName, fileName);

				Console.WriteLine($"Writing file {fullFileName}");

				await File.WriteAllTextAsync(fullFileName, sb.ToString());
			}
		}

		private static async Task<string> GetPostContainer(BlogPost blogPost, string postFooter = null)
		{
            var mdFileName = Path.Combine(PostsDirectory.FullName, $"{blogPost.Slug}.md");
            var mdContent = "";
            if(File.Exists(mdFileName))
            {
                mdContent = await File.ReadAllTextAsync(mdFileName);
            }
            else
            {
                await File.WriteAllTextAsync(mdFileName, string.Format(DefaultMarkdownContentFormat, blogPost.Title));
                Console.WriteLine($"Writing file {mdFileName}");
            }
            var htmlContent = Markdown.ToHtml(mdContent);

			var sb = new StringBuilder();
			sb.AppendLine("<article class='post'>");
			sb.AppendLine("<header class='post-header'>");
			sb.AppendLine($"<a href='/{blogPost.Slug}'><h1 class='post-title'>{HttpUtility.HtmlEncode(blogPost.Title)}</h1></a>");
			sb.AppendLine("<section class='post-meta'>");
			sb.AppendLine($"<time class='post-date' datetime='{blogPost.PublishedOn.Value.DateTime.ToString("yyyy-MM-dd")}'>{blogPost.PublishedOn.Value.DateTime.ToLongDateString()}</time>");
			sb.AppendLine("</section>");
			sb.AppendLine("</header>");
			sb.AppendLine("<section class='post-content'>");
			sb.AppendLine(htmlContent);
			sb.AppendLine("</section>");

			sb.AppendLine("<footer class='post-footer'>");
			sb.AppendLine("</footer>");

			if (!string.IsNullOrWhiteSpace(postFooter))
			{
				sb.AppendLine(postFooter);
			}

			sb.AppendLine("</article>");
			return sb.ToString();
		}

		private static async Task CreateIndexPage(IEnumerable<BlogPost> blogPosts, string title)
		{
			var latestPosts = blogPosts.Where(x => !x.IsPage).OrderByDescending(x => x.PublishedOn.Value).Take(5);

			var sb = new StringBuilder();
			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(title)));

			foreach (var blogPost in latestPosts)
			{
				sb.AppendLine(await GetPostContainer(blogPost));
			}

			sb.AppendLine(FooterContent);

			var fileName = $"index.html";
			var fullFileName = Path.Combine(BaseDirectory.FullName, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			await File.WriteAllTextAsync(fullFileName, sb.ToString());
		}

		private static async Task CreateMainTagPage(Dictionary<string, string> tagLookup, IEnumerable<BlogPost> blogPosts)
		{
			var tagFolder = Path.Combine(BaseDirectory.FullName, "tag");
			if(!Directory.Exists(tagFolder))
			{
				Directory.CreateDirectory(tagFolder);
			}

			var sb = new StringBuilder();
			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode("Tags")));

			var tagCloud = new StringBuilder();
			var postsPerTag = new StringBuilder();

            var tags = blogPosts.SelectMany(x => x.Tags).Distinct();

			foreach (var tag in tags.OrderBy(x => x))
			{
				var tagPosts = blogPosts.Where(x => x.Tags.Contains(tag)).OrderByDescending(x => x.PublishedOn.Value);
				var count = tagPosts.Count();

                var tagName = tag;
                if(tagLookup.ContainsKey(tag))
                {
                    tagName = tagLookup[tag].Trim();
                }

				tagCloud.AppendLine($"<a href='#{tag}' class='tag'>{HttpUtility.HtmlEncode(tagName)} ({count})</a>");

				postsPerTag.AppendLine($"<div class='tagList'>");
				postsPerTag.AppendLine($"<a class='tag' name='{tag}' href='/tag/{tag}'>{HttpUtility.HtmlEncode(tagName)} ({count})</a>");
				postsPerTag.AppendLine("<ol>");
				foreach(var blogPost in tagPosts.OrderByDescending(x => x.PublishedOn.Value))
				{
					postsPerTag.AppendLine($"<li><span class='date'>{HttpUtility.HtmlEncode(blogPost.PublishedOn.Value.DateTime.ToShortDateString())}</span><a href='/{blogPost.Slug}' class='title'>{HttpUtility.HtmlEncode(blogPost.Title)}</a></li>");
				}
				postsPerTag.AppendLine("</ol>");
                postsPerTag.AppendLine($"</div>");

                await CreateTagPage(tag, tagName, tagFolder, tagPosts);
			}

            sb.AppendLine("<h2>Tag Cloud</h2>");
			sb.AppendLine("<div class='tagCloud'>");
			sb.AppendLine(tagCloud.ToString());
			sb.AppendLine("</div>");

            sb.AppendLine("<h2>Posts by Tag</h2>");
            sb.AppendLine("<div class='tagListContainer'>");
			sb.AppendLine(postsPerTag.ToString());
			sb.AppendLine("</div>");

			sb.AppendLine(FooterContent);

			var fileName = $"tags.html";
			var fullFileName = Path.Combine(BaseDirectory.FullName, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			await File.WriteAllTextAsync(fullFileName, sb.ToString());
		}

		private static async Task CreateTagPage(string tag, string tagName, string tagFolder, IEnumerable<BlogPost> blogPosts)
		{
            var sb = new StringBuilder();

			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(tagName)));

			sb.AppendLine($"<h2 class='tag'>{HttpUtility.HtmlEncode(tagName)}</h2>");

			foreach(var blogPost in blogPosts)
			{
				sb.AppendLine(await GetPostContainer(blogPost));
			}

			sb.AppendLine(FooterContent);

			var fileName = $"{tag}.html";
			var fullFileName = Path.Combine(tagFolder, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			await File.WriteAllTextAsync(fullFileName, sb.ToString());
		}

		private static async Task CreateDatePage(IEnumerable<BlogPost> blogPosts)
		{
			var sb = new StringBuilder();

			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode("Archive")));

			sb.AppendLine($"<h2 class='archive'>Archive</h2>");

			var currentYear = 0;

			var filteredBlogPosts = blogPosts.Where(x => !x.IsPage);

			foreach (var blogPost in filteredBlogPosts.OrderByDescending(x => x.PublishedOn.Value))
			{
				if(currentYear != blogPost.PublishedOn.Value.Year)
				{
					if(currentYear != 0)
					{
						sb.AppendLine("</ol>");
					}
					currentYear = blogPost.PublishedOn.Value.Year;
					sb.AppendLine($"<h3 class='year'>{currentYear}</h3>");
					sb.AppendLine("<ol>");
				}
				sb.AppendLine($"<li><span class='date'>{HttpUtility.HtmlEncode(blogPost.PublishedOn.Value.DateTime.ToShortDateString())}</span><a href='/{blogPost.Slug}' class='title'>{HttpUtility.HtmlEncode(blogPost.Title)}</a></li>");
			}
			if(filteredBlogPosts.Count() > 0)
			{
				sb.AppendLine("</ol>");
			}

			sb.AppendLine(FooterContent);

			var fileName = $"archive.html";
			var fullFileName = Path.Combine(BaseDirectory.FullName, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			await File.WriteAllTextAsync(fullFileName, sb.ToString());
		}

		private static async Task CreateUnpublishedRedirects(string pageContent, IEnumerable<BlogPost> blogPosts)
		{
			foreach (var blogPost in blogPosts)
			{
                var mdFileName = Path.Combine(PostsDirectory.FullName, $"{blogPost.Slug}.md");
                if (!File.Exists(mdFileName))
                {
                    await File.WriteAllTextAsync(mdFileName, string.Format(DefaultMarkdownContentFormat, blogPost.Title));
                    Console.WriteLine($"Writing file {mdFileName}");
                }

                var html = pageContent.Replace(TitleMarker, HttpUtility.HtmlEncode(blogPost.Title));
				var fileName = Path.Combine(BaseDirectory.FullName, $"{blogPost.Slug}.html");
				Console.WriteLine($"Writing file {fileName}");
				await File.WriteAllTextAsync(fileName, html);
			}
		}

		private static string GetLatestArticlesSidebox(IEnumerable<BlogPost> blogPosts)
		{
			var sb = new StringBuilder();
			sb.AppendLine("<div class='box sidebox latest-articles'>");
			sb.AppendLine("<div class='sidebox-title'>Latest Articles</div>");
			sb.AppendLine("<div class='sidebox-content'>");

			var latestBlogPosts = blogPosts.Where(x => !x.IsPage).OrderByDescending(x => x.PublishedOn.Value).Take(5);
			foreach (var blogPost in latestBlogPosts)
			{
				sb.AppendLine($"<a href='/{blogPost.Slug}'><div class='date'>{blogPost.PublishedOn.Value.DateTime.ToString("d MMM yyyy").ToUpper()}</div><div>{HttpUtility.HtmlEncode(blogPost.Title)}</div></a>");
			}
			sb.AppendLine("</div>");
			sb.AppendLine("</div>");
			return sb.ToString();
		}

		private static async Task Create404Page(string title)
		{
			var sb = new StringBuilder();
			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(title)));
            sb.AppendLine(await GetTemplateContent("404"));
			sb.AppendLine(FooterContent);
			var fileName = Path.Combine(BaseDirectory.FullName, "404.html");
			Console.WriteLine($"Writing file {fileName}");
			await File.WriteAllTextAsync(fileName, sb.ToString());
		}

        private static async Task<string> GetTemplateContent(string name)
        {
            var fileName = Path.Combine(TemplatesDirectory.FullName, $"{name}.html");
            if(!File.Exists(fileName))
            {
                return "";
            }
            return await File.ReadAllTextAsync(fileName);
        }
	}
}
