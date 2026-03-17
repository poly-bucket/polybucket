import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen, waitFor } from "@/test/test-utils";
import MyModelsPage from "./page";

const mockUser = {
  id: "user-1",
  email: "test@example.com",
  username: "testuser",
  accessToken: "token",
};

const mockGetModels = vi.fn();
const mockDeleteModel = vi.fn();

vi.mock("@/lib/api/clientFactory", () => ({
  ApiClientFactory: {
    getApiClient: () => ({
      getModelByUserId_GetModelsByUserId: mockGetModels,
      deleteModel_DeleteModel: mockDeleteModel,
    }),
  },
}));

vi.mock("sonner", () => ({
  toast: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

describe("MyModelsPage", () => {
  beforeEach(() => {
    mockGetModels.mockReset();
    mockDeleteModel.mockReset();
  });

  it("renders My Models heading", () => {
    mockGetModels.mockResolvedValue({ models: [] });

    render(<MyModelsPage />, { mockAuth: { user: mockUser } });

    expect(screen.getByRole("heading", { name: /my models/i })).toBeInTheDocument();
  });

  it("shows loading skeletons initially", () => {
    mockGetModels.mockImplementation(() => new Promise(() => {}));

    render(<MyModelsPage />, { mockAuth: { user: mockUser } });

    const skeletons = document.querySelectorAll('[class*="animate"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it("shows empty state when user has no models", async () => {
    mockGetModels.mockResolvedValue({ models: [] });

    render(<MyModelsPage />, { mockAuth: { user: mockUser } });

    await waitFor(() => {
      expect(screen.getByText(/you haven't uploaded any models yet/i)).toBeInTheDocument();
    });

    expect(screen.getByRole("link", { name: /upload your first model/i })).toHaveAttribute(
      "href",
      "/models/upload"
    );
  });

  it("shows model cards when user has models", async () => {
    const mockModels = [
      {
        id: "model-1",
        name: "Test Model",
        authorId: "user-1",
        author: { id: "user-1" },
      },
    ];

    mockGetModels.mockResolvedValue({ models: mockModels });

    render(<MyModelsPage />, { mockAuth: { user: mockUser } });

    await waitFor(() => {
      expect(screen.getByText("Test Model")).toBeInTheDocument();
    });
  });
});
