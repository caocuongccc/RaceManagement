using RaceManagement.API.ViewModels;
using RaceManagement.Shared.DTOs;

namespace RaceManagement.API.Extensions
{
    public static class ViewModelExtensions
    {
        public static DashboardViewModel ToDashboardViewModel(this DashboardOverviewDto dto)
        {
            return new DashboardViewModel
            {
                Overview = dto,
                PageTitle = "Race Management Dashboard",
                LastRefresh = DateTime.Now,
                ShowCharts = true,
                ShowEmailStats = true,
                ShowRecentActivities = true
            };
        }

        public static RaceAnalyticsViewModel ToRaceAnalyticsViewModel(this RaceAnalyticsDto dto)
        {
            return new RaceAnalyticsViewModel
            {
                Analytics = dto,
                PageTitle = $"Analytics - {dto.RaceName}"
            };
        }

        public static SystemHealthViewModel ToSystemHealthViewModel(this SystemHealthDto dto)
        {
            return new SystemHealthViewModel
            {
                Health = dto,
                LastCheck = DateTime.Now
            };
        }

        public static List<StatsWidgetViewModel> ToStatsWidgets(this DashboardOverviewDto dto)
        {
            return new List<StatsWidgetViewModel>
            {
                new StatsWidgetViewModel
                {
                    Title = "Total Races",
                    Value = dto.TotalRaces.ToString(),
                    SubText = $"{dto.ActiveRaces} active",
                    Icon = "fas fa-running",
                    ColorClass = "primary",
                    Url = "/api/races"
                },
                new StatsWidgetViewModel
                {
                    Title = "Registrations",
                    Value = dto.TotalRegistrations.ToString(),
                    SubText = $"{dto.TodayRegistrations} today",
                    Icon = "fas fa-users",
                    ColorClass = "success",
                    Url = "/api/registrations"
                },
                new StatsWidgetViewModel
                {
                    Title = "Revenue",
                    Value = dto.TotalRevenue.ToString("C0"),
                    SubText = $"{dto.MonthlyRevenue:C0} this month",
                    Icon = "fas fa-dollar-sign",
                    ColorClass = "info",
                    Url = "/Dashboard/Reports"
                },
                new StatsWidgetViewModel
                {
                    Title = "Pending",
                    Value = dto.PendingPayments.ToString(),
                    SubText = $"{dto.BibsToGenerate} need BIBs",
                    Icon = "fas fa-clock",
                    ColorClass = "warning",
                    Url = "/Dashboard/SystemHealth"
                }
            };
        }

        public static ChartDataViewModel ToRegistrationChartData(this List<RegistrationTrendDto> trends)
        {
            return new ChartDataViewModel
            {
                ChartType = "line",
                Labels = trends.Select(t => t.Label).ToList(),
                Datasets = new List<ChartDatasetViewModel>
                {
                    new ChartDatasetViewModel
                    {
                        Label = "Daily Registrations",
                        Data = trends.Select(t => (decimal)t.RegistrationCount).ToList(),
                        BorderColor = "#007bff",
                        BackgroundColor = "rgba(0, 123, 255, 0.1)"
                    }
                }
            };
        }

        public static ChartDataViewModel ToRevenueChartData(this List<RevenueTrendDto> trends)
        {
            return new ChartDataViewModel
            {
                ChartType = "bar",
                Labels = trends.Select(t => t.Label).ToList(),
                Datasets = new List<ChartDatasetViewModel>
                {
                    new ChartDatasetViewModel
                    {
                        Label = "Daily Revenue",
                        Data = trends.Select(t => t.Revenue).ToList(),
                        BorderColor = "#28a745",
                        BackgroundColor = "rgba(40, 167, 69, 0.1)"
                    }
                }
            };
        }

        // FIX: Add extension for RaceAnalyticsDto collections
        public static List<RaceAnalyticsViewModel> ToViewModels(this List<RaceAnalyticsDto> dtos)
        {
            return dtos.Select(dto => dto.ToRaceAnalyticsViewModel()).ToList();
        }
        // Fix for Reports page
        public static string GetTrendDirectionIcon(this RaceAnalyticsDto analytics)
        {
            return analytics.TrendDirection switch
            {
                "up" => "fas fa-arrow-up text-success",
                "down" => "fas fa-arrow-down text-danger",
                _ => "fas fa-minus text-muted"
            };
        }
    }
}
