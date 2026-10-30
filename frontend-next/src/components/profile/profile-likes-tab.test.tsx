import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import { ProfileLikesTab } from "./profile-likes-tab";

const mockFetchUserLikedModels = vi.fn();

vi.mock("@/lib/services/userLikedModelsService", () => ({
  fetchUserLikedModels: (...args: unknown[]) => mockFetchUserLikedModels(...args),
}));

vi.mock("@/components/models/model-card", () => ({
  ModelCard: ({ model }: { model: { name?: string } }) => (
    <div data-testid="model-card">{model.name}</div>
  ),
  ModelCardSkeleton: () => <div data-testid="model-skeleton" />,
}));

describe("ProfileLikesTab", () => {
  beforeEach(() => {
    mockFetchUserLikedModels.mockReset();
    mockFetchUserLikedModels.mockResolvedValue({
      models: [{ id: "m1", name: "Benchy", author: { username: "maker" } }],
      totalCount: 1,
      page: 1,
      pageSize: 12,
      totalPages: 1,
    });
  });

  it("renders a liked model card", async () => {
    render(<ProfileLikesTab username="maker" page={1} onPageChange={() => {}} />);

    await waitFor(() => expect(screen.getByTestId("model-card")).toHaveTextContent("Benchy"));
  });
});
