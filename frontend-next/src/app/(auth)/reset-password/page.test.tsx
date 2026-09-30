import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import type { AuthUser } from "@/lib/auth/authSession";
import ResetPasswordPage from "./page";

const mockSearchParamsGet = vi.fn((_key: string): string | null => null);
const mockResetPassword = vi.fn();
const mockTakeToken = vi.fn((): string | null => null);

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  useSearchParams: () => ({ get: mockSearchParamsGet }),
}));

vi.mock("@/lib/services/accountEmailService", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/lib/services/accountEmailService")>()),
  resetPassword: (token: string, newPassword: string, confirmPassword: string) =>
    mockResetPassword(token, newPassword, confirmPassword),
  takeTokenFromUrl: () => mockTakeToken(),
}));

describe("ResetPasswordPage", () => {
  beforeEach(() => {
    mockSearchParamsGet.mockReset().mockReturnValue(null);
    mockResetPassword.mockReset().mockResolvedValue(undefined);
    mockTakeToken.mockReset().mockReturnValue("raw-token");
  });

  it("submits the token with the new password and shows success", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<ResetPasswordPage />, { mockAuth: { user: null } });

    // Act
    await user.type(await screen.findByLabelText(/new password/i), "NewPassword1");
    await user.type(screen.getByLabelText(/confirm password/i), "NewPassword1");
    await user.click(screen.getByRole("button", { name: /update password/i }));

    // Assert
    expect(mockResetPassword).toHaveBeenCalledWith("raw-token", "NewPassword1", "NewPassword1");
    expect(await screen.findByText(/signed out of every device/i)).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /sign in/i })).toHaveAttribute("href", "/login");
  });

  it("rejects mismatched passwords without calling the API", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<ResetPasswordPage />, { mockAuth: { user: null } });

    // Act
    await user.type(await screen.findByLabelText(/new password/i), "NewPassword1");
    await user.type(screen.getByLabelText(/confirm password/i), "Different1");
    await user.click(screen.getByRole("button", { name: /update password/i }));

    // Assert
    expect(await screen.findByText("Passwords do not match.")).toBeInTheDocument();
    expect(mockResetPassword).not.toHaveBeenCalled();
  });

  it("shows the server error when the link is invalid", async () => {
    // Arrange
    mockResetPassword.mockRejectedValue({
      isAxiosError: true,
      response: { status: 400, data: { message: "Invalid or expired reset link." } },
    });
    const user = userEvent.setup();
    render(<ResetPasswordPage />, { mockAuth: { user: null } });

    // Act
    await user.type(await screen.findByLabelText(/new password/i), "NewPassword1");
    await user.type(screen.getByLabelText(/confirm password/i), "NewPassword1");
    await user.click(screen.getByRole("button", { name: /update password/i }));

    // Assert
    expect(await screen.findByText(/invalid or expired reset link/i)).toBeInTheDocument();
  });

  it("signs out a signed-in user after resetting", async () => {
    // Arrange
    const logout = vi.fn();
    const signedIn: AuthUser = { id: "1", email: "a@example.com", username: "a", accessToken: "t" };
    const user = userEvent.setup();
    render(<ResetPasswordPage />, { mockAuth: { user: signedIn, logout } });

    // Act
    await user.type(await screen.findByLabelText(/new password/i), "NewPassword1");
    await user.type(screen.getByLabelText(/confirm password/i), "NewPassword1");
    await user.click(screen.getByRole("button", { name: /update password/i }));

    // Assert
    await waitFor(() => expect(logout).toHaveBeenCalled());
  });

  it("uses invite wording when opened from an invite link", async () => {
    // Arrange
    mockSearchParamsGet.mockImplementation((key: string) => (key === "invite" ? "1" : null));

    // Act
    render(<ResetPasswordPage />, { mockAuth: { user: null } });

    // Assert
    expect(await screen.findByRole("button", { name: /set password/i })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: /welcome/i })).toBeInTheDocument();
  });

  it("offers a new link when there is no token", async () => {
    // Arrange
    mockTakeToken.mockReturnValue(null);

    // Act
    render(<ResetPasswordPage />, { mockAuth: { user: null } });

    // Assert
    expect(await screen.findByRole("link", { name: /request a new link/i })).toHaveAttribute("href", "/forgot-password");
  });
});
