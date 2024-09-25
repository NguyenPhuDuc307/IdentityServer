namespace vnLab.ViewModel
{
    public class PaginationBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int PageCount
        {
            get
            {
                // Tính số trang cần thiết dựa trên số lượng bản ghi và kích thước trang
                return (int)Math.Ceiling((double)TotalRecords / PageSize);
            }
        }
    }
}