// @vitest-environment node
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { NextRequest } from "next/server";
import { proxyRequestToApi } from "./proxyToApi";

const DEFAULT_NEXT_PROXY_BODY_LIMIT_BYTES = 10 * 1024 * 1024;

vi.mock("./env", () => ({
  getPolybucketApiUrl: () => "http://polybucket-api.test",
}));

vi.mock("./authOperations", () => ({
  readSealedSession: vi.fn(async () => null),
  ensureFreshAccessToken: vi.fn(),
}));

vi.mock("./cookies", () => ({
  appendSessionCookie: vi.fn(),
  refreshMaxAgeFromIso: vi.fn(() => 3600),
}));

async function readStreamByteLength(body: ReadableStream<Uint8Array>): Promise<number> {
  const reader = body.getReader();
  let total = 0;
  while (true) {
    const { done, value } = await reader.read();
    if (done) {
      break;
    }
    total += value.byteLength;
  }
  return total;
}

describe("proxyRequestToApi", () => {
  const originalFetch = globalThis.fetch;

  beforeEach(() => {
    vi.restoreAllMocks();
  });

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it("streams POST bodies to the API without buffering the full payload in an ArrayBuffer", async () => {
    const payloadSizeBytes = DEFAULT_NEXT_PROXY_BODY_LIMIT_BYTES + 512 * 1024;
    const payload = new Uint8Array(payloadSizeBytes).fill(97);
    let upstreamReceivedBytes = 0;

    globalThis.fetch = vi.fn(async (_url: string, init?: RequestInit) => {
      expect(init?.method).toBe("POST");
      expect(init?.duplex).toBe("half");
      expect(init?.body).toBeDefined();
      expect(init?.body).not.toBeInstanceOf(ArrayBuffer);
      upstreamReceivedBytes = await readStreamByteLength(init!.body as ReadableStream<Uint8Array>);
      return new Response(JSON.stringify({ ok: true }), {
        status: 201,
        headers: { "content-type": "application/json" },
      });
    });

    const request = new NextRequest("http://localhost:3000/api/polybucket/api/models", {
      method: "POST",
      body: new Blob([payload]),
      headers: { "content-type": "multipart/form-data; boundary=----vitest" },
    });

    const response = await proxyRequestToApi(request, "api/models");

    expect(response.status).toBe(201);
    expect(upstreamReceivedBytes).toBe(payloadSizeBytes);
    expect(globalThis.fetch).toHaveBeenCalledOnce();
    expect(String(globalThis.fetch.mock.calls[0][0])).toBe(
      "http://polybucket-api.test/api/models"
    );
  });

  it("does not send a request body or duplex option for GET requests", async () => {
    globalThis.fetch = vi.fn(async () => {
      return new Response("[]", { status: 200, headers: { "content-type": "application/json" } });
    });

    const request = new NextRequest("http://localhost:3000/api/polybucket/api/models", {
      method: "GET",
    });

    await proxyRequestToApi(request, "api/models");

    const init = globalThis.fetch.mock.calls[0][1] as RequestInit | undefined;
    expect(init?.body).toBeUndefined();
    expect(init?.duplex).toBeUndefined();
  });
});