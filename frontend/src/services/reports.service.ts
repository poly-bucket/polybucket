import api from './api';

export enum ReportType {
  Model = 0,
  Comment = 1,
}

export enum ReportReason {
  Inappropriate = 0,
  Spam = 1,
  Copyright = 2,
  Other = 3,
}

export interface Report {
  id: string;
  type: ReportType;
  targetId: string;
  reporterId: string;
  reason: ReportReason;
  description: string;
  createdAt: string;
  isResolved: boolean;
  resolution?: string;
  resolvedAt?: string;
  resolvedById?: string;
}

interface SubmitReportRequest {
  type: ReportType;
  targetId: string;
  reason: ReportReason;
  description: string;
}

interface ResolveReportRequest {
  resolution: string;
}

class ReportsService {
  async submitReport(request: SubmitReportRequest): Promise<Report> {
    const response = await api.post<Report>('/reports', request);
    return response.data;
  }

  async getReportsForTarget(type: ReportType, targetId: string): Promise<Report[]> {
    const response = await api.get<Report[]>(`/reports/target/${targetId}?type=${type}`);
    return response.data;
  }

  async getUnresolvedReports(): Promise<Report[]> {
    const response = await api.get<Report[]>('/reports/unresolved');
    return response.data;
  }

  async resolveReport(reportId: string, resolution: string): Promise<void> {
    await api.post(`/reports/${reportId}/resolve`, { resolution });
  }

  async getReport(reportId: string): Promise<Report> {
    const response = await api.get<Report>(`/reports/${reportId}`);
    return response.data;
  }
}

export const reportsService = new ReportsService(); 