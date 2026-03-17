import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminUsersPage from "./page";

const mockGetUsers = vi.fn();

vi.mock("@/lib/services/adminService", () => ({
  getUsers: (...args: unknown[]) => mockGetUsers(...args),
  banUser: vi.fn(),
  unbanUser: vi.fn(),
}));

describe("AdminUsersPage", () => {
  beforeEach(() => {
    mockGetUsers.mockReset();
    mockGetUsers.mockResolvedValue({
      users: [],
      totalCount: 0,
      totalPages: 1,
    });
  });

  it("renders User Management heading", async () => {
    render(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText("User Management")).toBeInTheDocument();
    });
  });

  it("renders Refresh button", async () => {
    render(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /refresh/i })).toBeInTheDocument();
    });
  });

  it("calls getUsers on mount", async () => {
    render(<AdminUsersPage />);

    await waitFor(() => {
      expect(mockGetUsers).toHaveBeenCalled();
    });
  });

  it("shows no users found when empty", async () => {
    mockGetUsers.mockResolvedValue({
      users: [],
      totalCount: 0,
      totalPages: 1,
    });

    render(<AdminUsersPage />);

    await waitFor(() => {
      expect(screen.getByText("No users found")).toBeInTheDocument();
    });
  });
});
