import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import ForgotPasswordPage from "./page";

const mockRequestPasswordReset = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  useSearchParams: () => ({ get: vi.fn(() => null) }),
}));

vi.mock("@/lib/services/accountEmailService", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/lib/services/accountEmailService")>()),
  requestPasswordReset: (email: string) => mockRequestPasswordReset(email),
}));

describe("ForgotPasswordPage", () => {
  beforeEach(() => {
    mockRequestPasswordReset.mockReset().mockResolvedValue(undefined);
  });

  it("requests a reset and shows a neutral confirmation", async () => {
    // Arrange
    const user = userEvent.setup();
    render(<ForgotPasswordPage />, { mockAuth: { user: null } });

    // Act
    await user.type(screen.getByLabelText(/email/i), "  maker@example.com ");
    await user.click(screen.getByRole("button", { name: /send reset link/i }));

    // Assert
    expect(mockRequestPasswordReset).toHaveBeenCalledWith("maker@example.com");
    expect(await screen.findByText(/check your inbox/i)).toBeInTheDocument();
    expect(screen.getByText(/if an account exists for/i)).toBeInTheDocument();
  });

  it("shows a rate limit message when too many requests are made", async () => {
    // Arrange
    mockRequestPasswordReset.mockRejectedValue({ isAxiosError: true, response: { status: 429, data: {} } });
    const user = userEvent.setup();
    render(<ForgotPasswordPage />, { mockAuth: { user: null } });

    // Act
    await user.type(screen.getByLabelText(/email/i), "maker@example.com");
    await user.click(screen.getByRole("button", { name: /send reset link/i }));

    // Assert
    expect(await screen.findByRole("alert")).toHaveTextContent(/too many/i);
  });

  it("links back to sign in", () => {
    // Arrange
    render(<ForgotPasswordPage />, { mockAuth: { user: null } });

    // Act
    const link = screen.getByRole("link", { name: /back to sign in/i });

    // Assert
    expect(link).toHaveAttribute("href", "/login");
  });
});
