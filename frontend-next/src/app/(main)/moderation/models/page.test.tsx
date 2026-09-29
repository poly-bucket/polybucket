import { describe, it, expect, vi } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import ModerationModelsPage from "./page";

vi.mock("@/components/moderation/model-queue-tab", () => ({
  ModelQueueTab: () => <div>Model Queue Tab</div>,
}));

describe("ModerationModelsPage", () => {
  it("renders model queue section", async () => {
    render(<ModerationModelsPage />);

    await waitFor(() => {
      expect(screen.getByText("Model queue")).toBeInTheDocument();
      expect(screen.getByText("Model Queue Tab")).toBeInTheDocument();
    });
  });
});
