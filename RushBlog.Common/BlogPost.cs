using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RushBlog.Common
{
	public class BlogPost
	{
		public BlogPost()
		{
			Tags = new List<Tag>();
		}

		public Guid Id { get; set; }

		[Required]
		[Display(Name = "Title")]
		public string Title { get; set; }

		[Display(Name = "Content")]
		public string MarkdownContent { get; set; }

		[Display(Name = "Preview")]
		public string HtmlContent { get; set; }

		[Display(Name = "Publish")]
		public bool IsPublished { get; set; }

		[Display(Name = "Published On Date")]
		public DateTimeOffset? PublishedOn { get; set; }

		[Required]
		[Display(Name = "Slug")]
		public string Slug { get; set; }

		[Display(Name = "Tags")]
		public List<Tag> Tags { get; set; }

		[Display(Name = "Make a page")]
		public bool IsPage { get; set; }
	}
}
