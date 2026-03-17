import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminRolesPage from "./page";

const mockGetRoles = vi.fn();
const mockGetAllPermissions = vi.fn();

vi.mock("@/lib/services/adminService", () => ({
  getRoles: (...args: unknown[]) => mockGetRoles(...args),
  getAllPermissions: () => mockGetAllPermissions(),
  createRole: vi.fn(),
  updateRole: vi.fn(),
  deleteRole: vi.fn(),
  assignPermissionsToRole: vi.fn(),
  removePermissionsFromRole: vi.fn(),
}));

describe("AdminRolesPage", () => {
  beforeEach(() => {
    mockGetRoles.mockReset();
    mockGetAllPermissions.mockReset();
    mockGetRoles.mockResolvedValue({
      roles: [],
      pagination: { totalCount: 0, totalPages: 1 },
    });
    mockGetAllPermissions.mockResolvedValue([]);
  });

  it("renders Roles heading", async () => {
    render(<AdminRolesPage />);

    await waitFor(() => {
      expect(screen.getByText("Roles & Permissions")).toBeInTheDocument();
    });
  });

  it("renders Create Role button", async () => {
    render(<AdminRolesPage />);

    await waitFor(() => {
      expect(screen.getByRole("button", { name: /create role/i })).toBeInTheDocument();
    });
  });

  it("calls getRoles and getAllPermissions on mount", async () => {
    render(<AdminRolesPage />);

    await waitFor(() => {
      expect(mockGetRoles).toHaveBeenCalled();
      expect(mockGetAllPermissions).toHaveBeenCalled();
    });
  });
});
