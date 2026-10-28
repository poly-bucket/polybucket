import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import {
  ensureFreshAccessToken,
  hydrateProfileFromMe,
  readSealedSession,
} from "@/lib/bff/authOperations";
import { appendSessionCookie, clearSessionCookie, refreshMaxAgeFromIso } from "@/lib/bff/cookies";
import { toClientJson } from "@/lib/bff/userProfile";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const session = await readSealedSession(request);
  if (!session) {
    return NextResponse.json({ message: "Unauthorized" }, { status: 401 });
  }
  const fresh = await ensureFreshAccessToken(session);
  if (!fresh.ok) {
    const res = NextResponse.json({ message: "Unauthorized" }, { status: 401 });
    clearSessionCookie(res);
    return res;
  }
  const profile = await hydrateProfileFromMe(fresh.session.accessToken);
  if (!profile) {
    const res = NextResponse.json({ message: "Unauthorized" }, { status: 401 });
    clearSessionCookie(res);
    return res;
  }
  const res = NextResponse.json({ user: toClientJson(profile).user });
  if (fresh.rotated) {
    const maxAge = refreshMaxAgeFromIso(fresh.refreshExpiresAt);
    await appendSessionCookie(
      res,
      { ...fresh.session, roles: profile.roles ?? fresh.session.roles },
      maxAge
    );
  }
  return res;
}
