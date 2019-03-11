namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        private const int maxPageSize = 20;
        private int _pageSize = 10;

        public string Fields { get; set; }
        public string Genre { get; set; }

        public string OrderBy { get; set; } = "Name";

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > maxPageSize) ? maxPageSize : value;
            }
        }

        public string SearchQuery { get; set; }
    }
}