import { test, expect } from "@playwright/test";
import path from "node:path";
import { hasE2ECredentials, loginAsTestUser } from "./loginTestUser";

const DEFAULT_NEXT_PROXY_BODY_LIMIT_BYTES = 10 * 1024 * 1024;
const FIXTURE_STL = path.join(__dirname, "fixtures", "cube.stl");

const FORM_READ_FAILURE_PATTERNS = [
  /Failed to read the request form/i,
  /Unexpected end of Stream/i,
];

test.describe("BFF large model upload", () => {
  test("proxies multipart payloads larger than 10MB without truncating the body", async ({ page }) => {
    test.skip(!hasE2ECredentials(), "Set E2E_LOGIN_EMAIL and E2E_LOGIN_PASSWORD");

    test.setTimeout(180_000);

    const loggedIn = await loginAsTestUser(page);
    expect(loggedIn).toBe(true);

    const paddingBytes = DEFAULT_NEXT_PROXY_BODY_LIMIT_BYTES + 256 * 1024;
    const padding = Buffer.alloc(paddingBytes, 0x61);
    const stlBody = await import("node:fs/promises").then((fs) => fs.readFile(FIXTURE_STL));

    const response = await page.request.post("/api/polybucket/api/models", {
      multipart: {
        name: `e2e-large-upload-${Date.now()}`,
        license: "MIT",
        privacy: "public",
        aiGenerated: "false",
        workInProgress: "false",
        nSFW: "false",
        remix: "false",
        files: {
          name: "large-bundle.stl",
          mimeType: "model/stl",
          buffer: Buffer.concat([stlBody, padding]),
        },
      },
    });

    const responseText = await response.text();
    for (const pattern of FORM_READ_FAILURE_PATTERNS) {
      expect(responseText).not.toMatch(pattern);
    }

    expect(response.status()).toBe(201);
  });
});