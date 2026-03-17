import { describe, it, expect, vi, beforeEach } from "vitest";
import * as adminService from "./adminService";

const mockClient = {
  getAdminModelStatistics_GetAdminModelStatistics: vi.fn(),
  getUsers_GetUsers: vi.fn(),
  roleManagement_GetAllRoles: vi.fn(),
  roleManagement_GetAllPermissions: vi.fn(),
};

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: () => mockClient,
  },
}));

describe("adminService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("getAdminModelStatistics calls API client", async () => {
    const mockStats = { totalModels: 10, totalFiles: 100 };
    mockClient.getAdminModelStatistics_GetAdminModelStatistics.mockResolvedValue(
      mockStats
    );

    const result = await adminService.getAdminModelStatistics();

    expect(
      mockClient.getAdminModelStatistics_GetAdminModelStatistics
    ).toHaveBeenCalledTimes(1);
    expect(result).toEqual(mockStats);
  });

  it("getUsers passes filters to API client", async () => {
    const mockResponse = { users: [], totalCount: 0, totalPages: 1 };
    mockClient.getUsers_GetUsers.mockResolvedValue(mockResponse);

    await adminService.getUsers(1, 20, "test", "Admin", "Active", "Username", true);

    expect(mockClient.getUsers_GetUsers).toHaveBeenCalledWith(
      1,
      20,
      "test",
      "Admin",
      "Active",
      "Username",
      true
    );
  });

  it("getRoles calls roleManagement API", async () => {
    const mockResponse = { roles: [], pagination: { totalCount: 0 } };
    mockClient.roleManagement_GetAllRoles.mockResolvedValue(mockResponse);

    await adminService.getRoles(1, 10, undefined, "priority", true);

    expect(mockClient.roleManagement_GetAllRoles).toHaveBeenCalledWith(
      1,
      10,
      undefined,
      "priority",
      true
    );
  });
});
