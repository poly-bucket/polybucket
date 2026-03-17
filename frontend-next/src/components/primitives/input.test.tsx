import { describe, it, expect } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import { Input } from "./input";

describe("Input", () => {
  it("renders with placeholder", () => {
    render(<Input placeholder="Enter text" />);
    expect(screen.getByPlaceholderText("Enter text")).toBeInTheDocument();
  });

  it("accepts user input", async () => {
    const user = userEvent.setup();
    render(<Input placeholder="Search" />);
    const input = screen.getByPlaceholderText("Search");
    await user.type(input, "hello");
    expect(input).toHaveValue("hello");
  });

  it("renders with data-slot attribute", () => {
    render(<Input placeholder="Test" />);
    expect(screen.getByPlaceholderText("Test")).toHaveAttribute("data-slot", "input");
  });

  it("supports disabled state", () => {
    render(<Input placeholder="Disabled" disabled />);
    expect(screen.getByPlaceholderText("Disabled")).toBeDisabled();
  });
});
