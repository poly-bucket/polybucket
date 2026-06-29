import { describe, it, expect } from "vitest";
import { render, screen } from "@/test/test-utils";
import type { Model } from "@/lib/api/client";
import { ModelCard } from "./model-card";

const baseModel = {
  id: "m1",
  name: "Test Model",
  author: { id: "u1", username: "alice" },
  likes: 0,
  downloads: 0,
  comments: [],
} as unknown as Model;

describe("ModelCard", () => {
  it("renders an image when the thumbnail is an image URL", () => {
    const model = {
      ...baseModel,
      thumbnailUrl: "http://localhost:9000/bucket/models/m1/preview.png?X-Amz-Signature=abc",
    } as unknown as Model;

    render(<ModelCard model={model} />);

    const img = screen.getByRole("img", { name: "Test Model" });
    expect(img).toHaveAttribute(
      "src",
      "http://localhost:9000/bucket/models/m1/preview.png?X-Amz-Signature=abc"
    );
  });

  it("renders the placeholder when the thumbnail is a 3D mesh file", () => {
    const model = {
      ...baseModel,
      thumbnailUrl: "http://localhost:9000/bucket/models/m1/part.stl?X-Amz-Signature=abc",
    } as unknown as Model;

    render(<ModelCard model={model} />);

    expect(screen.queryByRole("img")).not.toBeInTheDocument();
    expect(screen.getByText("Test Model")).toBeInTheDocument();
  });

  it("renders the placeholder when there is no thumbnail", () => {
    render(<ModelCard model={baseModel} />);

    expect(screen.queryByRole("img")).not.toBeInTheDocument();
  });
});
