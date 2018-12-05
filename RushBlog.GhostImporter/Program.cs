using Newtonsoft.Json;
using RushBlog.Common;
using RushBlog.DataAccess;
using RushBlog.GhostImporter.Entities;
using RushBlog.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RushBlog.GhostImporter
{
	class Program
	{
		static void Main(string[] args)
		{
			var database = new Database();
			var blogService = new BlogService(database);

			var json = File.ReadAllText(@"C:\Users\rush.frisby.SCTFLASH\Downloads\rush-frisby.ghost.2018-11-16.json");
			var root = JsonConvert.DeserializeObject<RootObject>(json);

			var db = root.db.First();

			foreach(var post in db.data.posts)
			{
				var tags = new List<Common.Tag>();

				var postTags = db.data.posts_tags.Where(x => x.post_id == post.id);
				foreach(var postTag in postTags)
				{
					var tag = db.data.tags.FirstOrDefault(x => x.id == postTag.tag_id);
					if(tag != null)
					{
						tags.Add(new Common.Tag
						{
							Name = tag.name,
							Slug = tag.slug
						});
					}
				}

				Console.WriteLine($"Adding post \"{post.title}\"");

				blogService.AddBlogPost(new BlogPost
				{
					Id = Guid.Parse(post.uuid),
					HtmlContent = post.html,
					MarkdownContent = post.markdown,
					IsPublished = post.visibility == "public",
					Slug = post.slug,
					Title = post.title,
					PublishedOn = post.published_at == null ? (DateTimeOffset?)null : DateTimeOffset.Parse(post.published_at),
					Tags = tags
				});
			}
			
			Console.WriteLine("Done");
			Console.ReadKey();
		}
	}
}
