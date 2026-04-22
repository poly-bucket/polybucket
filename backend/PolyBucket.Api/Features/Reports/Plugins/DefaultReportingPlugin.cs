using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetAllReports.Domain;
using PolyBucket.Api.Features.Reports.GetReport.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Domain;
using PolyBucket.Api.Features.Reports.GetReportsForTarget.Domain;
using PolyBucket.Api.Features.Reports.GetUnresolvedReports.Domain;
using PolyBucket.Api.Features.Reports.ResolveReport.Domain;
using PolyBucket.Api.Features.Reports.SubmitReport.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Reports.Plugins
{
    public class DefaultReportingPlugin(
        ISubmitReportService submitReportService,
        IGetAllReportsService getAllReportsService,
        IGetReportService getReportService,
        IGetReportAnalyticsService getReportAnalyticsService,
        IGetReportsForTargetService getReportsForTargetService,
        IGetUnresolvedReportsService getUnresolvedReportsService,
        IResolveReportService resolveReportService) : IReportingPlugin
    {
        private readonly ISubmitReportService _submitReportService = submitReportService;
        private readonly IGetAllReportsService _getAllReportsService = getAllReportsService;
        private readonly IGetReportService _getReportService = getReportService;
        private readonly IGetReportAnalyticsService _getReportAnalyticsService = getReportAnalyticsService;
        private readonly IGetReportsForTargetService _getReportsForTargetService = getReportsForTargetService;
        private readonly IGetUnresolvedReportsService _getUnresolvedReportsService = getUnresolvedReportsService;
        private readonly IResolveReportService _resolveReportService = resolveReportService;

        public string Id => "default-reporting-plugin";
        public string Name => "Default Reporting Plugin";
        public string Version => "1.0.0";
        public string Author => "PolyBucket Team";
        public string Description => "Default implementation for report management and analytics";
        
        public IEnumerable<PluginComponent> FrontendComponents => new List<PluginComponent>();
        
        public PluginMetadata Metadata => new PluginMetadata
        {
            MinimumAppVersion = "1.0.0",
            RequiredPermissions = new List<string> { "moderation.view.reports" },
            OptionalPermissions = new List<string> { "moderation.handle.reports" },
            Settings = new Dictionary<string, PluginSetting>(),
            Lifecycle = new PluginLifecycle
            {
                AutoStart = true,
                CanDisable = true,
                CanUninstall = false
            }
        };

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task UnloadAsync()
        {
            await Task.CompletedTask;
        }

        public Task<Report> SubmitReportAsync(ReportType type, Guid targetId, Guid reporterId, ReportReason reason, string description)
        {
            return _submitReportService.SubmitReportAsync(type, targetId, reporterId, reason, description, default);
        }

        public Task<IEnumerable<Report>> GetReportsForTargetAsync(ReportType type, Guid targetId)
        {
            return _getReportsForTargetService.GetReportsForTargetAsync(type, targetId, default);
        }

        public Task<IEnumerable<Report>> GetUnresolvedReportsAsync()
        {
            return _getUnresolvedReportsService.GetUnresolvedReportsAsync(default);
        }

        public Task<ReportsResponse> GetAllReportsAsync(int page = 1, int pageSize = 20, bool? isResolved = null, ReportType? type = null)
        {
            return _getAllReportsService.GetAllReportsAsync(page, pageSize, isResolved, type, default);
        }

        public Task ResolveReportAsync(Guid reportId, Guid resolverId, string resolution)
        {
            return _resolveReportService.ResolveReportAsync(reportId, resolverId, resolution, default);
        }

        public Task<Report?> GetReportByIdAsync(Guid reportId)
        {
            return _getReportService.GetReportByIdAsync(reportId, default);
        }

        public Task<ReportsAnalytics> GetReportsAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return _getReportAnalyticsService.GetReportsAnalyticsAsync(fromDate, toDate, default);
        }

        public Task<List<TopReportedItem>> GetTopReportedModelsAsync(int limit = 10)
        {
            return _getReportAnalyticsService.GetTopReportedModelsAsync(limit, default);
        }

        public Task<List<TopReportedItem>> GetTopReportedUsersAsync(int limit = 10)
        {
            return _getReportAnalyticsService.GetTopReportedUsersAsync(limit, default);
        }

        public Task<List<TopReportedItem>> GetTopReportedCommentsAsync(int limit = 10)
        {
            return _getReportAnalyticsService.GetTopReportedCommentsAsync(limit, default);
        }

        public Task<List<ReportTrend>> GetReportTrendsAsync(string period = "daily", int days = 30)
        {
            return _getReportAnalyticsService.GetReportTrendsAsync(period, days, default);
        }

        public Task<List<ModeratorActivity>> GetModeratorActivityAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return _getReportAnalyticsService.GetModeratorActivityAsync(fromDate, toDate, default);
        }
    }
}
