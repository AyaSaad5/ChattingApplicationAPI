namespace ChattingApplication.Helpers
{
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int totalPages, int totalItems, int itemsPerPage)
        {
            CurrentPage = currentPage;
            TotalPages = totalPages;
            TotalItems = totalItems;
            ItemsPerPage = itemsPerPage;
        }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
