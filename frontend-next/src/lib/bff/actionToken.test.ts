import { describe, it, expect } from "vitest";
import { NextRequest } from "next/server";
import { redirectWithoutQueryToken } from "./actionToken";

describe("redirectWithoutQueryToken", () => {
  it("redirects and strips token query param", () => {
    const request = new NextRequest(
      "https://app.test/verify-email?token=secret&type=email-change"
    );
    const response = redirectWithoutQueryToken(request);
    expect(response).not.toBeNull();
    expect(response!.headers.get("location")).toBe(
      "https://app.test/verify-email?type=email-change"
    );
  });
});
