import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { MarketplacePluginsSection } from "./marketplace-plugins-section";

const mockBrowse = vi.fn();
const mockGetBaseUrl = vi.fn();
const mockUpdateBaseUrl = vi.fn();

vi.mock("@/lib/services/pluginsAdminService", () => ({
  browseMarketplacePlugins: (...args: unknown[]) => mockBrowse(...args),
  getMarketplaceBaseUrl: () => mockGetBaseUrl(),
  updateMarketplaceBaseUrl: (...args: unknown[]) => mockUpdateBaseUrl(...args),
  installMarketplacePlugin: vi.fn(),
}));

describe("MarketplacePluginsSection", () => {
  it("renders catalog results and sends search filter to browse", async () => {
    // Arrange
    mockGetBaseUrl.mockResolvedValue("http://localhost:5001");
    mockBrowse.mockResolvedValue({
      plugins: [{ id: "dark-theme-plugin", name: "Dark Theme", description: "Theme", version: "1.0.0", category: "Themes", repositoryUrl: "https://github.com/a/b", targets: ["frontend"] }],
      totalCount: 1,
      page: 1,
      pageSize: 10,
      totalPages: 1,
    });
    const user = userEvent.setup();

    // Act
    render(<MarketplacePluginsSection />);
    await waitFor(() => expect(mockBrowse).toHaveBeenCalled());
    const search = await screen.findByPlaceholderText("Search plugins");
    await user.type(search, "dark");

    // Assert
    await waitFor(() =>
      expect(mockBrowse).toHaveBeenCalledWith(
        expect.objectContaining({ search: "dark" })
      )
    );
    expect(await screen.findByText("Dark Theme")).toBeInTheDocument();
  });
});
