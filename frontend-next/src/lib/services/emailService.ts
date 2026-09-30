import { isAxiosError } from "axios";
import axiosInstance from "@/lib/api/axiosConfig";

export type EmailTransportKind = "Disabled" | "Smtp" | "Log";
export type EmailSecurityMode = "None" | "StartTls" | "SslOnConnect" | "Auto";
export type EmailSettingSource = "Default" | "Database" | "Environment";
export type EmailDiagnosticStage =
  | "Configuration"
  | "Dns"
  | "Connect"
  | "Tls"
  | "Auth"
  | "Send";
export type EmailMessageStatus =
  | "Pending"
  | "Sending"
  | "Sent"
  | "Failed"
  | "DeadLetter";
export type EmailTemplateKey =
  | "Test"
  | "VerifyEmail"
  | "PasswordReset"
  | "Welcome"
  | "AdminCreatedAccount"
  | "PasswordChanged"
  | "TwoFactorChanged"
  | "EmailChangeRequested"
  | "EmailChangedNotice"
  | "Notification";

export type EmailSettingField =
  | "transport"
  | "smtpHost"
  | "smtpPort"
  | "smtpSecurity"
  | "smtpUsername"
  | "smtpPassword"
  | "allowInvalidCertificates"
  | "fromAddress"
  | "fromName"
  | "replyTo"
  | "publicBaseUrl"
  | "requireEmailVerification";

export interface EmailSettings {
  enabled: boolean;
  transport: EmailTransportKind;
  smtpHost: string;
  smtpPort: number;
  smtpSecurity: EmailSecurityMode;
  smtpUsername: string;
  hasPassword: boolean;
  allowInvalidCertificates: boolean;
  fromAddress: string;
  fromName: string;
  replyTo?: string | null;
  publicBaseUrl: string;
  requireEmailVerification: boolean;
  isConfigured: boolean;
  lastSuccessfulTestAt?: string | null;
  validationErrors: string[];
  sources: Partial<Record<EmailSettingField, EmailSettingSource>>;
  managedByEnvironment: EmailSettingField[];
}

export interface UpdateEmailSettingsRequest {
  transport: EmailTransportKind;
  smtpHost: string;
  smtpPort: number;
  smtpSecurity: EmailSecurityMode;
  smtpUsername: string;
  smtpPassword?: string;
  clearPassword: boolean;
  allowInvalidCertificates: boolean;
  fromAddress: string;
  fromName: string;
  replyTo?: string;
  publicBaseUrl: string;
  requireEmailVerification: boolean;
}

export interface EmailDiagnosticStageResult {
  stage: EmailDiagnosticStage;
  success: boolean;
  message: string;
  elapsedMilliseconds: number;
}

export interface EmailTestResult {
  success: boolean;
  message: string;
  stages: EmailDiagnosticStageResult[];
}

export interface EmailOutboxItem {
  id: string;
  template: EmailTemplateKey;
  recipient: string;
  status: EmailMessageStatus;
  attempts: number;
  nextAttemptAt: string;
  lastError?: string | null;
  createdAt: string;
  sentAt?: string | null;
}

export interface EmailOutboxPage {
  items: EmailOutboxItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  statusCounts: Partial<Record<EmailMessageStatus, number>>;
}

export interface RenderedEmail {
  subject: string;
  htmlBody: string;
  textBody: string;
}

export async function getEmailSettings(): Promise<EmailSettings> {
  const { data } = await axiosInstance.get<EmailSettings>("/api/system-settings/email");
  return data;
}

export async function updateEmailSettings(
  request: UpdateEmailSettingsRequest
): Promise<EmailSettings> {
  const { data } = await axiosInstance.put<EmailSettings>(
    "/api/system-settings/email",
    request
  );
  return data;
}

export async function testEmailConfiguration(
  testEmailAddress: string
): Promise<EmailTestResult> {
  const { data } = await axiosInstance.post<EmailTestResult>(
    "/api/system-settings/email/test",
    { testEmailAddress }
  );
  return data;
}

export async function getEmailOutbox(
  status?: EmailMessageStatus,
  page = 1,
  pageSize = 25
): Promise<EmailOutboxPage> {
  const { data } = await axiosInstance.get<EmailOutboxPage>("/api/admin/email/outbox", {
    params: { status, page, pageSize },
  });
  return data;
}

export async function retryEmailMessage(id: string): Promise<void> {
  await axiosInstance.post(`/api/admin/email/outbox/${id}/retry`);
}

export async function previewEmailTemplate(
  key: EmailTemplateKey
): Promise<RenderedEmail> {
  const { data } = await axiosInstance.get<RenderedEmail>(
    `/api/admin/email/templates/${key}/preview`
  );
  return data;
}

export function getEmailApiErrorMessage(error: unknown, fallback: string): string {
  if (!isAxiosError(error)) {
    return fallback;
  }

  const body = error.response?.data as
    | { detail?: string; title?: string; errors?: Record<string, string[]>; message?: string }
    | undefined;
  if (body?.errors) {
    const messages = Object.values(body.errors).flat().filter(Boolean);
    if (messages.length > 0) {
      return messages.join(" ");
    }
  }

  if (error.response?.status === 429) {
    return "Too many attempts. Wait a minute and try again.";
  }

  return body?.detail ?? body?.message ?? body?.title ?? fallback;
}
