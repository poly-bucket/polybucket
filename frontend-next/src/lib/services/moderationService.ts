import { ApiClientFactory } from "@/lib/api/clientFactory";
import {
  ResolveReportRequest,
  type ReportsResponse,
  type ReportsAnalytics,
  type BannedUsersListResponse,
  type ModerationAuditResponse,
  type ReportType,
  type TopReportedItem,
  type ModeratorActivity,
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
): Promise<ReportsAnalytics> {
  return client().getReportAnalytics_GetReportsAnalytics(
    fromDate ?? null,
    toDate ?? null
  );
}

export async function getTopReportedModels(
  limit?: number
): Promise<TopReportedItem[]> {
  return client().getReportAnalytics_GetTopReportedModels(limit ?? 10);
}

export async function getTopReportedUsers(
  limit?: number
): Promise<TopReportedItem[]> {
  return client().getReportAnalytics_GetTopReportedUsers(limit ?? 10);
}

export async function getTopReportedComments(
  limit?: number
): Promise<TopReportedItem[]> {
  return client().getReportAnalytics_GetTopReportedComments(limit ?? 10);
}

export async function getModeratorActivity(
  fromDate?: Date | null,
  toDate?: Date | null
): Promise<ModeratorActivity[]> {
  return client().getReportAnalytics_GetModeratorActivity(
    fromDate ?? null,
    toDate ?? null
  );
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
): Promise<ModerationAuditResponse> {
  return client().getModerationAuditLogs_GetAuditLogs(
    page ?? 1,
    pageSize ?? 20,
    action ?? null,
    userId ?? null,
    modelId ?? null
  );
}
