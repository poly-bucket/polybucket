import { describe, it, expect, vi } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import ProfileSettingsPage from "./page";
import { fetchUserProfile } from "@/lib/services/userProfileService";
import type { UserProfileData } from "@/lib/services/userProfileService";

const mockUser = {
  id: "user-1",
  email: "test@example.com",
  username: "testuser",
  accessToken: "token",
};

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn() }),
  usePathname: () => "/settings/profile",
  useSearchParams: () => ({ get: () => null }),
}));

vi.mock("@/lib/services/userProfileService", () => ({
  fetchUserProfile: vi.fn(),
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: () => ({
      updateUserProfile_UpdateUserProfile: vi.fn().mockResolvedValue(undefined),
    }),
  },
}));

describe("ProfileSettingsPage", () => {
  const mockFetchUserProfile = vi.mocked(fetchUserProfile);

  beforeEach(() => {
    mockFetchUserProfile.mockResolvedValue({
      username: "testuser",
      bio: "",
      country: "",
      websiteUrl: "",
      twitterUrl: "",
      instagramUrl: "",
      youtubeUrl: "",
    } as UserProfileData);
  });

  it("renders form fields after loading profile", async () => {
    render(<ProfileSettingsPage />, {
      mockAuth: { user: mockUser },
    });

    await waitFor(() => {
      expect(screen.getByPlaceholderText(/tell us about yourself/i)).toBeInTheDocument();
    });

    expect(screen.getByPlaceholderText(/your country/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/https:\/\/example.com/i)).toBeInTheDocument();
  });

  it("calls fetchUserProfile with user id", async () => {
    render(<ProfileSettingsPage />, {
      mockAuth: { user: mockUser },
    });

    await waitFor(() => {
      expect(mockFetchUserProfile).toHaveBeenCalledWith("user-1");
    });
  });

  it("shows loading state initially", () => {
    mockFetchUserProfile.mockImplementation(
      () => new Promise(() => {})
    );

    render(<ProfileSettingsPage />, {
      mockAuth: { user: mockUser },
    });

    const spinner = document.querySelector(".animate-spin");
    expect(spinner).toBeInTheDocument();
  });
});
