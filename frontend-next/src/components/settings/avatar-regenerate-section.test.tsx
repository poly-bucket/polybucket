import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AvatarRegenerateSection } from "./avatar-regenerate-section";
import { buildGeneratedUserAvatarSavePayload } from "@/lib/avatar/minidenticon";

const mockRegenerate = vi.fn();
const mockRefreshUserFromMe = vi.fn();

vi.mock("@/lib/services/avatarService", () => ({
  regenerateUserAvatar: (...args: unknown[]) => mockRegenerate(...args),
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock("@/contexts/AuthContext", () => ({
  useAuth: () => ({
    user: { id: "11111111-1111-1111-1111-111111111111" },
    refreshUserFromMe: mockRefreshUserFromMe,
  }),
}));

describe("AvatarRegenerateSection", () => {
  const userId = "11111111-1111-1111-1111-111111111111";

  beforeEach(() => {
    vi.clearAllMocks();
    mockRegenerate.mockResolvedValue({ avatar: "<svg/>", userId });
    mockRefreshUserFromMe.mockResolvedValue(undefined);
  });

  it("Arrange: generated pattern with seed; Act: save; Assert: sends preview svg to API", async () => {
    const user = userEvent.setup();
    render(<AvatarRegenerateSection />);

    const seedInput = screen.getByLabelText(/tweak the pattern/i);
    await user.clear(seedInput);
    await user.type(seedInput, "SaveSeed99");

    const expected = buildGeneratedUserAvatarSavePayload(userId, "SaveSeed99");

    await user.click(screen.getByRole("button", { name: /save avatar/i }));

    await waitFor(() => {
      expect(mockRegenerate).toHaveBeenCalledWith(expected.salt, expected.avatar);
    });
    expect(expected.avatar).toContain('viewBox="-1.5 -1.5 8 8"');
  });

  it("Arrange: locked seed with edited input; Act: save; Assert: uses locked salt and matching svg", async () => {
    const user = userEvent.setup();
    render(<AvatarRegenerateSection />);

    const seedInput = screen.getByLabelText(/tweak the pattern/i);
    await user.type(seedInput, "LockedOne");
    await user.click(screen.getByRole("button", { name: /lock avatar seed/i }));
    await user.clear(seedInput);
    await user.type(seedInput, "EditedTwo");

    const lockedPayload = buildGeneratedUserAvatarSavePayload(userId, "LockedOne");

    await user.click(screen.getByRole("button", { name: /save avatar/i }));

    await waitFor(() => {
      expect(mockRegenerate).toHaveBeenCalledWith(
        lockedPayload.salt,
        lockedPayload.avatar
      );
    });
  });
});