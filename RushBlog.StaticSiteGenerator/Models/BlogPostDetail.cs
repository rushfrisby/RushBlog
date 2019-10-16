namespace RushBlog.StaticSiteGenerator.Models
{
    public class BlogPostDetail : BlogPost
    {
        public string Content { get; set; }

        public MasterModel MasterModel { get; set; }
    }
}
