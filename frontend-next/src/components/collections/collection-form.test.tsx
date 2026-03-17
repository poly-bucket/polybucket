import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import { CollectionForm } from "./collection-form";

describe("CollectionForm", () => {
  it("renders with submit label and disables submit when name is empty", () => {
    const onSubmit = vi.fn();
    render(
      <CollectionForm
        onSubmit={onSubmit}
        submitLabel="Create"
        isSubmitting={false}
        onCancel={vi.fn()}
      />
    );

    expect(screen.getByRole("button", { name: /create/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /create/i })).toBeDisabled();
  });

  it("shows password field when visibility is Unlisted", async () => {
    const user = userEvent.setup();
    render(
      <CollectionForm
        onSubmit={vi.fn()}
        submitLabel="Create"
        isSubmitting={false}
        onCancel={vi.fn()}
      />
    );
    expect(screen.queryByPlaceholderText(/password to protect/)).not.toBeInTheDocument();

    await user.click(screen.getByLabelText(/unlisted/i));
    expect(screen.getByPlaceholderText(/password to protect/)).toBeInTheDocument();
  });

  it("calls onSubmit with values on valid submit", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <CollectionForm
        onSubmit={onSubmit}
        submitLabel="Save"
        isSubmitting={false}
        onCancel={vi.fn()}
      />
    );

    await user.type(screen.getByPlaceholderText("Enter collection name"), "My Collection");
    await user.type(
      screen.getByPlaceholderText(/describe your collection/i),
      "A description"
    );
    await user.click(screen.getByRole("button", { name: /save/i }));

    expect(onSubmit).toHaveBeenCalledWith({
      name: "My Collection",
      description: "A description",
      visibility: "Private",
      password: undefined,
    });
  });

  it("includes password when visibility is Unlisted", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);
    render(
      <CollectionForm
        onSubmit={onSubmit}
        submitLabel="Save"
        isSubmitting={false}
        onCancel={vi.fn()}
      />
    );

    await user.type(screen.getByPlaceholderText("Enter collection name"), "Col");
    await user.click(screen.getByLabelText(/unlisted/i));
    await user.type(screen.getByPlaceholderText(/password to protect/), "secret123");
    await user.click(screen.getByRole("button", { name: /save/i }));

    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        name: "Col",
        visibility: "Unlisted",
        password: "secret123",
      })
    );
  });

  it("calls onCancel when Cancel is clicked", async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    render(
      <CollectionForm
        onSubmit={vi.fn()}
        submitLabel="Create"
        isSubmitting={false}
        onCancel={onCancel}
      />
    );
    await user.click(screen.getByRole("button", { name: /cancel/i }));
    expect(onCancel).toHaveBeenCalled();
  });

  it("uses initialValues when provided", () => {
    render(
      <CollectionForm
        initialValues={{
          name: "Existing",
          description: "Existing desc",
          visibility: "Public",
        }}
        onSubmit={vi.fn()}
        submitLabel="Update"
        isSubmitting={false}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByDisplayValue("Existing")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Existing desc")).toBeInTheDocument();
    expect(screen.getByLabelText(/public/i)).toBeChecked();
  });

  it("disables submit when isSubmitting", () => {
    render(
      <CollectionForm
        onSubmit={vi.fn()}
        submitLabel="Create"
        isSubmitting={true}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByRole("button", { name: /saving/i })).toBeDisabled();
  });
});
