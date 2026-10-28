import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { readSealedSession } from "@/lib/bff/authOperations";
import { apiPostJson } from "@/lib/bff/apiClient";
import { clearSessionCookie } from "@/lib/bff/cookies";

export const runtime = "nodejs";

export async function POST(request: NextRequest) {
  const session = await readSealedSession(request);
  if (session?.refreshToken) {
    await apiPostJson("/api/auth/logout", { refreshToken: session.refreshToken });
  }
  const res = new NextResponse(null, { status: 204 });
  clearSessionCookie(res);
  return res;
}
