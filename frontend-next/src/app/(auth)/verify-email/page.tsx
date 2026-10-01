"use client";

/**
 * Landing page for email verification and email-change links (`?token=...`, optional `type=email-change`).
 * The token is removed from the address bar on load and only sent when the user clicks confirm,
 * so link scanners that prefetch URLs cannot consume it.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { CheckCircle2, MailWarning } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { getOrCreateRefreshPromise } from "@/lib/auth/authSession";
import { Button } from "@/components/primitives/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Skeleton } from "@/components/ui/glass/skeleton";
import { Input } from "@/components/primitives/input";
import {
  getAccountApiErrorMessage,
  resendVerificationEmail,
  takeTokenFromUrl,
  verifyEmail,
  type VerifyEmailResult,
} from "@/lib/services/accountEmailService";

type Phase = "loading" | "ready" | "missing" | "verifying" | "done" | "failed";

export default function VerifyEmailPage() {
  const searchParams = useSearchParams();
  const isEmailChange = searchParams.get("type") === "email-change";
  const { user, isAuthenticated, refreshUserFromMe } = useAuth();

  const [token, setToken] = useState<string | null>(null);
  const [phase, setPhase] = useState<Phase>("loading");
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<VerifyEmailResult | null>(null);
  const [resendEmail, setResendEmail] = useState("");
  const [resendState, setResendState] = useState<"idle" | "sending" | "sent">("idle");

  useEffect(() => {
    const fromUrl = takeTokenFromUrl();
    if (fromUrl) {
      setToken(fromUrl);
      setPhase("ready");
    } else {
      setPhase((current) => (current === "loading" ? "missing" : current));
    }
  }, []);

  useEffect(() => {
    if (user?.email && !resendEmail) {
      setResendEmail(user.email);
    }
  }, [user?.email, resendEmail]);

  const handleConfirm = async () => {
    if (!token) {
      return;
    }
    setPhase("verifying");
    setError(null);
    try {
      const verified = await verifyEmail(token);
      setResult(verified);
      setPhase("done");
      if (isAuthenticated) {
        await getOrCreateRefreshPromise();
        await refreshUserFromMe();
      }
    } catch (err) {
      setError(getAccountApiErrorMessage(err, "This link could not be verified."));
      setPhase("failed");
    }
  };

  const handleResend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resendEmail.trim()) {
      return;
    }
    setResendState("sending");
    await resendVerificationEmail(resendEmail.trim()).catch(() => undefined);
    setResendState("sent");
  };

  const title = isEmailChange ? "Confirm your new email" : "Verify your email";

  return (
    <div className="w-full max-w-md space-y-8">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-white">{title}</h1>
      </div>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle>
            {phase === "done" ? (isEmailChange ? "Email changed" : "Email verified") : title}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {phase === "loading" && (
            <div className="space-y-3">
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-10 w-full rounded-md" />
            </div>
          )}

          {(phase === "ready" || phase === "verifying") && (
            <>
              <p className="text-sm text-white/70">
                {isEmailChange
                  ? "Click below to finish changing the email address on your account."
                  : "Click below to confirm this email address belongs to you."}
              </p>
              <Button onClick={handleConfirm} disabled={phase === "verifying"} className="w-full">
                {phase === "verifying"
                  ? "Confirming…"
                  : isEmailChange
                    ? "Confirm new email"
                    : "Verify email"}
              </Button>
            </>
          )}

          {phase === "done" && result && (
            <div className="space-y-4">
              <div className="flex items-start gap-3 rounded-md border border-green-500/40 bg-green-500/10 px-3 py-2 text-sm text-green-200" role="status">
                <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
                <span>
                  {result.purpose === "ChangeAddress"
                    ? `Your account email is now ${result.email}.`
                    : `${result.email} is verified. You're all set.`}
                </span>
              </div>
              <Button asChild className="w-full">
                <Link href={isAuthenticated ? "/" : "/login"}>
                  {isAuthenticated ? "Continue" : "Sign in"}
                </Link>
              </Button>
            </div>
          )}

          {(phase === "missing" || phase === "failed") && (
            <div className="space-y-4">
              <div className="flex items-start gap-3 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-100" role="alert">
                <MailWarning className="mt-0.5 h-4 w-4 shrink-0" />
                <span>{error ?? "This page needs the link from your verification email."}</span>
              </div>

              {!isEmailChange && (
                <form onSubmit={handleResend} className="space-y-3">
                  <label htmlFor="resendEmail" className="block text-sm font-medium text-white/80">
                    Send a new verification link
                  </label>
                  <Input
                    id="resendEmail"
                    type="email"
                    autoComplete="email"
                    placeholder="you@example.com"
                    value={resendEmail}
                    onChange={(e) => {
                      setResendEmail(e.target.value);
                      setResendState("idle");
                    }}
                    required
                    className="border-white/20 bg-white/5 text-white placeholder:text-white/50"
                  />
                  <Button type="submit" variant="outline" disabled={resendState === "sending"} className="w-full border-white/20 bg-transparent text-white hover:bg-white/10">
                    {resendState === "sending" ? "Sending…" : "Send new link"}
                  </Button>
                  {resendState === "sent" && (
                    <p className="text-sm text-white/70" role="status">
                      If that address belongs to an unverified account, a new link is on its way.
                    </p>
                  )}
                </form>
              )}

              <p className="text-center text-sm text-white/60">
                <Link href="/login" className="text-primary underline-offset-4 hover:underline">
                  Back to sign in
                </Link>
              </p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
