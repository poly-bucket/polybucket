import { describe, it, expect } from "vitest";
import { render, screen } from "@/test/test-utils";
import { MarkdownContent } from "./markdown-content";

describe("MarkdownContent", () => {
  it("does not render javascript links", () => {
    render(<MarkdownContent content={"[click](javascript:alert(1))"} />);
    expect(screen.queryByRole("link")).not.toBeInTheDocument();
    expect(screen.getByText("click")).toBeInTheDocument();
  });

  it("renders safe https links", () => {
    render(<MarkdownContent content={"[docs](https://example.com/doc)"} />);
    const link = screen.getByRole("link", { name: "docs" });
    expect(link).toHaveAttribute("href", "https://example.com/doc");
    expect(link).toHaveAttribute("rel", "noopener noreferrer");
  });
});
