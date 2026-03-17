import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { renderHook, act } from "@testing-library/react";
import { useDebouncedValue } from "./use-debounced-value";

describe("useDebouncedValue", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("returns initial value immediately", () => {
    const { result } = renderHook(() => useDebouncedValue("initial", 100));
    expect(result.current).toBe("initial");
  });

  it("updates to new value after delay", () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebouncedValue(value, delay),
      { initialProps: { value: "a", delay: 100 } }
    );

    expect(result.current).toBe("a");

    rerender({ value: "b", delay: 100 });
    expect(result.current).toBe("a");

    act(() => {
      vi.advanceTimersByTime(100);
    });
    expect(result.current).toBe("b");
  });

  it("cancels previous timeout when value changes before delay", () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebouncedValue(value, delay),
      { initialProps: { value: "a", delay: 100 } }
    );

    rerender({ value: "b", delay: 100 });
    act(() => {
      vi.advanceTimersByTime(50);
    });
    expect(result.current).toBe("a");

    rerender({ value: "c", delay: 100 });
    act(() => {
      vi.advanceTimersByTime(50);
    });
    expect(result.current).toBe("a");

    act(() => {
      vi.advanceTimersByTime(50);
    });
    expect(result.current).toBe("c");
  });
});
