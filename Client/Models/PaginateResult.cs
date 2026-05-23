namespace Client.Models
{
    public class PaginateResult
    {
        public int PageIndex { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    public class PaginateResult<T> : PaginateResult
    {
        public List<T> Items { get; set; } = new List<T>();
    }
}
