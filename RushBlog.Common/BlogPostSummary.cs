using System;

namespace RushBlog.Common
{
	public class BlogPostSummary
	{
		public Guid Id { get; set; }

		public string Title { get; set; }

		public bool IsPublished { get; set; }

		public DateTimeOffset? PublishedOn { get; set; }

		public bool IsPage { get; set; }
	}
}
