namespace BPN.Payment.API.Utils.Constants
{
    public class PaginatedResult<T>
    {
        public List<T> Items { get; }
        public int TotalItems { get; }
        public int Page { get; }
        public int Size { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / Size);

        public PaginatedResult(List<T> items, int totalItems, int page, int size)
        {
            Items = items;
            TotalItems = totalItems;
            Page = page;
            Size = size;
        }
    }

}
