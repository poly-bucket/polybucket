import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CollectionForm } from "./collection-form";

vi.mock("@/lib/avatar/minidenticon", async (importOriginal) => {
  const mod = await importOriginal<typeof import("@/lib/avatar/minidenticon")>();
  return {
    ...mod,
    minidenticonSvg: () => "<svg xmlns=\"http://www.w3.org/2000/svg\"/>",
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
});
