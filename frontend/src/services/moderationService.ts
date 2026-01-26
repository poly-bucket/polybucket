import { ApiClientFactory } from '../api/clientFactory';
import {
  ResolveReportRequest,
  BanUserRequest,
  FileResponse
} from '../api/client';

export interface Report {
  id: string;
  type: 'Model' | 'Comment' | 'User' | 'Collection';
  targetId: string;
  reporterId: string;
  reason: 'Inappropriate' | 'Spam' | 'Copyright' | 'Malware' | 'Other';
  description: string;
  createdAt: string;
  isResolved: boolean;
  resolution?: string;
  resolvedAt?: string;
  resolvedById?: string;
}

export interface ReportsResponse {
  reports: Report[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface BannedUser {
  id: string;
  username: string;
  email: string;
  bannedAt?: string;
  bannedByUsername?: string;
  banReason?: string;
  banExpiresAt?: string;
  roleName: string;
}

export interface BannedUsersResponse {
  users: BannedUser[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface BanUserInput {
  reason: string;
  expiresAt?: string;
}

async function parseFileResponseAsJson<T>(r: FileResponse): Promise<T> {
  if ((r as any).data instanceof Blob) {
    const text = await (r as any).data.text();
    return JSON.parse(text || 'null');
  }
  return r as any;
}

const api = () => ApiClientFactory.getApiClient();

export class ModerationService {
  async getAllReports(
    page: number = 1,
    pageSize: number = 20,
    isResolved?: boolean,
    type?: string
  ): Promise<ReportsResponse> {
    const response = await api().getAllReports_GetAllReports(page, pageSize, isResolved ?? null, type || null);
    return response as any as ReportsResponse;
  }

  async getUnresolvedReports(): Promise<Report[]> {
    const response = await api().getUnresolvedReports_GetUnresolvedReports();
    const parsed = await parseFileResponseAsJson<{ reports?: Report[] } | Report[]>(response);
    if (Array.isArray(parsed)) return parsed;
    return parsed?.reports ?? [];
  }

  async getReportsAnalytics(fromDate?: Date, toDate?: Date): Promise<any> {
    return api().getReportsAnalytics_GetReportsAnalytics(fromDate ?? null, toDate ?? null);
  }

  async getTopReportedModels(limit: number = 10): Promise<any[]> {
    return api().getReportsAnalytics_GetTopReportedModels(limit);
  }

  async getTopReportedUsers(limit: number = 10): Promise<any[]> {
    return api().getReportsAnalytics_GetTopReportedUsers(limit);
  }

  async getTopReportedComments(limit: number = 10): Promise<any[]> {
    return api().getReportsAnalytics_GetTopReportedComments(limit);
  }

  async getReportTrends(period: string = 'daily', days: number = 30): Promise<any[]> {
    return api().getReportsAnalytics_GetReportTrends(period, days);
  }

  async getModeratorActivity(fromDate?: Date, toDate?: Date): Promise<any[]> {
    return api().getReportsAnalytics_GetModeratorActivity(fromDate ?? null, toDate ?? null);
  }

  async getReportById(reportId: string): Promise<Report> {
    const response = await api().getReport_GetReport(reportId);
    const parsed = await parseFileResponseAsJson<Report>(response);
    return parsed as Report;
  }

  async resolveReport(reportId: string, resolution: string): Promise<void> {
    const request = new ResolveReportRequest({ resolution });
    await api().resolveReport_ResolveReport(reportId, request);
  }

  async getBannedUsers(page: number = 1, pageSize: number = 20): Promise<BannedUsersResponse> {
    return api().banUser_GetBannedUsers(page, pageSize);
  }

  async banUser(userId: string, banRequest: BanUserInput): Promise<void> {
    const request = new BanUserRequest({
      reason: banRequest.reason,
      expiresAt: banRequest.expiresAt
    });
    await api().banUser_BanUser(userId, request);
  }

  async unbanUser(userId: string): Promise<void> {
    await api().banUser_UnbanUser(userId);
  }

  async getModerationAuditLogs(
    page: number = 1,
    pageSize: number = 20,
    action?: string,
    userId?: string,
    modelId?: string
  ): Promise<any> {
    return api().getModerationAuditLogs_GetAuditLogs(page, pageSize, action ?? null, userId ?? null, modelId ?? null);
  }
}

export const moderationService = new ModerationService();
