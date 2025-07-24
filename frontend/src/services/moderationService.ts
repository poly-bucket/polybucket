import api from '../utils/axiosConfig';

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
  private readonly baseUrl = '/api';

  /**
   * Get all reports with pagination and filtering
   */
  async getAllReports(
    page: number = 1,
    pageSize: number = 20,
    isResolved?: boolean,
    type?: string
  ): Promise<ReportsResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (isResolved !== undefined) {
      params.append('isResolved', isResolved.toString());
    }

    if (type) {
      params.append('type', type);
    }

    const response = await api.get(`${this.baseUrl}/reports?${params}`);
    return response.data;
  }

  /**
   * Get unresolved reports only
   */
  async getUnresolvedReports(): Promise<Report[]> {
    const response = await api.get(`${this.baseUrl}/reports/unresolved`);
    return response.data;
  }

  /**
   * Get comprehensive reports analytics
   */
  async getReportsAnalytics(fromDate?: Date, toDate?: Date): Promise<any> {
    const params = new URLSearchParams();
    if (fromDate) {
      params.append('fromDate', fromDate.toISOString());
    }
    if (toDate) {
      params.append('toDate', toDate.toISOString());
    }

    const response = await api.get(`${this.baseUrl}/reports/analytics?${params}`);
    return response.data;
  }

  /**
   * Get top reported models
   */
  async getTopReportedModels(limit: number = 10): Promise<any[]> {
    const response = await api.get(`${this.baseUrl}/reports/analytics/top-models?limit=${limit}`);
    return response.data;
  }

  /**
   * Get top reported users
   */
  async getTopReportedUsers(limit: number = 10): Promise<any[]> {
    const response = await api.get(`${this.baseUrl}/reports/analytics/top-users?limit=${limit}`);
    return response.data;
  }

  /**
   * Get top reported comments
   */
  async getTopReportedComments(limit: number = 10): Promise<any[]> {
    const response = await api.get(`${this.baseUrl}/reports/analytics/top-comments?limit=${limit}`);
    return response.data;
  }

  /**
   * Get report trends
   */
  async getReportTrends(period: string = 'daily', days: number = 30): Promise<any[]> {
    const response = await api.get(`${this.baseUrl}/reports/analytics/trends?period=${period}&days=${days}`);
    return response.data;
  }

  /**
   * Get moderator activity
   */
  async getModeratorActivity(fromDate?: Date, toDate?: Date): Promise<any[]> {
    const params = new URLSearchParams();
    if (fromDate) {
      params.append('fromDate', fromDate.toISOString());
    }
    if (toDate) {
      params.append('toDate', toDate.toISOString());
    }

    const response = await api.get(`${this.baseUrl}/reports/analytics/moderator-activity?${params}`);
    return response.data;
  }

  /**
   * Get specific report by ID
   */
  async getReportById(reportId: string): Promise<Report> {
    const response = await api.get(`${this.baseUrl}/reports/${reportId}`);
    return response.data;
  }

  /**
   * Resolve a report
   */
  async resolveReport(reportId: string, resolution: string): Promise<void> {
    await api.post(`${this.baseUrl}/reports/${reportId}/resolve`, {
      resolution,
    });
  }

  /**
   * Get all banned users
   */
  async getBannedUsers(
    page: number = 1,
    pageSize: number = 20
  ): Promise<BannedUsersResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    const response = await api.get(`${this.baseUrl}/admin/users/banned?${params}`);
    return response.data;
  }

  /**
   * Ban a user
   */
  async banUser(userId: string, request: BanUserRequest): Promise<void> {
    await api.post(`${this.baseUrl}/admin/users/${userId}/ban`, request);
  }

  /**
   * Unban a user
   */
  async unbanUser(userId: string): Promise<void> {
    await api.post(`${this.baseUrl}/admin/users/${userId}/unban`);
  }

  /**
   * Get moderation audit logs
   */
  async getModerationAuditLogs(
    page: number = 1,
    pageSize: number = 20,
    action?: string,
    userId?: string,
    modelId?: string
  ): Promise<any> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });

    if (action) params.append('action', action);
    if (userId) params.append('userId', userId);
    if (modelId) params.append('modelId', modelId);

    const response = await api.get(`${this.baseUrl}/admin/moderation/audit-logs?${params}`);
    return response.data;
  }
}

export const moderationService = new ModerationService(); 