
namespace SoftBridge.Shared.Common.Params
{
    public class BaseQueryParams
    {
        private const int MaxPageSize = 20;
        private int _pageSize = 6;

        private int _pageIndex = 1;
        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = (value < 1) ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        private string? _search;
        public string? Search
        {
            get => _search;
            set => _search = value?.Trim().ToLower();
        }

        
    }
}
