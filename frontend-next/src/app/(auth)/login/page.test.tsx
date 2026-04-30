import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import LoginPage from "./page";

const mockPush = vi.fn();

const mockSearchParamsGet = vi.fn((key: string) =>
  key === "redirect" ? null : null
);

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
  useSearchParams: () => ({ get: mockSearchParamsGet }),
}));

describe("LoginPage", () => {
  beforeEach(() => {
    mockPush.mockClear();
    mockSearchParamsGet.mockImplementation((key: string) =>
      key === "redirect" ? null : null
    );
  });

  it("renders form with heading and inputs", () => {
    render(<LoginPage />, {
      mockAuth: { user: null, login: vi.fn().mockResolvedValue({ success: true }) },
    });

    expect(screen.getByRole("heading", { name: /welcome back/i })).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/email or username/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /sign in/i })).toBeInTheDocument();
  });

  it("shows error when login fails", async () => {
    const login = vi.fn().mockResolvedValue({ success: false, error: "Invalid credentials" });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "test@example.com");
    await user.type(screen.getByPlaceholderText(/password/i), "wrongpassword");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    expect(await screen.findByText("Invalid credentials")).toBeInTheDocument();
  });

  it("calls login with credentials on submit", async () => {
    const login = vi.fn().mockResolvedValue({ success: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    expect(login).toHaveBeenCalledWith("user@test.com", "secret");
  });

  it("redirects to / on successful login", async () => {
    const login = vi.fn().mockResolvedValue({ success: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/");
    });
  });

  it("shows authenticator step when login requires two-factor", async () => {
    const login = vi.fn().mockResolvedValue({ success: false, requiresTwoFactor: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    expect(await screen.findByLabelText(/authentication code/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /^verify$/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /^back$/i })).toBeInTheDocument();
  });

  it("calls login with two-factor token on verify", async () => {
    const login = vi
      .fn()
      .mockResolvedValueOnce({ success: false, requiresTwoFactor: true })
      .mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await screen.findByLabelText(/authentication code/i);
    await user.type(screen.getByLabelText(/authentication code/i), "123456");
    await user.click(screen.getByRole("button", { name: /^verify$/i }));

    await waitFor(() => {
      expect(login).toHaveBeenLastCalledWith("user@test.com", "secret", {
        twoFactorToken: "123456",
      });
    });

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/");
    });
  });

  it("shows error when two-factor verification fails", async () => {
    const login = vi
      .fn()
      .mockResolvedValueOnce({ success: false, requiresTwoFactor: true })
      .mockResolvedValueOnce({ success: false, error: "Invalid two-factor authentication code" });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await screen.findByLabelText(/authentication code/i);
    await user.type(screen.getByLabelText(/authentication code/i), "999999");
    await user.click(screen.getByRole("button", { name: /^verify$/i }));

    expect(
      await screen.findByText("Invalid two-factor authentication code")
    ).toBeInTheDocument();
  });

  it("redirects to safe redirect query path after successful login", async () => {
    mockSearchParamsGet.mockImplementation((key: string) =>
      key === "redirect" ? "/dashboard" : null
    );
    const login = vi.fn().mockResolvedValue({ success: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/dashboard");
    });
  });

  it("redirects to setup when first-time setup is required", async () => {
    const login = vi.fn().mockResolvedValue({ success: true, requiresFirstTimeSetup: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "admin@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/setup");
    });
  });

  it("calls login with backup code when using backup flow", async () => {
    const login = vi
      .fn()
      .mockResolvedValueOnce({ success: false, requiresTwoFactor: true })
      .mockResolvedValueOnce({ success: true });
    const user = userEvent.setup();

    render(<LoginPage />, { mockAuth: { user: null, login } });

    await user.type(screen.getByPlaceholderText(/email or username/i), "user@test.com");
    await user.type(screen.getByPlaceholderText(/password/i), "secret");
    await user.click(screen.getByRole("button", { name: /sign in/i }));

    await screen.findByLabelText(/authentication code/i);
    await user.click(screen.getByRole("button", { name: /use a backup code instead/i }));

    await user.type(screen.getByLabelText(/backup code/i), "ABCD-1234");
    await user.click(screen.getByRole("button", { name: /^verify$/i }));

    await waitFor(() => {
      expect(login).toHaveBeenLastCalledWith("user@test.com", "secret", {
        backupCode: "ABCD-1234",
      });
    });
  });
});
