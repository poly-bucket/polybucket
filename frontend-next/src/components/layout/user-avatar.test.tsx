import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { UserAvatar } from "./user-avatar";

vi.mock("@/lib/avatar/minidenticon", () => ({
  minidenticonSvg: () => "<svg/>",
  resolvedImageSrcFromAvatarField: (v: string | undefined | null) => {
    if (v == null || v === "") return null;
    if (v.startsWith("http://") || v.startsWith("https://")) return v;
    if (v.startsWith("data:")) return v;
    if (v.trimStart().toLowerCase().startsWith("<svg")) {
      return `data:image/svg+xml,${encodeURIComponent(v)}`;
    }
    return null;
  },
  svgToDataUrl: (s: string) => `data:image/svg+xml,${encodeURIComponent(s)}`,
}));

describe("UserAvatar", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("Arrange: profile picture set; Act: render; Assert: uses profile image", () => {
    render(
      <UserAvatar
        userId="u1"
        username="tester"
        profilePictureUrl="https://example.com/p.png"
      />
    );
    const img = screen.getByRole("img", { name: /tester's profile picture/i });
    expect(img).toHaveAttribute("src", "https://example.com/p.png");
  });

  it("Arrange: no profile but stored avatar; Act: render; Assert: uses resolved stored", () => {
    render(
      <UserAvatar
        userId="u1"
        username="tester"
        avatar="<svg xmlns='http://www.w3.org/2000/svg'></svg>"
      />
    );
    const img = screen.getByRole("img", { name: /tester's avatar/i });
    expect(img.getAttribute("src")?.startsWith("data:image/svg+xml,")).toBe(
      true
    );
  });

  it("Arrange: no profile or stored; Act: render; Assert: uses minidenticon data url", () => {
    render(<UserAvatar userId="u1" username="tester" />);
    const img = screen.getByRole("img", { name: /tester's avatar/i });
    expect(img.getAttribute("src")?.startsWith("data:image/svg+xml,")).toBe(
      true
    );
  });
});
