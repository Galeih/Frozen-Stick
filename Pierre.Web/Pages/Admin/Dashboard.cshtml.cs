using Microsoft.AspNetCore.Mvc.RazorPages;
using Pierre.Web.Application.DTOs;
using Pierre.Web.Application.Services;

namespace Pierre.Web.Pages.Admin;

public class DashboardModel : PageModel
{
    private readonly DashboardService _dashboardService;

    public DashboardDto Dashboard { get; set; } = new();

    public DashboardModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task OnGetAsync()
    {
        Dashboard = await _dashboardService.GetDashboardAsync();
    }
}
