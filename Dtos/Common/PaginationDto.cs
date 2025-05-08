namespace MyIdentityApi.Dtos.Common;

public class PaginationDto
{
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : value < 1 ? 10 : value;
    }

    public string? SortBy { get; set; }
    public bool IsDescending { get; set; }
    public string? SearchQuery { get; set; } 
}