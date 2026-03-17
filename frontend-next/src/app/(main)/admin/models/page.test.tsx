import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import AdminModelsPage from "./page";

const mockGetModelConfig = vi.fn();
const mockGetSiteModelSettings = vi.fn();

vi.mock("@/lib/services/adminService", () => ({
  getModelConfigurationSettings: () => mockGetModelConfig(),
  updateModelConfigurationSettings: vi.fn(),
  getSiteModelSettings: () => mockGetSiteModelSettings(),
  updateSiteModelSettings: vi.fn(),
  deleteAllModels: vi.fn(),
}));

describe("AdminModelsPage", () => {
  beforeEach(() => {
    mockGetModelConfig.mockReset();
    mockGetSiteModelSettings.mockReset();
    mockGetModelConfig.mockResolvedValue({
      success: true,
      settings: {
        allowAnonUploads: false,
        requireUploadModeration: true,
        allowAnonDownloads: true,
        enableModelVersioning: true,
      },
    });
    mockGetSiteModelSettings.mockResolvedValue({
      success: true,
      settings: {
        maxFileSizeBytes: 100 * 1024 * 1024,
        maxFilesPerUpload: 5,
        allowedFileTypes: ".stl,.obj",
        enableFileCompression: true,
        autoGeneratePreviews: true,
        requireModeration: true,
        requireLoginForUpload: true,
      },
    });
  });

  it("renders Model Settings heading", async () => {
    render(<AdminModelsPage />);

    await waitFor(() => {
      expect(screen.getByText("Model Settings")).toBeInTheDocument();
    });
  });

  it("renders Model Configuration section", async () => {
    render(<AdminModelsPage />);

    await waitFor(() => {
      expect(screen.getByText("Model Configuration")).toBeInTheDocument();
    });
  });

  it("fetches settings on mount", async () => {
    render(<AdminModelsPage />);

    await waitFor(() => {
      expect(mockGetModelConfig).toHaveBeenCalled();
      expect(mockGetSiteModelSettings).toHaveBeenCalled();
    });
  });
});
