import type { NextRequest } from "next/server";
import { NextResponse } from "next/server";
import { AUTH_ACTION_TOKEN_COOKIE } from "@/lib/auth/authConstants";
import { clearActionTokenCookie } from "@/lib/bff/actionToken";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const token = request.cookies.get(AUTH_ACTION_TOKEN_COOKIE)?.value ?? null;
  const res = NextResponse.json({ token });
  if (token) {
    clearActionTokenCookie(res);
  }
  return res;
}
