import { describe, it, expect } from "vitest";
import { getModelFileLeafName } from "./model-file-name";

describe("getModelFileLeafName", () => {
  it("strips forward-slash directory prefixes", () => {
    expect(getModelFileLeafName("images/photo.jpg")).toBe("photo.jpg");
    expect(getModelFileLeafName("files/switch_shelf.stl")).toBe("switch_shelf.stl");
  });

  it("strips backslash directory prefixes", () => {
    expect(getModelFileLeafName("images\\photo.jpg")).toBe("photo.jpg");
  });

  it("returns the name unchanged when there is no directory", () => {
    expect(getModelFileLeafName("README.txt")).toBe("README.txt");
  });
});