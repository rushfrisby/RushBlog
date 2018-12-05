using RushBlog.Common;
using RushBlog.DataAccess;
using System;
using System.Linq;
using System.Collections.Generic;

namespace RushBlog.Logic
{
	public class BlogService : IBlogService
	{
		private readonly IDatabase _database;
		private readonly IRepository<BlogPost> _blogPosts;
		private readonly IRepository<Tag> _tags;
		private readonly IRepository<TemplateSection> _sections;

		public BlogService(IDatabase database)
		{
			_database = database ?? throw new ArgumentNullException(nameof(database));
			_blogPosts = database.Repository<BlogPost>();
			_tags = database.Repository<Tag>();
			_sections = database.Repository<TemplateSection>();
		}

		public BlogPost AddBlogPost(BlogPost blogPost)
		{
			if(blogPost.Id == Guid.Empty)
			{
				blogPost.Id = Guid.NewGuid();
			}

			MapExistingTags(blogPost);

			blogPost = _blogPosts.Add(blogPost);
			_database.Save();
			return blogPost;
		}

		public PagedResultSet<BlogPostSummary> BlogPostSearch(BlogPostSearchCriteria criteria)
		{
			var query = from x in _blogPosts.All()
						select new BlogPostSummary
						{
							Id = x.Id,
							IsPage = x.IsPage,
							IsPublished = x.IsPublished,
							PublishedOn = x.PublishedOn,
							Title = x.Title
						};

			if(!string.IsNullOrEmpty(criteria.Title))
			{
				query = from x in query
						where x.Title.Contains(criteria.Title)
						select x;
			}

			if(criteria.IsPage != null)
			{
				query = from x in query
						where x.IsPage == criteria.IsPage.Value
						select x;
			}

			var orderedQuery = from x in query
							   orderby x.IsPublished ascending, x.PublishedOn descending
							   select x;

			var result = new PagedResultSet<BlogPostSummary>();

			result.PageResults = orderedQuery.Skip(criteria.Skip).Take(criteria.Take).ToArray();
			result.TotalRecords = query.Count();
			result.StartRecord = criteria.Skip + 1;
			result.EndRecord = result.StartRecord + result.PageResults.Count() - (result.TotalRecords > 0 ? 1 : 0);

			return result;
		}

		public IEnumerable<BlogPost> GetAllBlogPosts()
		{
			return _blogPosts.All().ToArray();
		}

		public IEnumerable<Tag> GetAllTags()
		{
			return _tags.All().ToArray();
		}

		public IEnumerable<TemplateSection> GetAllTemplateSections()
		{
			return _sections.All().ToArray();
		}

		public BlogPost GetBlogPost(Guid id)
		{
			return _blogPosts.Get(id);
		}

		public TemplateSection GetTemplateSection(int id)
		{
			return _sections.Get(id);
		}

		public IDictionary<int, string> ListTemplateSections()
		{
			return _sections.All().ToDictionary(x => x.Id, x => x.Name);
		}

		public BlogPost UpdateBlogPost(BlogPost blogPost)
		{
			MapExistingTags(blogPost);
			_blogPosts.Update(blogPost);
			_database.Save();
			return blogPost;
		}

		public void UpdateTemplateSection(int id, string content)
		{
			var section = _sections.Get(id);
			section.Content = content;
			_sections.Update(section);
			_database.Save();
		}

		private void MapExistingTags(BlogPost blogPost)
		{
			if (blogPost.Tags != null && blogPost.Tags.Any())
			{
				var tags = new List<Tag>();
				foreach (var tag in blogPost.Tags)
				{
					var existingTag = _tags.Where(x => x.Name == tag.Name).FirstOrDefault();
					if (existingTag != null)
					{
						tags.Add(existingTag);
					}
					else
					{
						tags.Add(tag);
					}
				}
				blogPost.Tags = tags;
			}
		}
	}
}
