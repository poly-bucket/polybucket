import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import type { AuthUser } from "@/lib/auth/authSession";
import VerifyEmailPage from "./page";

const mockSearchParamsGet = vi.fn((_key: string): string | null => null);
const mockVerifyEmail = vi.fn();
const mockResend = vi.fn();
const mockTakeToken = vi.fn((): string | null => null);
const mockRefreshPromise = vi.fn().mockResolvedValue(true);

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  useSearchParams: () => ({ get: mockSearchParamsGet }),
}));

vi.mock("@/lib/auth/authSession", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/lib/auth/authSession")>()),
  getOrCreateRefreshPromise: () => mockRefreshPromise(),
}));

vi.mock("@/lib/services/accountEmailService", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/lib/services/accountEmailService")>()),
  verifyEmail: (token: string) => mockVerifyEmail(token),
  resendVerificationEmail: (email: string) => mockResend(email),
  takeTokenFromUrl: () => mockTakeToken(),
}));

const signedInUser: AuthUser = {
  id: "user-1",
  email: "maker@example.com",
  username: "maker",
  accessToken: "access-token",
  isEmailVerified: false,
};

describe("VerifyEmailPage", () => {
  beforeEach(() => {
    mockSearchParamsGet.mockReset().mockReturnValue(null);
    mockVerifyEmail.mockReset();
    mockResend.mockReset().mockResolvedValue(undefined);
    mockTakeToken.mockReset().mockReturnValue(null);
    mockRefreshPromise.mockClear();
  });

  it("does not verify until the user clicks confirm", async () => {
    // Arrange
    mockTakeToken.mockReturnValue("raw-token");

    // Act
    render(<VerifyEmailPage />, { mockAuth: { user: null } });

    // Assert
    expect(await screen.findByRole("button", { name: /verify email/i })).toBeInTheDocument();
    expect(mockVerifyEmail).not.toHaveBeenCalled();
  });

  it("verifies the token on click and shows the verified address", async () => {
    // Arrange
    mockTakeToken.mockReturnValue("raw-token");
    mockVerifyEmail.mockResolvedValue({ message: "ok", purpose: "VerifyAddress", email: "maker@example.com" });
    const user = userEvent.setup();
    render(<VerifyEmailPage />, { mockAuth: { user: null } });

    // Act
    await user.click(await screen.findByRole("button", { name: /verify email/i }));

    // Assert
    expect(mockVerifyEmail).toHaveBeenCalledWith("raw-token");
    expect(await screen.findByText(/maker@example.com is verified/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /sign in/i })).toHaveAttribute("href", "/login");
  });

  it("refreshes the session after a signed-in user verifies", async () => {
    // Arrange
    mockTakeToken.mockReturnValue("raw-token");
    mockVerifyEmail.mockResolvedValue({ message: "ok", purpose: "VerifyAddress", email: "maker@example.com" });
    const refreshUserFromMe = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(<VerifyEmailPage />, { mockAuth: { user: signedInUser, refreshUserFromMe } });

    // Act
    await user.click(await screen.findByRole("button", { name: /verify email/i }));

    // Assert
    await waitFor(() => expect(refreshUserFromMe).toHaveBeenCalled());
    expect(mockRefreshPromise).toHaveBeenCalled();
  });

  it("uses email change wording and shows the new address after confirming", async () => {
    // Arrange
    mockSearchParamsGet.mockImplementation((key: string) => (key === "type" ? "email-change" : null));
    mockTakeToken.mockReturnValue("raw-token");
    mockVerifyEmail.mockResolvedValue({ message: "ok", purpose: "ChangeAddress", email: "new@example.com" });
    const user = userEvent.setup();
    render(<VerifyEmailPage />, { mockAuth: { user: null } });

    // Act
    await user.click(await screen.findByRole("button", { name: /confirm new email/i }));

    // Assert
    expect(await screen.findByText(/your account email is now new@example.com/i)).toBeInTheDocument();
  });

  it("shows the server error and a resend form when verification fails", async () => {
    // Arrange
    mockTakeToken.mockReturnValue("raw-token");
    mockVerifyEmail.mockRejectedValue({
      isAxiosError: true,
      response: { status: 400, data: { message: "Invalid or expired verification link." } },
    });
    const user = userEvent.setup();
    render(<VerifyEmailPage />, { mockAuth: { user: null } });

    // Act
    await user.click(await screen.findByRole("button", { name: /verify email/i }));

    // Assert
    expect(await screen.findByText(/invalid or expired verification link/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/send a new verification link/i)).toBeInTheDocument();
  });

  it("prefills the resend form for a signed-in user and shows a neutral confirmation", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<VerifyEmailPage />, { mockAuth: { user: signedInUser } });
    const input = await screen.findByLabelText(/send a new verification link/i);
    await waitFor(() => expect(input).toHaveValue("maker@example.com"));

    // Act
    await user.click(screen.getByRole("button", { name: /send new link/i }));

    // Assert
    expect(mockResend).toHaveBeenCalledWith("maker@example.com");
    expect(await screen.findByText(/if that address belongs to an unverified account/i)).toBeInTheDocument();
  });

  it("explains that the link is needed when there is no token", async () => {
    // Arrange
    mockTakeToken.mockReturnValue(null);

    // Act
    render(<VerifyEmailPage />, { mockAuth: { user: null } });

    // Assert
    expect(await screen.findByText(/needs the link from your verification email/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /verify email/i })).not.toBeInTheDocument();
  });
});
