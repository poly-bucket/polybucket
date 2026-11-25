import { describe, it, expect } from "vitest";
import { render, screen } from "@/test/test-utils";
import type { Model } from "@/lib/api/client";
import { ModelListRow } from "./model-list-row";

const baseModel = {
  id: "m1",
  name: "List Model",
  author: { id: "u1", username: "alice" },
  likes: 0,
  downloads: 0,
  comments: [],
} as unknown as Model;

describe("ModelListRow", () => {
  it("renders model title and author in list layout", () => {
    const model = {
      ...baseModel,
      thumbnailUrl: "http://localhost:9000/bucket/models/m1/preview.png?X-Amz-Signature=abc",
    } as unknown as Model;

    render(<ModelListRow model={model} />);

    expect(screen.getByText("List Model")).toBeInTheDocument();
    expect(screen.getByText("by alice")).toBeInTheDocument();
    expect(screen.getByRole("img", { name: "List Model" })).toBeInTheDocument();
  });
});