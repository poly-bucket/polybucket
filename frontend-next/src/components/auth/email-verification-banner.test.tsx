import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import type { AuthUser } from "@/lib/auth/authSession";
import type { EmailVerificationStatus } from "@/lib/services/accountEmailService";
import { DISMISS_STORAGE_KEY, EmailVerificationBanner } from "./email-verification-banner";

const mockGetStatus = vi.fn();
const mockResend = vi.fn();

vi.mock("@/lib/services/accountEmailService", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/lib/services/accountEmailService")>()),
  getEmailVerificationStatus: () => mockGetStatus(),
  resendVerificationEmail: (email: string) => mockResend(email),
}));

const unverifiedUser: AuthUser = {
  id: "user-1",
  email: "maker@example.com",
  username: "maker",
  accessToken: "access-token",
  isEmailVerified: false,
};

function status(overrides: Partial<EmailVerificationStatus> = {}): EmailVerificationStatus {
  return {
    email: "maker@example.com",
    isEmailVerified: false,
    emailVerifiedAt: null,
    pendingEmail: null,
    emailVerificationRequired: true,
    emailDeliveryAvailable: true,
    ...overrides,
  };
}

describe("EmailVerificationBanner", () => {
  beforeEach(() => {
    window.sessionStorage.clear();
    mockGetStatus.mockReset().mockResolvedValue(status());
    mockResend.mockReset().mockResolvedValue(undefined);
  });

  it("shows the required message for an unverified user", async () => {
    // Arrange
    const user = unverifiedUser;

    // Act
    render(<EmailVerificationBanner />, { mockAuth: { user } });

    // Assert
    const banner = await screen.findByRole("region", { name: /email verification/i });
    expect(banner).toHaveTextContent(/verify maker@example.com to upload models/i);
    expect(screen.queryByRole("button", { name: /dismiss/i })).not.toBeInTheDocument();
  });

  it("does not render or fetch for a user already known to be verified", async () => {
    // Arrange
    const user = { ...unverifiedUser, isEmailVerified: true };

    // Act
    render(<EmailVerificationBanner />, { mockAuth: { user } });

    // Assert
    await waitFor(() => expect(mockGetStatus).not.toHaveBeenCalled());
    expect(screen.queryByRole("region", { name: /email verification/i })).not.toBeInTheDocument();
  });

  it("does not render when email delivery is unavailable", async () => {
    // Arrange
    mockGetStatus.mockResolvedValue(status({ emailDeliveryAvailable: false }));

    // Act
    render(<EmailVerificationBanner />, { mockAuth: { user: unverifiedUser } });

    // Assert
    await waitFor(() => expect(mockGetStatus).toHaveBeenCalled());
    expect(screen.queryByRole("region", { name: /email verification/i })).not.toBeInTheDocument();
  });

  it("resends the verification email and shows confirmation", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<EmailVerificationBanner />, { mockAuth: { user: unverifiedUser } });

    // Act
    await user.click(await screen.findByRole("button", { name: /resend email/i }));

    // Assert
    expect(mockResend).toHaveBeenCalledWith("maker@example.com");
    expect(await screen.findByRole("button", { name: /^sent$/i })).toBeDisabled();
    expect(screen.getByRole("region", { name: /email verification/i })).toHaveTextContent(/check your inbox/i);
  });

  it("can be dismissed for the session when verification is optional", async () => {
    // Arrange
    mockGetStatus.mockResolvedValue(status({ emailVerificationRequired: false }));
    const user = userEvent.setup();
    render(<EmailVerificationBanner />, { mockAuth: { user: unverifiedUser } });

    // Act
    await user.click(await screen.findByRole("button", { name: /dismiss/i }));

    // Assert
    expect(screen.queryByRole("region", { name: /email verification/i })).not.toBeInTheDocument();
    expect(window.sessionStorage.getItem(DISMISS_STORAGE_KEY)).toBe("1");
  });

  it("ignores a previous dismissal when verification is required", async () => {
    // Arrange
    window.sessionStorage.setItem(DISMISS_STORAGE_KEY, "1");

    // Act
    render(<EmailVerificationBanner />, { mockAuth: { user: unverifiedUser } });

    // Assert
    expect(await screen.findByRole("region", { name: /email verification/i })).toBeInTheDocument();
  });
});
