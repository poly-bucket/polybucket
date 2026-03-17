import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import { DeleteCollectionDialog } from "./delete-collection-dialog";

describe("DeleteCollectionDialog", () => {
  it("does not render when isOpen is false", () => {
    render(
      <DeleteCollectionDialog
        isOpen={false}
        collectionName="Test"
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        isDeleting={false}
      />
    );
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("renders with collection name when open", () => {
    render(
      <DeleteCollectionDialog
        isOpen={true}
        collectionName="My Collection"
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        isDeleting={false}
      />
    );
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByText(/My Collection/)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /^delete$/i })).toBeInTheDocument();
  });

  it("calls onConfirm when Delete is clicked", async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    render(
      <DeleteCollectionDialog
        isOpen={true}
        collectionName="Test"
        onConfirm={onConfirm}
        onCancel={vi.fn()}
        isDeleting={false}
      />
    );
    await user.click(screen.getByRole("button", { name: /^delete$/i }));
    expect(onConfirm).toHaveBeenCalledTimes(1);
  });

  it("calls onCancel when Cancel is clicked", async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    render(
      <DeleteCollectionDialog
        isOpen={true}
        collectionName="Test"
        onConfirm={vi.fn()}
        onCancel={onCancel}
        isDeleting={false}
      />
    );
    await user.click(screen.getByRole("button", { name: /cancel/i }));
    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  it("shows Deleting... and disables buttons when isDeleting", () => {
    render(
      <DeleteCollectionDialog
        isOpen={true}
        collectionName="Test"
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
        isDeleting={true}
      />
    );
    expect(screen.getByRole("button", { name: /deleting/i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /cancel/i })).toBeDisabled();
  });
});
