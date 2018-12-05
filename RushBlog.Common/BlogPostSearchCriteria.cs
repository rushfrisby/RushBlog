namespace RushBlog.Common
{
	public class BlogPostSearchCriteria
	{
		public string Title { get; set; }

		public bool? IsPage { get; set; }

		public int Skip { get; set; }

		public int Take { get; set; }
	}
}
