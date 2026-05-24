namespace Client.Models.Admin
{
    public class DashboardViewModel
    {
        public DashboardFilter Filter { get; set; } = new();

        public UserStats UserStats { get; set; } = new();

        public JobStats JobStats { get; set; } = new();

        public ApplicationStats ApplicationStats { get; set; } = new();

        public RevenueStats RevenueStats { get; set; } = new();

        public ReportStats ReportStats { get; set; } = new();

        public AnalyticsStats AnalyticsStats { get; set; } = new();

        public List<UserGrowthChartItem> UserGrowthChart { get; set; } = [];

        public List<JobGrowthChartItem> JobGrowthChart { get; set; } = [];

        public List<RecentUserItem> RecentUsers { get; set; } = [];

        public List<RecentJobItem> RecentJobs { get; set; } = [];
    }

    #region Filters

    public class DashboardFilter
    {
        public DashboardRangeType RangeType { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }

    public enum DashboardRangeType
    {
        Today,
        ThisWeek,
        ThisMonth,
        ThisYear,
        Custom
    }

    #endregion

    #region User Stats

    public class UserStats
    {
        public int TotalUsers { get; set; }

        public int TotalNewUsers { get; set; }

        public int TotalFreelancers { get; set; }

        public int TotalNewFreelancers { get; set; }

        public int TotalEmployers { get; set; }

        public int TotalNewEmployers { get; set; }

        public int ActiveUsers { get; set; }

        public int BannedUsers { get; set; }
    }

    #endregion

    #region Job Stats

    public class JobStats
    {
        public int TotalJobs { get; set; }

        public int ActiveJobs { get; set; }

        public int ClosedJobs { get; set; }

        public int DraftJobs { get; set; }

        public int ExpiredJobs { get; set; }

        public int NewJobs { get; set; }
    }

    #endregion

    #region Application Stats

    public class ApplicationStats
    {
        public int TotalApplications { get; set; }

        public int TotalMatches { get; set; }

        public int PendingApplications { get; set; }

        public int AcceptedApplications { get; set; }

        public int RejectedApplications { get; set; }

        public double MatchRate { get; set; }
    }

    #endregion

    #region Revenue Stats

    public class RevenueStats
    {
        public decimal TotalRevenue { get; set; }

        public decimal MonthlyRevenue { get; set; }

        public decimal TodayRevenue { get; set; }

        public decimal AverageRevenuePerEmployer { get; set; }
    }

    #endregion

    #region Report Stats

    public class ReportStats
    {
        public int PendingReports { get; set; }

        public int ResolvedReports { get; set; }

        public int RejectedReports { get; set; }
    }

    #endregion

    #region Analytics

    public class AnalyticsStats
    {
        public int PageVisits { get; set; }

        public int UniqueVisitors { get; set; }

        public int ActiveVisitors { get; set; }

        public double BounceRate { get; set; }
    }

    #endregion

    #region Charts

    public class UserGrowthChartItem
    {
        public string Label { get; set; } = string.Empty;

        public int Freelancers { get; set; }

        public int Employers { get; set; }
    }

    public class JobGrowthChartItem
    {
        public string Label { get; set; } = string.Empty;

        public int Jobs { get; set; }

        public int Applications { get; set; }
    }

    #endregion

    #region Recent Activities

    public class RecentUserItem
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class RecentJobItem
    {
        public Guid JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string EmployerName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    #endregion
}