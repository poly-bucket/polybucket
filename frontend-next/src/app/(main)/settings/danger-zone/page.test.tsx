import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import DangerZoneSettingsPage from "./page";
import {
  exportAccountData,
  revokeAllSessions,
  deleteAccount,
  getActiveSessions,
  revokeSession,
} from "@/lib/services/dangerZoneService";

const mockPush = vi.fn();
const mockLogout = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush }),
}));

vi.mock("@/lib/services/dangerZoneService", () => ({
  exportAccountData: vi.fn(),
  revokeAllSessions: vi.fn(),
  deleteAccount: vi.fn(),
  getActiveSessions: vi.fn(),
  revokeSession: vi.fn(),
}));

vi.mock("@/services/twoFactorAuthService", () => ({
  twoFactorAuthService: {
    getStatus: vi.fn().mockResolvedValue({
      isEnabled: false,
      isInitialized: false,
      remainingBackupCodes: 0,
    }),
  },
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe("DangerZoneSettingsPage", () => {
  const mockUser = {
    id: "u1",
    email: "a@b.com",
    username: "myuser",
    accessToken: "t",
  };

  beforeEach(() => {
    vi.mocked(exportAccountData).mockResolvedValue({
      exportedAtUtc: new Date().toISOString(),
      userId: "u1",
      email: "a@b.com",
      username: "myuser",
      isProfilePublic: true,
      showEmail: false,
      showLastLogin: false,
      showStatistics: true,
      createdAt: new Date().toISOString(),
    });
    vi.mocked(revokeAllSessions).mockResolvedValue(undefined);
    vi.mocked(deleteAccount).mockResolvedValue(undefined);
    vi.mocked(getActiveSessions).mockResolvedValue([]);
    vi.mocked(revokeSession).mockResolvedValue(undefined);
    mockPush.mockClear();
    mockLogout.mockClear();
    vi.spyOn(URL, "createObjectURL").mockReturnValue("blob:mock");
    vi.spyOn(URL, "revokeObjectURL").mockImplementation(() => {});
  });

  it("renders export, sign out everywhere, and delete sections", async () => {
    render(<DangerZoneSettingsPage />, {
      mockAuth: { user: mockUser, logout: mockLogout },
    });

    expect(await screen.findByText(/export data/i)).toBeInTheDocument();
    expect(screen.getAllByText(/active sessions/i).length).toBeGreaterThan(0);
    expect(screen.getByText(/sign out everywhere/i)).toBeInTheDocument();
    expect(screen.getByText(/delete account/i)).toBeInTheDocument();
  });

  it("calls export when download button is clicked", async () => {
    const user = userEvent.setup();
    const clickSpy = vi.spyOn(HTMLAnchorElement.prototype, "click").mockImplementation(() => {});

    render(<DangerZoneSettingsPage />, {
      mockAuth: { user: mockUser, logout: mockLogout },
    });

    await user.click(screen.getByRole("button", { name: /download export/i }));

    await waitFor(() => {
      expect(exportAccountData).toHaveBeenCalledTimes(1);
    });

    clickSpy.mockRestore();
  });

  it("revokes sessions and redirects to login", async () => {
    const user = userEvent.setup();

    render(<DangerZoneSettingsPage />, {
      mockAuth: { user: mockUser, logout: mockLogout },
    });

    await user.click(screen.getByRole("button", { name: /sign out of all sessions/i }));
    await user.click(screen.getByRole("button", { name: /^confirm$/i }));

    await waitFor(() => {
      expect(revokeAllSessions).toHaveBeenCalled();
      expect(mockLogout).toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalledWith("/login");
    });
  });

  it("submits delete account with confirmation and password", async () => {
    const user = userEvent.setup();

    render(<DangerZoneSettingsPage />, {
      mockAuth: { user: mockUser, logout: mockLogout },
    });

    await user.click(screen.getByRole("button", { name: /delete my account/i }));

    await screen.findByRole("heading", { name: /delete account/i });

    await user.type(screen.getByLabelText(/confirm username/i), "myuser");
    await user.type(screen.getByLabelText(/current password/i), "secretpass");

    await user.click(screen.getByRole("button", { name: /delete account permanently/i }));

    await waitFor(() => {
      expect(deleteAccount).toHaveBeenCalled();
      expect(mockLogout).toHaveBeenCalled();
      expect(mockPush).toHaveBeenCalledWith("/login");
    });

    const payload = vi.mocked(deleteAccount).mock.calls[0]?.[0] as
      | { password?: string }
      | undefined;
    expect(payload?.password).toBe("secretpass");
  });
});
