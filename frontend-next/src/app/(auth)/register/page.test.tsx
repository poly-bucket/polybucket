import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import RegisterPage from "./page";

const mockPush = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
}));

const mockFetchRegistrationStatus = vi.fn();

vi.mock("@/lib/services/registrationService", async (importOriginal) => {
  const actual = await importOriginal<typeof import("@/lib/services/registrationService")>();
  return {
    ...actual,
    fetchRegistrationStatus: () => mockFetchRegistrationStatus(),
  };
});

describe("RegisterPage", () => {
  beforeEach(() => {
    mockPush.mockClear();
    mockFetchRegistrationStatus.mockReset();
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ requiresEmailVerification: true }),
      })
    );
  });

  it("shows registration closed when public signup is disabled", async () => {
    // Arrange
    mockFetchRegistrationStatus.mockResolvedValue(false);

    // Act
    render(<RegisterPage />);

    // Assert
    expect(await screen.findByRole("heading", { name: /registration closed/i })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /sign in/i })).toHaveAttribute("href", "/login");
  });

  it("shows check your email after signup when verification is required", async () => {
    // Arrange
    mockFetchRegistrationStatus.mockResolvedValue(true);
    const user = userEvent.setup();

    // Act
    render(<RegisterPage />);
    await screen.findByRole("heading", { name: /create account/i });
    await user.type(screen.getByLabelText(/^email$/i), "maker@test.com");
    await user.type(screen.getByLabelText(/^username$/i), "maker");
    await user.type(screen.getByLabelText(/^password$/i), "Password123!");
    await user.type(screen.getByLabelText(/^confirm password$/i), "Password123!");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    // Assert
    expect(await screen.findByRole("heading", { name: /check your email/i })).toBeInTheDocument();
  });

  it("redirects home after signup when the session is created", async () => {
    // Arrange
    mockFetchRegistrationStatus.mockResolvedValue(true);
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue({
        ok: true,
        json: async () => ({ user: { id: "u1", email: "open@test.com", username: "open" } }),
      })
    );
    const user = userEvent.setup();

    // Act
    render(<RegisterPage />);
    await screen.findByRole("heading", { name: /create account/i });
    await user.type(screen.getByLabelText(/^email$/i), "open@test.com");
    await user.type(screen.getByLabelText(/^username$/i), "open");
    await user.type(screen.getByLabelText(/^password$/i), "Password123!");
    await user.type(screen.getByLabelText(/^confirm password$/i), "Password123!");
    await user.click(screen.getByRole("button", { name: /create account/i }));

    // Assert
    await waitFor(() => expect(mockPush).toHaveBeenCalledWith("/"));
  });
});
