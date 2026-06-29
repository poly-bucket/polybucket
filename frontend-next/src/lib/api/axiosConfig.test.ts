import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { AxiosError, type AxiosResponse } from "axios";
import axiosInstance from "./axiosConfig";

function makeUnauthorizedError(headers: Record<string, string>): AxiosError {
  const response = {
    status: 401,
    statusText: "Unauthorized",
    headers,
    data: {},
    config: { headers: {} },
  } as unknown as AxiosResponse;
  return new AxiosError(
    "Unauthorized",
    "ERR_BAD_REQUEST",
    { headers: {} } as never,
    {},
    response
  );
}

const assignMock = vi.fn();
const originalLocation = window.location;
const originalAdapter = axiosInstance.defaults.adapter;

beforeEach(() => {
  window.localStorage.clear();
  assignMock.mockClear();
  Object.defineProperty(window, "location", {
    configurable: true,
    value: { pathname: "/models/123", assign: assignMock },
  });
});

afterEach(() => {
  Object.defineProperty(window, "location", {
    configurable: true,
    value: originalLocation,
  });
  axiosInstance.defaults.adapter = originalAdapter;
});

describe("axios response interceptor - private site redirect", () => {
  it("redirects an anonymous user to /login on a marked 401", async () => {
    // Arrange
    axiosInstance.defaults.adapter = async () => {
      throw makeUnauthorizedError({ "x-site-access": "login-required" });
    };

    // Act
    await expect(axiosInstance.get("/api/models")).rejects.toBeTruthy();

    // Assert
    expect(assignMock).toHaveBeenCalledWith("/login?redirect=%2Fmodels%2F123");
  });

  it("does not redirect on an ordinary 401 without the marker", async () => {
    // Arrange
    axiosInstance.defaults.adapter = async () => {
      throw makeUnauthorizedError({ "content-type": "application/json" });
    };

    // Act
    await expect(axiosInstance.get("/api/models")).rejects.toBeTruthy();

    // Assert
    expect(assignMock).not.toHaveBeenCalled();
  });

  it("does not redirect when already on an auth page (no loop)", async () => {
    // Arrange
    Object.defineProperty(window, "location", {
      configurable: true,
      value: { pathname: "/login", assign: assignMock },
    });
    axiosInstance.defaults.adapter = async () => {
      throw makeUnauthorizedError({ "x-site-access": "login-required" });
    };

    // Act
    await expect(axiosInstance.get("/api/models")).rejects.toBeTruthy();

    // Assert
    expect(assignMock).not.toHaveBeenCalled();
  });
});
