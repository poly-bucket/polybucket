import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CollectionForm } from "./collection-form";

vi.mock("@/lib/avatar/minidenticon", async (importOriginal) => {
  const mod = await importOriginal<typeof import("@/lib/avatar/minidenticon")>();
  return {
    ...mod,
    minidenticonSvg: (seed: string) =>
      `<svg xmlns="http://www.w3.org/2000/svg" data-seed="${seed}"/>`,
  };
});

describe("CollectionForm", () => {
  const onCancel = vi.fn();
  const onSubmit = vi.fn().mockResolvedValue(undefined);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("Arrange: name + generated avatar; Act: submit; Assert: onSubmit includes raw svg avatar", async () => {
    const user = userEvent.setup();
    render(
      <CollectionForm
        onSubmit={onSubmit}
        onCancel={onCancel}
        isSubmitting={false}
        submitLabel="Create"
      />
    );

    const nameInput = screen.getByLabelText(/Collection Name/i);
    await user.type(nameInput, "My List");

    const btn = screen.getByRole("button", { name: "Create" });
    await user.click(btn);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalled();
    });
    const payload = onSubmit.mock.calls[0][0] as { avatar?: string; name: string };
    expect(payload.name).toBe("My List");
    expect(payload.avatar).toBeDefined();
    expect(payload.avatar?.trimStart().toLowerCase().startsWith("<svg")).toBe(
      true
    );
  });

  it("Arrange: form render; Act: inspect icon selector; Assert: generated and upload cards are shown", () => {
    render(
      <CollectionForm
        onSubmit={onSubmit}
        onCancel={onCancel}
        isSubmitting={false}
        submitLabel="Create"
      />
    );

    expect(screen.getByText("Generated")).toBeInTheDocument();
    expect(screen.getByText("Upload Image")).toBeInTheDocument();
  });

  it("Arrange: initialValues.avatar; Act: submit; Assert: onSubmit returns that avatar, not a newly generated one", async () => {
    const user = userEvent.setup();
    const existing =
      '<svg xmlns="http://www.w3.org/2000/svg" data-case="kept"><path d="M0 0"/></svg>';
    render(
      <CollectionForm
        initialValues={{
          name: "My List",
          description: "",
          visibility: "Private",
          avatar: existing,
        }}
        onSubmit={onSubmit}
        onCancel={onCancel}
        isSubmitting={false}
        submitLabel="Save"
      />
    );

    const btn = screen.getByRole("button", { name: "Save" });
    await user.click(btn);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalled();
    });
    const payload = onSubmit.mock.calls[0][0] as { avatar?: string; name: string };
    expect(payload.name).toBe("My List");
    expect(payload.avatar).toBe(existing);
  });

  it("Arrange: generated icon seed is locked; Act: type more name and submit; Assert: avatar seed remains based on lock-time name", async () => {
    const user = userEvent.setup();
    render(
      <CollectionForm
        onSubmit={onSubmit}
        onCancel={onCancel}
        isSubmitting={false}
        submitLabel="Create"
      />
    );

    const nameInput = screen.getByLabelText(/Collection Name/i);
    await user.type(nameInput, "Alpha");
    await user.click(screen.getByRole("button", { name: /Lock icon seed/i }));
    await user.type(nameInput, "Beta");
    await user.click(screen.getByRole("button", { name: "Create" }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalled();
    });
    const payload = onSubmit.mock.calls[0][0] as { avatar?: string; name: string };
    expect(payload.name).toBe("AlphaBeta");
    expect(payload.avatar).toContain('data-seed="Alpha-');
    expect(payload.avatar).not.toContain('data-seed="AlphaBeta-');
  });

  it("Arrange: seed is locked; Act: inspect refresh control; Assert: new-pattern button is disabled", async () => {
    const user = userEvent.setup();
    render(
      <CollectionForm
        onSubmit={onSubmit}
        onCancel={onCancel}
        isSubmitting={false}
        submitLabel="Create"
      />
    );

    const nameInput = screen.getByLabelText(/Collection Name/i);
    await user.type(nameInput, "Alpha");
    await user.click(screen.getByRole("button", { name: /Lock icon seed/i }));

    const refreshButton = screen.getByRole("button", { name: "New pattern" });
    expect(refreshButton).toBeDisabled();
  });

  it("Arrange: uploaded image selected; Act: submit; Assert: onSubmit includes uploaded avatar data URL", async () => {
    const user = userEvent.setup();
    render(
      <CollectionForm
        onSubmit={onSubmit}
        onCancel={onCancel}
        isSubmitting={false}
        submitLabel="Create"
      />
    );

    const nameInput = screen.getByLabelText(/Collection Name/i);
    await user.type(nameInput, "Upload Test");
    const input = screen.getByLabelText(/Choose image/i) as HTMLInputElement;
    const file = new File(["file-bytes"], "icon.png", { type: "image/png" });
    await user.upload(input, file);
    await user.click(screen.getByRole("button", { name: "Create" }));

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalled();
    });
    const payload = onSubmit.mock.calls[0][0] as { avatar?: string };
    expect(payload.avatar?.startsWith("data:image/png;base64,")).toBe(true);
  });
});
