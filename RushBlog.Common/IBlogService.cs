using System;
using System.Collections.Generic;

namespace RushBlog.Common
{
	public interface IBlogService
	{
		PagedResultSet<BlogPostSummary> BlogPostSearch(BlogPostSearchCriteria criteria);

		BlogPost GetBlogPost(Guid id);

		BlogPost AddBlogPost(BlogPost blogPost);

		BlogPost UpdateBlogPost(BlogPost blogPost);

		IEnumerable<Tag> GetAllTags();

		IEnumerable<BlogPost> GetAllBlogPosts();

		IDictionary<int, string> ListTemplateSections();

		IEnumerable<TemplateSection> GetAllTemplateSections();

		void UpdateTemplateSection(int id, string content);

		TemplateSection GetTemplateSection(int id);
	}
}
