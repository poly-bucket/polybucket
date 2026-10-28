import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import {
  readSealedSession,
  refreshSessionPayload,
  sealTokensInResponse,
} from "@/lib/bff/authOperations";
import { clearSessionCookie } from "@/lib/bff/cookies";
import { toClientJson } from "@/lib/bff/userProfile";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const session = await readSealedSession(request);
  if (!session?.refreshToken) {
    const res = NextResponse.json({ message: "Unauthorized" }, { status: 401 });
    clearSessionCookie(res);
    return res;
  }
  const refreshed = await refreshSessionPayload(session.refreshToken);
  if (!refreshed.ok) {
    const res = NextResponse.json({ message: "Unauthorized" }, { status: 401 });
    clearSessionCookie(res);
    return res;
  }
  const res = NextResponse.json({ user: toClientJson(refreshed.profile).user });
  await sealTokensInResponse(
    res,
    refreshed.payload.accessToken,
    refreshed.payload.refreshToken,
    refreshed.refreshExpiresAt,
    refreshed.payload.roles
  );
  return res;
}
