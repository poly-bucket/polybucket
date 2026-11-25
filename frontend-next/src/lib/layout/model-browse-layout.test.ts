import { describe, it, expect } from "vitest";
import { resolveModelBrowseLayout } from "./model-browse-layout";

describe("resolveModelBrowseLayout", () => {
  it("returns default medium card and normal spacing with lg:grid-cols-4", () => {
    const layout = resolveModelBrowseLayout({});

    expect(layout.viewType).toBe("grid");
    expect(layout.cardSize).toBe("medium");
    expect(layout.gapClassName).toBe("gap-4");
    expect(layout.containerClassName).toContain("grid-cols-2");
    expect(layout.containerClassName).toContain("sm:grid-cols-3");
    expect(layout.containerClassName).toContain("lg:grid-cols-4");
  });

  it("maps spacing to gap classes", () => {
    expect(resolveModelBrowseLayout({ cardSpacing: "compact" }).gapClassName).toBe(
      "gap-2"
    );
    expect(resolveModelBrowseLayout({ cardSpacing: "spacious" }).gapClassName).toBe(
      "gap-6"
    );
  });

  it("clamps invalid card size and spacing to defaults", () => {
    const layout = resolveModelBrowseLayout({
      cardSize: "huge",
      cardSpacing: "wide",
      gridColumns: 99,
    });

    expect(layout.cardSize).toBe("medium");
    expect(layout.gapClassName).toBe("gap-4");
    expect(layout.containerClassName).toContain("lg:grid-cols-4");
  });

  it("uses lg grid column lookup for allowed values", () => {
    expect(resolveModelBrowseLayout({ gridColumns: 6 }).containerClassName).toContain(
      "lg:grid-cols-6"
    );
    expect(resolveModelBrowseLayout({ gridColumns: 2 }).containerClassName).toContain(
      "lg:grid-cols-2"
    );
  });

  it("maps card sizes small and large", () => {
    expect(resolveModelBrowseLayout({ cardSize: "small" }).cardSize).toBe("small");
    expect(resolveModelBrowseLayout({ cardSize: "large" }).cardSize).toBe("large");
  });

  it("uses flex column container when dashboard view type is list", () => {
    const layout = resolveModelBrowseLayout({ dashboardViewType: "list" });

    expect(layout.viewType).toBe("list");
    expect(layout.containerClassName).toBe("flex flex-col");
    expect(layout.containerClassName).not.toContain("grid");
  });

  it("clamps invalid dashboard view type to grid", () => {
    const layout = resolveModelBrowseLayout({ dashboardViewType: "table" });

    expect(layout.viewType).toBe("grid");
    expect(layout.containerClassName).toContain("grid-cols-2");
  });
});