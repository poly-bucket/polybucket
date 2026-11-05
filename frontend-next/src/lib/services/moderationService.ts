import { ApiClientFactory } from "@/lib/api/clientFactory";
import {
  ResolveReportRequest,
  type ReportsResponse,
  type ReportsAnalyticsResponseDto,
  type BannedUsersListResponse,
  type ModerationAuditResponseDto,
  type ReportType,
  type TopReportedItemDto,
  type ModeratorActivityItemDto,
} from "@/lib/api/client";

const client = () => ApiClientFactory.getApiClient();

export async function getAllReports(
  page?: number,
  pageSize?: number,
  isResolved?: boolean | null,
  type?: ReportType | string | null
): Promise<ReportsResponse> {
  return client().getAllReports_GetAllReports(
    page,
    pageSize,
    isResolved ?? null,
    (type as ReportType) ?? null
  );
}

export async function getReportsAnalytics(
  fromDate?: Date | null,
  toDate?: Date | null
): Promise<ReportsAnalyticsResponseDto> {
  return client().getReportsAnalytics_GetReportsAnalytics(
    fromDate ?? null,
    toDate ?? null
  );
}

export async function getTopReportedModels(
  limit?: number
): Promise<TopReportedItemDto[]> {
  const response = await client().getTopReportedModels_GetTopReportedModels(
    limit ?? 10
  );
  return (response.items ?? []) as TopReportedItemDto[];
}

export async function getTopReportedUsers(
  limit?: number
): Promise<TopReportedItemDto[]> {
  const response = await client().getTopReportedUsers_GetTopReportedUsers(
    limit ?? 10
  );
  return (response.items ?? []) as TopReportedItemDto[];
}

export async function getTopReportedComments(
  limit?: number
): Promise<TopReportedItemDto[]> {
  const response = await client().getTopReportedComments_GetTopReportedComments(
    limit ?? 10
  );
  return (response.items ?? []) as TopReportedItemDto[];
}

export async function getModeratorActivity(
  fromDate?: Date | null,
  toDate?: Date | null
): Promise<ModeratorActivityItemDto[]> {
  const response = await client().getModeratorReportActivity_GetModeratorActivity(
    fromDate ?? null,
    toDate ?? null
  );
  if (Array.isArray(response)) {
    return response as ModeratorActivityItemDto[];
  }
  return (response as { items?: ModeratorActivityItemDto[] })?.items ?? [];
}

export async function resolveReport(
  reportId: string,
  resolution: string
): Promise<void> {
  const request = new ResolveReportRequest({ resolution });
  await client().resolveReport_ResolveReport(reportId, request);
}

export async function getBannedUsers(
  page?: number,
  pageSize?: number
): Promise<BannedUsersListResponse> {
  return client().getBannedUsers_GetBannedUsers(page ?? 1, pageSize ?? 20);
}

export async function unbanUser(userId: string): Promise<void> {
  await client().unbanUser_UnbanUser(userId);
}

export async function getModerationAuditLogs(
  page?: number,
  pageSize?: number,
  action?: string | null,
  userId?: string | null,
  modelId?: string | null
): Promise<ModerationAuditResponseDto> {
  return client().getModerationAuditLogs_GetAuditLogs(
    page ?? 1,
    pageSize ?? 20,
    action ?? null,
    userId ?? null,
    modelId ?? null
  );
}
