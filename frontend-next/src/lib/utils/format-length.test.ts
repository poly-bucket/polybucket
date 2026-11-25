import { describe, it, expect } from "vitest";
import { formatMillimeters, formatNozzleDiameter } from "./format-length";

describe("format-length", () => {
  it("formats metric millimeters", () => {
    expect(formatMillimeters(250, "metric")).toBe("250 mm");
  });

  it("formats imperial inches", () => {
    expect(formatMillimeters(25.4, "imperial")).toBe("1.00 in");
  });

  it("formats default nozzle per system", () => {
    expect(formatNozzleDiameter("metric")).toBe("0.4 mm");
    expect(formatNozzleDiameter("imperial")).toBe("0.016 in");
  });
});