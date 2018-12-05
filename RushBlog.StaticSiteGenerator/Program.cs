using RushBlog.Common;
using RushBlog.DataAccess;
using RushBlog.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace RushBlog.StaticSiteGenerator
{
	class Program
	{
		private const string TitleMarker = "{Title}";
		private static string OutputDir;
		private static string HeaderContent;
		private static string FooterContent;

		static void Main(string[] args)
		{
			if(args == null || args.Length == 0)
			{
				Console.WriteLine("The first argument must specify the output directory path");
			}

			OutputDir = args[0];

			var database = new Database();
			var blogService = new BlogService(database);

			var allBlogPosts = from x in blogService.GetAllBlogPosts()
							   select x;

			var publishedBlogPosts = from x in blogService.GetAllBlogPosts()
									 where x.IsPublished
									 where x.PublishedOn.HasValue
									 where x.PublishedOn.Value <= DateTimeOffset.Now
									 select x;

			var unpublishedBlogPosts = from x in blogService.GetAllBlogPosts()
									   where !x.IsPublished || !x.PublishedOn.HasValue || x.PublishedOn.Value > DateTimeOffset.Now
									   select x;

			var templateSections = blogService.GetAllTemplateSections();

			HeaderContent = templateSections.First(x => x.Name == "Header").Content;

			var footerContent = templateSections.First(x => x.Name == "Footer").Content;
			FooterContent = footerContent.Replace("{Latest}", GetLatestArticlesSidebox(publishedBlogPosts));

			var title = templateSections.First(x => x.Name == "Default Title").Content;
			var postFooter = templateSections.First(x => x.Name == "Post Footer").Content;
			var unpublishedTemplate = templateSections.First(x => x.Name == "Unpublished Post").Content;

			CreateBlogPosts(publishedBlogPosts, postFooter);
			CreateIndexPage(publishedBlogPosts, title);
			CreateMainTagPage(publishedBlogPosts);
			CreateDatePage(publishedBlogPosts);
			CreateUnpublishedRedirects(unpublishedTemplate, unpublishedBlogPosts);
			Create404Page(title, publishedBlogPosts.Concat(unpublishedBlogPosts));
		}

		private static void CreateBlogPosts(IEnumerable<BlogPost> blogPosts, string postFooter)
		{
			foreach (var blogPost in blogPosts)
			{
				var sb = new StringBuilder();
				sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(blogPost.Title)));
				sb.AppendLine(GetPostContainer(blogPost, postFooter));
				sb.AppendLine(FooterContent);

				var fileName = $"{blogPost.Slug}.html";
				var fullFileName = Path.Combine(OutputDir, fileName);

				Console.WriteLine($"Writing file {fullFileName}");

				File.WriteAllText(fullFileName, sb.ToString());
			}
		}

		private static string GetPostContainerOld(BlogPost blogPost, string postFooter = null)
		{
			var sb = new StringBuilder();
			sb.AppendLine("<div class='blogPost'>");
			sb.AppendLine($"<h2><a href='/{blogPost.Slug}'>{HttpUtility.HtmlEncode(blogPost.Title)}</a></h2>");
			sb.AppendLine("<div class='postInfo'>");
			sb.AppendLine($"<span class='date'>{blogPost.PublishedOn.Value.DateTime.ToLongDateString()}</span>");
			sb.AppendLine($"<span class='tags'>");
			foreach(var tag in blogPost.Tags)
			{
				sb.AppendLine($"<a href='/tag/{tag.Slug}'>{HttpUtility.HtmlEncode(tag.Name)}</a>");
			}
			sb.AppendLine("</span>");
			sb.AppendLine("</div>");
			sb.AppendLine("<div class='postContent'>");
			sb.AppendLine(blogPost.HtmlContent);
			sb.AppendLine("</div>");
			sb.AppendLine($"<div class='postFooter{(string.IsNullOrWhiteSpace(postFooter) ? " postFooterEmpty" : "")}'>");
			if (!string.IsNullOrWhiteSpace(postFooter))
			{
				sb.AppendLine(postFooter);
			}
			sb.AppendLine("</div>");
			sb.AppendLine("</div>");
			return sb.ToString();
		}

		private static string GetPostContainer(BlogPost blogPost, string postFooter = null)
		{
			var sb = new StringBuilder();
			sb.AppendLine("<article class='post'>");
			sb.AppendLine("<header class='post-header'>");
			sb.AppendLine($"<a href='/{blogPost.Slug}'><h1 class='post-title'>{HttpUtility.HtmlEncode(blogPost.Title)}</h1></a>");
			sb.AppendLine("<section class='post-meta'>");
			sb.AppendLine($"<time class='post-date' datetime='{blogPost.PublishedOn.Value.DateTime.ToString("yyyy-MM-dd")}'>{blogPost.PublishedOn.Value.DateTime.ToLongDateString()}</time>");
			sb.AppendLine("</section>");
			sb.AppendLine("</header>");
			sb.AppendLine("<section class='post-content'>");
			sb.AppendLine(blogPost.HtmlContent);
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

		private static void CreateIndexPage(IEnumerable<BlogPost> blogPosts, string title)
		{
			var latestPosts = blogPosts.Where(x => !x.IsPage).OrderByDescending(x => x.PublishedOn.Value).Take(5);

			var sb = new StringBuilder();
			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(title)));

			foreach (var blogPost in latestPosts)
			{
				sb.AppendLine(GetPostContainer(blogPost));
			}

			sb.AppendLine(FooterContent);

			var fileName = $"index.html";
			var fullFileName = Path.Combine(OutputDir, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			File.WriteAllText(fullFileName, sb.ToString());
		}

		private static void CreateMainTagPage(IEnumerable<BlogPost> blogPosts)
		{
			var tagFolder = Path.Combine(OutputDir, "tag");
			if(!Directory.Exists(tagFolder))
			{
				Directory.CreateDirectory(tagFolder);
			}

			var tags = blogPosts.SelectMany(x => x.Tags).Distinct().OrderBy(x => x.Name);

			var sb = new StringBuilder();
			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode("Tags")));

			var tagCloud = new StringBuilder();
			var postsPerTag = new StringBuilder();

			foreach (var tag in tags)
			{

				var tagPosts = blogPosts.Where(x => x.Tags.Select(y => y.Id).Contains(tag.Id));
				var count = tagPosts.Count();

				tagCloud.AppendLine($"<a href='#{tag.Slug}' class='tag'>{HttpUtility.HtmlEncode(tag.Name)} ({count})</a>");

				postsPerTag.AppendLine($"<div class='tagList'>");
				postsPerTag.AppendLine($"<a class='tag' name='{tag.Slug}' href='/tag/{tag.Slug}'>{HttpUtility.HtmlEncode(tag.Name)} ({count})</a>");
				postsPerTag.AppendLine("<ol>");
				foreach(var blogPost in tagPosts.OrderByDescending(x => x.PublishedOn.Value))
				{
					postsPerTag.AppendLine($"<li><span class='date'>{HttpUtility.HtmlEncode(blogPost.PublishedOn.Value.DateTime.ToShortDateString())}</span><a href='/{blogPost.Slug}' class='title'>{HttpUtility.HtmlEncode(blogPost.Title)}</a></li>");
				}
				postsPerTag.AppendLine("</ol>");
				postsPerTag.AppendLine($"</div>");

				CreateTagPage(tag, tagFolder, tagPosts);
			}

			sb.AppendLine("<div class='tagCloud'>");
			sb.AppendLine(tagCloud.ToString());
			sb.AppendLine("</div>");

			sb.AppendLine("<div class='tagListContainer'>");
			sb.AppendLine(postsPerTag.ToString());
			sb.AppendLine("</div>");

			sb.AppendLine(FooterContent);

			var fileName = $"tags.html";
			var fullFileName = Path.Combine(OutputDir, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			File.WriteAllText(fullFileName, sb.ToString());
		}

		private static void CreateTagPage(Tag tag, string tagFolder, IEnumerable<BlogPost> blogPosts)
		{
			var sb = new StringBuilder();

			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(tag.Name)));

			sb.AppendLine($"<h2 class='tag'>{HttpUtility.HtmlEncode(tag.Name)}</h2>");

			foreach(var blogPost in blogPosts)
			{
				sb.AppendLine(GetPostContainer(blogPost));
			}

			sb.AppendLine(FooterContent);

			var fileName = $"{tag.Slug}.html";
			var fullFileName = Path.Combine(tagFolder, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			File.WriteAllText(fullFileName, sb.ToString());
		}

		private static void CreateDatePage(IEnumerable<BlogPost> blogPosts)
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
			var fullFileName = Path.Combine(OutputDir, fileName);

			Console.WriteLine($"Writing file {fullFileName}");

			File.WriteAllText(fullFileName, sb.ToString());
		}

		private static void CreateUnpublishedRedirects(string pageContent, IEnumerable<BlogPost> blogPosts)
		{
			foreach (var blogPost in blogPosts)
			{
				var html = pageContent.Replace(TitleMarker, HttpUtility.HtmlEncode(blogPost.Title));

				var fileName = $"{blogPost.Slug}.html";
				var fullFileName = Path.Combine(OutputDir, fileName);

				Console.WriteLine($"Writing file {fullFileName}");

				File.WriteAllText(fullFileName, html);
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

		private static void Create404Page(string title, IEnumerable<BlogPost> blogPosts)
		{
			var sb = new StringBuilder();
			sb.AppendLine(HeaderContent.Replace(TitleMarker, HttpUtility.HtmlEncode(title)));

			sb.AppendLine("<p>The page you are looking for could not be found.</p>");

			sb.AppendLine("<script type='text/javascript'>");
			sb.AppendLine("var currentUrl = window.location.href;");
			sb.AppendLine("var lastChar = currentUrl.substr(currentUrl.length - 1);");
			sb.AppendLine("if(lastChar === '/') {");
			sb.AppendLine("	var prevSpot = currentUrl.lastIndexOf('/', currentUrl.length - 2);");
			sb.AppendLine("	var slug = currentUrl.substring(prevSpot + 1, currentUrl.length - 1);");
			var slugs = string.Join("\",\"", blogPosts.Select(x => x.Slug).OrderBy(x => x));
			sb.AppendLine($"	var slugs = [\"{slugs}\"];");
			sb.AppendLine("	for (i = 0; i < slugs.length; i++) {");
			sb.AppendLine("		if(slug === slugs[i])");
			sb.AppendLine("		{");
			sb.AppendLine("			var nextUrl = currentUrl.substring(0, currentUrl.length - 1);");
			sb.AppendLine("			window.location.replace(nextUrl);");
			sb.AppendLine("			break;");
			sb.AppendLine("		}");
			sb.AppendLine("	}");
			sb.AppendLine("}");
			sb.AppendLine("</script>");

			sb.AppendLine(FooterContent);

			var fileName = $"404.html";
			var fullFileName = Path.Combine(OutputDir, fileName);
			Console.WriteLine($"Writing file {fullFileName}");
			File.WriteAllText(fullFileName, sb.ToString());
		}
	}
}
