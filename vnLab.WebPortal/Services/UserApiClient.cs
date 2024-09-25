using vnLab.ViewModel;
using vnLab.ViewModel.Systems;

namespace vnLab.WebPortal.Services;

public interface IUserApiClient
{
    Task<UserViewModel> GetById(string id);
    Task<Pagination<UserViewModel>> GetPagination(string? keyword);
}


public class UserApiClient : BaseApiClient, IUserApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserApiClient(IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
        : base(httpClientFactory, configuration, httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserViewModel> GetById(string id)
    {
        return await GetAsync<UserViewModel>($"/api/users/{id}", true);
    }

    public async Task<Pagination<UserViewModel>> GetPagination(string? keyword)
    {
        var apiUrl = $"/api/users/filter?keyword={keyword}&page=1&pageSize=10";
        return await GetAsync<Pagination<UserViewModel>>(apiUrl, true);
    }
}