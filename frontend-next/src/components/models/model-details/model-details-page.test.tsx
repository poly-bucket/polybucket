import { describe, it, expect } from "vitest";
import type { Model } from "@/lib/api/client";
import { buildCarouselItems } from "./model-details-page";

describe("buildCarouselItems", () => {
  it("does not add a non-image (STL) thumbnail as an image item", () => {
    const model = {
      id: "m1",
      thumbnailUrl:
        "http://localhost:9000/bucket/models/m1/part.stl?X-Amz-Signature=abc",
      files: [],
    } as unknown as Model;

    const items = buildCarouselItems(model);

    expect(items.find((i) => i.id === "thumbnail")).toBeUndefined();
  });

  it("adds an image thumbnail as the first image item", () => {
    const model = {
      id: "m1",
      thumbnailUrl:
        "http://localhost:9000/bucket/models/m1/preview.png?X-Amz-Signature=abc",
      files: [],
    } as unknown as Model;

    const items = buildCarouselItems(model);

    const thumb = items.find((i) => i.id === "thumbnail");
    expect(thumb).toBeDefined();
    expect(thumb?.type).toBe("image");
  });

  it("adds 3D files as 3d items", () => {
    const model = {
      id: "m1",
      thumbnailUrl: undefined,
      files: [
        { id: "f1", name: "part.stl", mimeType: "application/octet-stream" },
      ],
    } as unknown as Model;

    const items = buildCarouselItems(model);

    const threeD = items.find((i) => i.id === "f1");
    expect(threeD).toBeDefined();
    expect(threeD?.type).toBe("3d");
    expect(threeD?.fileName).toBe("part.stl");
  });
});
