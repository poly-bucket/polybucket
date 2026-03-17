import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook } from "@testing-library/react";
import { useUnsavedChanges } from "./use-unsaved-changes";

vi.mock("next/navigation", () => ({
  usePathname: () => "/test",
}));

describe("useUnsavedChanges", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("adds beforeunload listener when isDirty is true", () => {
    const addSpy = vi.spyOn(window, "addEventListener");
    const removeSpy = vi.spyOn(window, "removeEventListener");

    const { unmount } = renderHook(() => useUnsavedChanges(true));

    expect(addSpy).toHaveBeenCalledWith("beforeunload", expect.any(Function));

    unmount();

    expect(removeSpy).toHaveBeenCalledWith("beforeunload", expect.any(Function));

    addSpy.mockRestore();
    removeSpy.mockRestore();
  });

  it("prevents default on beforeunload when isDirty", () => {
    const addSpy = vi.spyOn(window, "addEventListener");
    let handler: (e: BeforeUnloadEvent) => void = () => {};

    addSpy.mockImplementation((event, fn) => {
      if (event === "beforeunload") handler = fn as (e: BeforeUnloadEvent) => void;
    });

    renderHook(() => useUnsavedChanges(true));

    const event = new Event("beforeunload") as BeforeUnloadEvent;
    event.preventDefault = vi.fn();
    handler(event);

    expect(event.preventDefault).toHaveBeenCalled();

    addSpy.mockRestore();
  });

  it("does not prevent default on beforeunload when not isDirty", () => {
    const addSpy = vi.spyOn(window, "addEventListener");
    let handler: (e: BeforeUnloadEvent) => void = () => {};

    addSpy.mockImplementation((event, fn) => {
      if (event === "beforeunload") handler = fn as (e: BeforeUnloadEvent) => void;
    });

    renderHook(() => useUnsavedChanges(false));

    const event = new Event("beforeunload") as BeforeUnloadEvent;
    event.preventDefault = vi.fn();
    handler(event);

    expect(event.preventDefault).not.toHaveBeenCalled();

    addSpy.mockRestore();
  });
});
