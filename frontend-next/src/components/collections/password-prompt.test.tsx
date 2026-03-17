import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import { PasswordPrompt } from "./password-prompt";

describe("PasswordPrompt", () => {
  it("does not render when open is false", () => {
    render(
      <PasswordPrompt
        open={false}
        collectionName="Secret"
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("renders dialog with collection name when open", () => {
    render(
      <PasswordPrompt
        open={true}
        collectionName="Secret Collection"
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByText(/Secret Collection/)).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Enter password")).toBeInTheDocument();
  });

  it("disables submit when password is empty", () => {
    render(
      <PasswordPrompt
        open={true}
        collectionName="Secret"
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      />
    );
    expect(screen.getByRole("button", { name: /access collection/i })).toBeDisabled();
  });

  it("calls onSubmit with password when form is submitted", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    render(
      <PasswordPrompt
        open={true}
        collectionName="Secret"
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      />
    );
    await user.type(screen.getByPlaceholderText("Enter password"), "mypass");
    await user.click(screen.getByRole("button", { name: /access collection/i }));

    expect(onSubmit).toHaveBeenCalledWith("mypass");
  });

  it("calls onCancel when Cancel is clicked", async () => {
    const user = userEvent.setup();
    const onCancel = vi.fn();
    render(
      <PasswordPrompt
        open={true}
        collectionName="Secret"
        onSubmit={vi.fn()}
        onCancel={onCancel}
      />
    );
    await user.click(screen.getByRole("button", { name: /cancel/i }));
    expect(onCancel).toHaveBeenCalled();
  });

  it("displays error when error prop is provided", () => {
    render(
      <PasswordPrompt
        open={true}
        collectionName="Secret"
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
        error="Incorrect password"
      />
    );
    expect(screen.getByText("Incorrect password")).toBeInTheDocument();
  });
});
