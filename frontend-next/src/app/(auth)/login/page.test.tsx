import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import LoginPage from "./page";

const mockPush = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
  useSearchParams: () => ({ get: (key: string) => (key === "redirect" ? null : null) }),
}));

describe("LoginPage", () => {
  beforeEach(() => {
    mockPush.mockClear();
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
});
