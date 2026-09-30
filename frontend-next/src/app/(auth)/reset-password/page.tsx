"use client";

/**
 * Landing page for password reset and account invite links (`?token=...`, optional `invite=1`).
 * The token is removed from the address bar on load and only sent with the new password.
 */

import { useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { CheckCircle2, Eye, EyeOff } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/primitives/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Input } from "@/components/primitives/input";
import {
  getAccountApiErrorMessage,
  resetPassword,
  takeTokenFromUrl,
} from "@/lib/services/accountEmailService";

const MIN_PASSWORD_LENGTH = 6;

type Phase = "loading" | "form" | "missing" | "done";

export default function ResetPasswordPage() {
  const searchParams = useSearchParams();
  const isInvite = searchParams.get("invite") === "1";
  const { isAuthenticated, logout } = useAuth();

  const [token, setToken] = useState<string | null>(null);
  const [phase, setPhase] = useState<Phase>("loading");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fromUrl = takeTokenFromUrl();
    if (fromUrl) {
      setToken(fromUrl);
      setPhase("form");
    } else {
      setPhase((current) => (current === "loading" ? "missing" : current));
    }
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token) {
      return;
    }
    setError(null);
    if (password.length < MIN_PASSWORD_LENGTH) {
      setError(`Password must be at least ${MIN_PASSWORD_LENGTH} characters.`);
      return;
    }
    if (password !== confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setIsSubmitting(true);
    try {
      await resetPassword(token, password, confirmPassword);
      setPhase("done");
      if (isAuthenticated) {
        logout();
      }
    } catch (err) {
      setError(getAccountApiErrorMessage(err, "Could not reset the password."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const title = isInvite ? "Set your password" : "Choose a new password";

  return (
    <div className="w-full max-w-md space-y-8">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-white">{isInvite ? "Welcome" : "Reset password"}</h1>
      </div>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle>{phase === "done" ? "Password updated" : title}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {phase === "loading" && <p className="text-sm text-white/70">Loading…</p>}

          {phase === "missing" && (
            <div className="space-y-4">
              <p className="text-sm text-amber-100" role="alert">
                This page needs the link from your email. Request a new one if it has expired.
              </p>
              <Button asChild className="w-full">
                <Link href="/forgot-password">Request a new link</Link>
              </Button>
            </div>
          )}

          {phase === "form" && (
            <form onSubmit={handleSubmit} className="space-y-4">
              {isInvite && (
                <p className="text-sm text-white/70">
                  An administrator created an account for you. Choose a password to finish setting it up.
                </p>
              )}
              <div>
                <label htmlFor="newPassword" className="mb-1.5 block text-sm font-medium text-white/80">
                  New password
                </label>
                <div className="relative">
                  <Input
                    id="newPassword"
                    type={showPassword ? "text" : "password"}
                    autoComplete="new-password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
                    minLength={MIN_PASSWORD_LENGTH}
                    className="border-white/20 bg-white/5 pr-10 text-white placeholder:text-white/50"
                  />
                  <button
                    type="button"
                    aria-label={showPassword ? "Hide password" : "Show password"}
                    onClick={() => setShowPassword((prev) => !prev)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-white/60 transition-colors hover:text-white"
                  >
                    {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
                <p className="mt-1 text-xs text-white/50">At least {MIN_PASSWORD_LENGTH} characters.</p>
              </div>
              <div>
                <label htmlFor="confirmPassword" className="mb-1.5 block text-sm font-medium text-white/80">
                  Confirm password
                </label>
                <Input
                  id="confirmPassword"
                  type={showPassword ? "text" : "password"}
                  autoComplete="new-password"
                  value={confirmPassword}
                  onChange={(e) => setConfirmPassword(e.target.value)}
                  required
                  className="border-white/20 bg-white/5 text-white placeholder:text-white/50"
                />
              </div>
              {error && (
                <div className="rounded-md border border-red-500/50 bg-red-500/10 px-3 py-2 text-sm text-red-300" role="alert">
                  {error}
                </div>
              )}
              <Button type="submit" disabled={isSubmitting} className="w-full">
                {isSubmitting ? "Saving…" : isInvite ? "Set password" : "Update password"}
              </Button>
            </form>
          )}

          {phase === "done" && (
            <div className="space-y-4">
              <div className="flex items-start gap-3 rounded-md border border-green-500/40 bg-green-500/10 px-3 py-2 text-sm text-green-200" role="status">
                <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0" />
                <span>Your password has been updated and you have been signed out of every device.</span>
              </div>
              <Button asChild className="w-full">
                <Link href="/login">Sign in</Link>
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
