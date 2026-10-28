import { describe, it, expect } from "vitest";
import { render, screen } from "@/test/test-utils";
import { ProfileHeader } from "./profile-header";
import type { UserProfileData } from "@/lib/services/userProfileService";

function baseProfile(overrides: Partial<UserProfileData> = {}): UserProfileData {
  return {
    id: "u1",
    username: "maker",
    websiteUrl: "",
    twitterUrl: "",
    instagramUrl: "",
    youtubeUrl: "",
    ...overrides,
  } as UserProfileData;
}

describe("ProfileHeader", () => {
  it("renders safe external website link", () => {
    render(
      <ProfileHeader
        profile={baseProfile({ websiteUrl: "https://example.com" })}
        isOwnProfile={false}
      />
    );
    const link = screen.getByRole("link", { name: "Website" });
    expect(link.getAttribute("href")).toMatch(/^https:\/\/example\.com\/?$/);
  });

  it("does not render javascript links", () => {
    render(
      <ProfileHeader
        profile={baseProfile({ websiteUrl: "javascript:alert(1)" })}
        isOwnProfile={false}
      />
    );
    expect(screen.queryByRole("link", { name: "Website" })).not.toBeInTheDocument();
  });
});
