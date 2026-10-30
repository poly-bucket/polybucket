import { NextResponse } from "next/server";
import { handleRegisterBody } from "@/lib/bff/authOperations";

export const runtime = "nodejs";

export async function POST(request: Request) {
  let body: Record<string, unknown>;
  try {
    body = (await request.json()) as Record<string, unknown>;
  } catch {
    return NextResponse.json({ message: "Invalid JSON body" }, { status: 400 });
  }
  const outcome = await handleRegisterBody(body);
  return outcome.response;
}
