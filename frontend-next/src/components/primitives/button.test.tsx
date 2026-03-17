import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import { Button } from "./button";

describe("Button", () => {
  it("renders with text", () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole("button", { name: /click me/i })).toBeInTheDocument();
  });

  it("calls onClick when clicked", async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(<Button onClick={onClick}>Click</Button>);
    await user.click(screen.getByRole("button", { name: /click/i }));
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("does not call onClick when disabled", async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(
      <Button onClick={onClick} disabled>
        Click
      </Button>
    );
    await user.click(screen.getByRole("button", { name: /click/i }));
    expect(onClick).not.toHaveBeenCalled();
  });

  it("applies variant via data-variant", () => {
    render(<Button variant="destructive">Delete</Button>);
    expect(screen.getByRole("button")).toHaveAttribute("data-variant", "destructive");
  });

  it("applies size via data-size", () => {
    render(<Button size="lg">Large</Button>);
    expect(screen.getByRole("button")).toHaveAttribute("data-size", "lg");
  });

  it("renders as child when asChild", () => {
    render(
      <Button asChild>
        <a href="/test">Link</a>
      </Button>
    );
    const link = screen.getByRole("link", { name: /link/i });
    expect(link).toBeInTheDocument();
    expect(link).toHaveAttribute("href", "/test");
  });
});
