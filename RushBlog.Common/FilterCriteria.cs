namespace RushBlog.Common
{
    public class FilterCriteria
    {
        public FilterCriteria()
        {
            Skip = 0;
            Take = 20;
        }

        public int Skip { get; set; }

        public int Take { get; set; }
    }
}
