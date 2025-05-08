using Microsoft.AspNetCore.Mvc;
using MyIdentityApi.Dtos.Common;

namespace MyIdentityApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected IActionResult CreateApiResponse<T>(T data, int statusCode = 200)
    {
        var response = ApiResponse<T>.Success(data, statusCode);
        return StatusCode(statusCode, response);
    }

    protected IActionResult ApiError<T>(List<string> errors, int statusCode = 400)
    {
        var response = ApiResponse<T>.Fail(errors, statusCode);
        return StatusCode(statusCode, response);
    }

    protected IActionResult PaginatedResponse<T>(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        var paginatedData = new PaginatedResponseDto<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            SearchQuery = null 
        };
        
        return CreateApiResponse(paginatedData);
    }
}