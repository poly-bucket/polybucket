import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { getPolybucketApiUrl } from "./env";
import {
  ensureFreshAccessToken,
  readSealedSession,
} from "./authOperations";
import { appendSessionCookie, refreshMaxAgeFromIso } from "./cookies";

const HOP_BY_HOP = new Set([
  "connection",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailers",
  "transfer-encoding",
  "upgrade",
  "host",
  "content-length",
]);

const FORWARD_RESPONSE_HEADERS = ["content-type", "content-disposition", "x-site-access"];

export async function proxyRequestToApi(
  request: NextRequest,
  apiPath: string
): Promise<NextResponse> {
  const sealed = await readSealedSession(request);
  let accessToken: string | undefined;
  let sessionUpdate: { payload: Awaited<ReturnType<typeof ensureFreshAccessToken>> } | null = null;

  if (sealed) {
    const fresh = await ensureFreshAccessToken(sealed);
    if (!fresh.ok) {
      return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
    }
    accessToken = fresh.session.accessToken;
    if (fresh.rotated) {
      sessionUpdate = { payload: fresh };
    }
  }

  const base = getPolybucketApiUrl();
  const query = request.nextUrl.search;
  const segments = apiPath.split("/").filter(Boolean);
  const normalized =
    segments[0] === "api" ? segments.slice(1).join("/") : segments.join("/");
  const targetUrl = `${base}/api/${normalized}${query}`;

  const headers = new Headers();
  request.headers.forEach((value, key) => {
    const lower = key.toLowerCase();
    if (HOP_BY_HOP.has(lower)) {
      return;
    }
    if (lower === "authorization") {
      return;
    }
    headers.set(key, value);
  });
  if (accessToken) {
    headers.set("Authorization", `Bearer ${accessToken}`);
  }

  const method = request.method;
  const hasBody = method !== "GET" && method !== "HEAD";

  if (hasBody && normalized.includes("models")) {
    // #region agent log
    fetch("http://127.0.0.1:7425/ingest/0e4469e7-8733-4f5f-86f7-c22a81b7a59f", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Debug-Session-Id": "d87db7",
      },
      body: JSON.stringify({
        sessionId: "d87db7",
        runId: "pre-fix",
        hypothesisId: "B",
        location: "proxyToApi.ts:proxy",
        message: "Proxying model upload",
        data: {
          targetUrl,
          clientContentLength: request.headers.get("content-length"),
          contentType: request.headers.get("content-type"),
          streamingBody: true,
        },
        timestamp: Date.now(),
      }),
    }).catch(() => {});
    // #endregion
  }

  const upstream = await fetch(targetUrl, {
    method,
    headers,
    body: hasBody ? request.body : undefined,
    duplex: hasBody ? "half" : undefined,
    cache: "no-store",
  } as RequestInit);

  const responseHeaders = new Headers();
  for (const name of FORWARD_RESPONSE_HEADERS) {
    const value = upstream.headers.get(name);
    if (value) {
      responseHeaders.set(name, value);
    }
  }

  const responseBody = await upstream.arrayBuffer();
  if (normalized.includes("models") && method === "POST") {
    // #region agent log
    fetch("http://127.0.0.1:7425/ingest/0e4469e7-8733-4f5f-86f7-c22a81b7a59f", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-Debug-Session-Id": "d87db7",
      },
      body: JSON.stringify({
        sessionId: "d87db7",
        runId: "pre-fix",
        hypothesisId: "B",
        location: "proxyToApi.ts:response",
        message: "Model upload upstream response",
        data: {
          status: upstream.status,
          bodyPreview: new TextDecoder().decode(responseBody.slice(0, 240)),
        },
        timestamp: Date.now(),
      }),
    }).catch(() => {});
    // #endregion
  }
  const response = new NextResponse(responseBody, {
    status: upstream.status,
    statusText: upstream.statusText,
    headers: responseHeaders,
  });

  if (sessionUpdate?.payload.ok && sessionUpdate.payload.rotated) {
    const maxAge = refreshMaxAgeFromIso(sessionUpdate.payload.refreshExpiresAt);
    await appendSessionCookie(response, sessionUpdate.payload.session, maxAge);
  }

  return response;
}
