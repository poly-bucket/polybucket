import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import ModerationReportsPage from "./page";

const mockGetAllReports = vi.fn();
const mockGetReportsAnalytics = vi.fn();

vi.mock("@/components/moderation/reports-dashboard", () => ({
  ReportsDashboard: () => <div>Reports Dashboard</div>,
}));

describe("ModerationReportsPage", () => {
  beforeEach(() => {
    mockGetAllReports.mockReset();
    mockGetReportsAnalytics.mockReset();
  });

  it("renders Reports Dashboard", async () => {
    render(<ModerationReportsPage />);

    await waitFor(() => {
      expect(screen.getByText("Reports Dashboard")).toBeInTheDocument();
    });
  });
});
