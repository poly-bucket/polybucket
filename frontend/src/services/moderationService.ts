import { API_CONFIG } from '../api/config';
import { AxiosHttpClient } from '../api/axiosAdapter';
import {
  GetAllReportsClient,
  GetUnresolvedReportsClient,
  GetReportsAnalyticsClient,
  GetReportClient,
  ResolveReportClient,
  BanUserClient,
  GetModerationAuditLogsClient,
  ResolveReportRequest,
  BanUserRequest
} from './api.client';
import api from '../utils/axiosConfig';

const sharedHttpClient = new AxiosHttpClient(API_CONFIG.baseUrl);

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

export interface BanUserRequest {
  reason: string;
  expiresAt?: string;
}

export class ModerationService {
  async getAllReports(
    page: number = 1,
    pageSize: number = 20,
    isResolved?: boolean,
    type?: string
  ): Promise<ReportsResponse> {
    const client = new GetAllReportsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getAllReports(
      page,
      pageSize,
      isResolved,
      type || null
    );
    return response as any as ReportsResponse;
  }

  async getUnresolvedReports(): Promise<Report[]> {
    const client = new GetUnresolvedReportsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getUnresolvedReports();
    return response as any as Report[];
  }

  async getReportsAnalytics(fromDate?: Date, toDate?: Date): Promise<any> {
    const client = new GetReportsAnalyticsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getReportsAnalytics(
      fromDate?.toISOString() || null,
      toDate?.toISOString() || null
    );
    return response;
  }

  // SKIPPED: /api/reports/analytics/top-models endpoint not found in generated client
  async getTopReportedModels(limit: number = 10): Promise<any[]> {
    const response = await api.get(`/api/reports/analytics/top-models?limit=${limit}`);
    return response.data;
  }

  // SKIPPED: /api/reports/analytics/top-users endpoint not found in generated client
  async getTopReportedUsers(limit: number = 10): Promise<any[]> {
    const response = await api.get(`/api/reports/analytics/top-users?limit=${limit}`);
    return response.data;
  }

  // SKIPPED: /api/reports/analytics/top-comments endpoint not found in generated client
  async getTopReportedComments(limit: number = 10): Promise<any[]> {
    const response = await api.get(`/api/reports/analytics/top-comments?limit=${limit}`);
    return response.data;
  }

  // SKIPPED: /api/reports/analytics/trends endpoint not found in generated client
  async getReportTrends(period: string = 'daily', days: number = 30): Promise<any[]> {
    const response = await api.get(`/api/reports/analytics/trends?period=${period}&days=${days}`);
    return response.data;
  }

  // SKIPPED: /api/reports/analytics/moderator-activity endpoint not found in generated client
  async getModeratorActivity(fromDate?: Date, toDate?: Date): Promise<any[]> {
    const params = new URLSearchParams();
    if (fromDate) {
      params.append('fromDate', fromDate.toISOString());
    }
    if (toDate) {
      params.append('toDate', toDate.toISOString());
    }

    const response = await api.get(`/api/reports/analytics/moderator-activity?${params}`);
    return response.data;
  }

  async getReportById(reportId: string): Promise<Report> {
    const client = new GetReportClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getReport(reportId);
    return response as any as Report;
  }

  async resolveReport(reportId: string, resolution: string): Promise<void> {
    const client = new ResolveReportClient(API_CONFIG.baseUrl, sharedHttpClient);
    const request = new ResolveReportRequest({
      resolution: resolution
    });
    await client.resolveReport(reportId, request);
  }

  // SKIPPED: /api/admin/users/banned endpoint not found in generated client
  async getBannedUsers(
    page: number = 1,
    pageSize: number = 20
  ): Promise<BannedUsersResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    const response = await api.get(`/api/admin/users/banned?${params}`);
    return response.data;
  }

  async banUser(userId: string, banRequest: BanUserRequest): Promise<void> {
    const client = new BanUserClient(API_CONFIG.baseUrl, sharedHttpClient);
    const request = new BanUserRequest({
      reason: banRequest.reason,
      expiresAt: banRequest.expiresAt
    });
    await client.banUser(userId, request);
  }

  // SKIPPED: /api/admin/users/{userId}/unban endpoint not found in generated client
  async unbanUser(userId: string): Promise<void> {
    await api.post(`/api/admin/users/${userId}/unban`);
  }

  async getModerationAuditLogs(
    page: number = 1,
    pageSize: number = 20,
    action?: string,
    userId?: string,
    modelId?: string
  ): Promise<any> {
    const client = new GetModerationAuditLogsClient(API_CONFIG.baseUrl, sharedHttpClient);
    const response = await client.getModerationAuditLogs(
      page,
      pageSize,
      action || null,
      userId || null,
      modelId || null
    );
    return response;
  }
}

export const moderationService = new ModerationService();
