import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@/test/test-utils";
import userEvent from "@testing-library/user-event";
import { BulkOperationsTab } from "./bulk-operations-tab";

vi.mock("@/lib/services/adminService", () => ({
  deleteAllModels: vi.fn(),
  deleteAllFileSettings: vi.fn(),
  deleteAllCategories: vi.fn(),
  deleteAllCollections: vi.fn(),
}));

describe("BulkOperationsTab", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders all four bulk delete operations", () => {
    render(<BulkOperationsTab />);

    expect(screen.getByText("Delete all models")).toBeInTheDocument();
    expect(screen.getByText("Delete all file types")).toBeInTheDocument();
    expect(screen.getByText("Delete all categories")).toBeInTheDocument();
    expect(screen.getByText("Delete all collections")).toBeInTheDocument();
  });

  it("requires confirmation phrase before proceeding to password step", async () => {
    const user = userEvent.setup();
    render(<BulkOperationsTab />);

    await user.click(screen.getByRole("button", { name: /Delete All Models/i }));

    await waitFor(() => {
      expect(screen.getByPlaceholderText("DELETE ALL")).toBeInTheDocument();
    });

    const nextButton = screen.getByRole("button", { name: "Next" });
    expect(nextButton).toBeDisabled();

    await user.type(screen.getByPlaceholderText("DELETE ALL"), "DELETE ALL");

    expect(nextButton).not.toBeDisabled();
  });
});
