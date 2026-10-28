import { describe, it, expect, vi } from "vitest";
import { renderHook } from "@testing-library/react";
import { usePluginRoutes } from "./hooks";

vi.mock("@/contexts/AuthContext", () => ({
  useAuth: () => ({
    isAuthenticated: true,
    user: { id: "1", email: "a@b.c", username: "u", roles: ["User"] },
  }),
}));

vi.mock("./registry", () => ({
  getPluginRoutes: () => [
    {
      path: "open",
      component: () => null,
      protected: false,
    },
    {
      path: "admin-only",
      component: () => null,
      requiredRoles: ["Admin"],
    },
  ],
}));

describe("usePluginRoutes", () => {
  it("filters routes by required roles", () => {
    const { result } = renderHook(() => usePluginRoutes());
    expect(result.current.map((r) => r.path)).toEqual(["open"]);
  });
});
