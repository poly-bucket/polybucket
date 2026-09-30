import { isAxiosError } from "axios";
import axiosInstance from "@/lib/api/axiosConfig";

export type EmailVerificationPurpose = "VerifyAddress" | "ChangeAddress";

export interface VerifyEmailResult {
  message: string;
  purpose: EmailVerificationPurpose;
  email: string;
}

export interface EmailVerificationStatus {
  email: string;
  isEmailVerified: boolean;
  emailVerifiedAt?: string | null;
  pendingEmail?: string | null;
  emailVerificationRequired: boolean;
  emailDeliveryAvailable: boolean;
}

export interface PasswordResetLink {
  path: string;
  url?: string | null;
  expiresAt: string;
}

export const EMAIL_UNVERIFIED_CODE = "email_unverified";

export async function verifyEmail(token: string): Promise<VerifyEmailResult> {
  const { data } = await axiosInstance.post<VerifyEmailResult>("/api/auth/verify-email", { token });
  return data;
}

export async function resendVerificationEmail(email: string): Promise<void> {
  await axiosInstance.post("/api/auth/verify-email/resend", { email });
}

export async function requestPasswordReset(email: string): Promise<void> {
  await axiosInstance.post("/api/auth/forgot-password", { email });
}

export async function resetPassword(
  token: string,
  newPassword: string,
  confirmPassword: string
): Promise<void> {
  await axiosInstance.post("/api/auth/reset-password", { token, newPassword, confirmPassword });
}

export async function requestEmailChange(newEmail: string, currentPassword: string): Promise<void> {
  await axiosInstance.post("/api/auth/email-change", { newEmail, currentPassword });
}

export async function getEmailVerificationStatus(): Promise<EmailVerificationStatus> {
  const { data } = await axiosInstance.get<EmailVerificationStatus>("/api/auth/me");
  return data;
}

export async function markUserEmailVerified(userId: string): Promise<string> {
  const { data } = await axiosInstance.post<{ emailVerifiedAt: string }>(
    `/api/admin/users/${userId}/verify-email`
  );
  return data.emailVerifiedAt;
}

export async function generatePasswordResetLink(userId: string): Promise<PasswordResetLink> {
  const { data } = await axiosInstance.post<PasswordResetLink>(
    `/api/admin/users/${userId}/password-reset-link`
  );
  return data;
}

export function resolvePasswordResetLinkUrl(link: PasswordResetLink, origin: string): string {
  return link.url ?? `${origin.replace(/\/$/, "")}${link.path}`;
}

export function isEmailUnverifiedError(error: unknown): boolean {
  if (!isAxiosError(error) || error.response?.status !== 403) {
    return false;
  }
  const body = error.response.data as { code?: string } | undefined;
  return body?.code === EMAIL_UNVERIFIED_CODE;
}

export function getAccountApiErrorMessage(error: unknown, fallback: string): string {
  if (!isAxiosError(error)) {
    return fallback;
  }

  if (error.response?.status === 429) {
    return "Too many attempts. Wait a minute and try again.";
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

  return body?.message ?? body?.detail ?? body?.title ?? fallback;
}

export function takeTokenFromUrl(): string | null {
  if (typeof window === "undefined") {
    return null;
  }
  const url = new URL(window.location.href);
  const token = url.searchParams.get("token");
  if (!token) {
    return null;
  }
  url.searchParams.delete("token");
  window.history.replaceState(window.history.state, "", `${url.pathname}${url.search}${url.hash}`);
  return token;
}
