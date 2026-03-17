import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminDashboardPage from "./page";

const mockGetAdminModelStatistics = vi.fn();

vi.mock("@/lib/services/adminService", () => ({
  getAdminModelStatistics: () => mockGetAdminModelStatistics(),
}));

describe("AdminDashboardPage", () => {
  beforeEach(() => {
    mockGetAdminModelStatistics.mockReset();
  });

  it("renders Analytics Dashboard heading", async () => {
    mockGetAdminModelStatistics.mockResolvedValue({
      totalModels: 0,
      totalFiles: 0,
      totalFileSizeFormatted: "0 B",
      averageFilesPerModel: 0,
      publicModels: 0,
      privateModels: 0,
      unlistedModels: 0,
      aiGeneratedModels: 0,
    });

    render(<AdminDashboardPage />);

    await waitFor(() => {
      expect(screen.getByText("Analytics Dashboard")).toBeInTheDocument();
    });
  });

  it("displays stat cards with loaded data", async () => {
    mockGetAdminModelStatistics.mockResolvedValue({
      totalModels: 42,
      totalFiles: 100,
      totalFileSizeFormatted: "1.5 GB",
      averageFilesPerModel: 2.4,
      publicModels: 30,
      privateModels: 10,
      unlistedModels: 2,
      aiGeneratedModels: 0,
    });

    render(<AdminDashboardPage />);

    await waitFor(() => {
      expect(screen.getByText("42")).toBeInTheDocument();
      expect(screen.getByText("1.5 GB")).toBeInTheDocument();
      expect(screen.getByText("100")).toBeInTheDocument();
    });
  });

  it("shows loading state initially", () => {
    mockGetAdminModelStatistics.mockImplementation(
      () => new Promise(() => {})
    );

    render(<AdminDashboardPage />);

    expect(screen.getByText("Loading analytics...")).toBeInTheDocument();
  });

  it("shows error state when fetch fails", async () => {
    mockGetAdminModelStatistics.mockRejectedValue(new Error("Network error"));

    render(<AdminDashboardPage />);

    await waitFor(() => {
      expect(screen.getByText("Failed to load model statistics")).toBeInTheDocument();
    });
    expect(screen.getByText("Retry")).toBeInTheDocument();
  });
});
