"use client";

/**
 * Login screen: user signs in with email or username and password.
 * When the account has 2FA enabled, the API returns `requiresTwoFactor` (POST /api/auth/login);
 * the same route is called again with the same password plus `twoFactorToken` (6-digit TOTP) or `backupCode`.
 * Optional query: `redirect` — safe internal path to open after a successful sign-in.
 */

import { useEffect, useRef, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Eye, EyeOff } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/primitives/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Input } from "@/components/primitives/input";
import { cn } from "@/lib/utils";
import type { LoginResult } from "@/contexts/AuthContext";

type Phase = "credentials" | "twoFactor";

function normalizeTotpDigits(value: string): string {
  return value.replace(/\D/g, "").slice(0, 6);
}

export default function LoginPage() {
  const [phase, setPhase] = useState<Phase>("credentials");
  const [emailOrUsername, setEmailOrUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [totpCode, setTotpCode] = useState("");
  const [backupCode, setBackupCode] = useState("");
  const [useBackupCode, setUseBackupCode] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [codeInputInvalid, setCodeInputInvalid] = useState(false);

  const totpInputRef = useRef<HTMLInputElement>(null);
  const backupInputRef = useRef<HTMLInputElement>(null);

  const { login } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectTo = searchParams.get("redirect");

  useEffect(() => {
    if (phase !== "twoFactor") {
      return;
    }
    const t = window.setTimeout(() => {
      if (useBackupCode) {
        backupInputRef.current?.focus();
      } else {
        totpInputRef.current?.focus();
      }
    }, 0);
    return () => window.clearTimeout(t);
  }, [phase, useBackupCode]);

  const navigateAfterSuccess = (result: LoginResult) => {
    if (result.requiresFirstTimeSetup) {
      router.push("/setup");
    } else if (redirectTo?.startsWith("/")) {
      router.push(redirectTo);
    } else {
      router.push("/");
    }
  };

  const handleCredentialsSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    const result = await login(emailOrUsername, password);

    setIsSubmitting(false);

    if (result.success) {
      navigateAfterSuccess(result);
      return;
    }
    if (result.requiresTwoFactor) {
      setPhase("twoFactor");
      setTotpCode("");
      setBackupCode("");
      setUseBackupCode(false);
      setCodeInputInvalid(false);
      return;
    }
    setError(result.error ?? "Login failed");
  };

  const handleTwoFactorSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setCodeInputInvalid(false);

    if (useBackupCode) {
      const trimmed = backupCode.trim();
      if (!trimmed) {
        setError("Enter a backup code.");
        setCodeInputInvalid(true);
        return;
      }
      setIsSubmitting(true);
      const result = await login(emailOrUsername, password, { backupCode: trimmed });
      setIsSubmitting(false);
      if (result.success) {
        navigateAfterSuccess(result);
        return;
      }
      if (result.requiresTwoFactor) {
        setError("Sign in again and enter your password, then the backup code.");
        return;
      }
      setError(result.error ?? "Verification failed");
      setCodeInputInvalid(true);
      return;
    }

    const digits = normalizeTotpDigits(totpCode);
    if (digits.length !== 6) {
      setError("Enter the 6-digit code from your authenticator app.");
      setCodeInputInvalid(true);
      return;
    }

    setIsSubmitting(true);
    const result = await login(emailOrUsername, password, { twoFactorToken: digits });
    setIsSubmitting(false);

    if (result.success) {
      navigateAfterSuccess(result);
      return;
    }
    if (result.requiresTwoFactor) {
      setError("Sign in again and enter your password, then the code from your app.");
      return;
    }
    setError(result.error ?? "Invalid code");
    setCodeInputInvalid(true);
  };

  const handleBackToCredentials = () => {
    setPhase("credentials");
    setTotpCode("");
    setBackupCode("");
    setUseBackupCode(false);
    setError(null);
    setCodeInputInvalid(false);
  };

  const handleTotpChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setTotpCode(normalizeTotpDigits(e.target.value));
    if (codeInputInvalid) {
      setCodeInputInvalid(false);
    }
  };

  const handleTotpPaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
    e.preventDefault();
    const text = e.clipboardData.getData("text");
    setTotpCode(normalizeTotpDigits(text));
    if (codeInputInvalid) {
      setCodeInputInvalid(false);
    }
  };

  return (
    <div className="w-full max-w-md space-y-8">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-white">Welcome Back</h1>
        {phase === "twoFactor" && (
          <p className="mt-2 text-sm text-white/60">
            Two-step verification for <span className="text-white/80">{emailOrUsername}</span>
          </p>
        )}
      </div>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle>
            {phase === "credentials" ? "Sign in" : "Authenticator code"}
          </CardTitle>
        </CardHeader>
        <CardContent>
          {phase === "credentials" ? (
            <form onSubmit={handleCredentialsSubmit} className="space-y-4">
              <div>
                <label htmlFor="emailOrUsername" className="mb-1.5 block text-sm font-medium text-white/80">
                  Email or username
                </label>
                <Input
                  id="emailOrUsername"
                  name="emailOrUsername"
                  type="text"
                  autoComplete="username"
                  placeholder="Email or Username"
                  value={emailOrUsername}
                  onChange={(e) => setEmailOrUsername(e.target.value)}
                  required
                  className="border-white/20 bg-white/5 text-white placeholder:text-white/50"
                />
              </div>
              <div>
                <label htmlFor="password" className="mb-1.5 block text-sm font-medium text-white/80">
                  Password
                </label>
                <div className="relative">
                  <Input
                    id="password"
                    name="password"
                    type={showPassword ? "text" : "password"}
                    autoComplete="current-password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    required
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
              </div>

              {error && (
                <div
                  className="rounded-md border border-red-500/50 bg-red-500/10 px-3 py-2 text-sm text-red-300"
                  role="alert"
                >
                  {error}
                </div>
              )}

              <Button type="submit" disabled={isSubmitting} className="w-full">
                {isSubmitting ? "Signing in…" : "Sign in"}
              </Button>
            </form>
          ) : (
            <form onSubmit={handleTwoFactorSubmit} className="space-y-4">
              <p className="text-sm text-white/70">
                {useBackupCode
                  ? "Enter one of your saved backup codes."
                  : "Open your authenticator app and enter the 6-digit code."}
              </p>

              {useBackupCode ? (
                <div>
                  <label htmlFor="backupCode" className="mb-1.5 block text-sm font-medium text-white/80">
                    Backup code
                  </label>
                  <Input
                    ref={backupInputRef}
                    id="backupCode"
                    name="backupCode"
                    type="text"
                    autoComplete="off"
                    value={backupCode}
                    onChange={(e) => {
                      setBackupCode(e.target.value);
                      if (codeInputInvalid) {
                        setCodeInputInvalid(false);
                      }
                    }}
                    aria-invalid={codeInputInvalid}
                    className={cn(
                      "border-white/20 bg-white/5 text-white placeholder:text-white/50",
                      codeInputInvalid && "border-red-500/60"
                    )}
                  />
                </div>
              ) : (
                <div>
                  <label htmlFor="totpCode" className="mb-1.5 block text-sm font-medium text-white/80">
                    Authentication code
                  </label>
                  <Input
                    ref={totpInputRef}
                    id="totpCode"
                    name="totpCode"
                    type="text"
                    inputMode="numeric"
                    autoComplete="one-time-code"
                    maxLength={6}
                    value={totpCode}
                    onChange={handleTotpChange}
                    onPaste={handleTotpPaste}
                    aria-invalid={codeInputInvalid}
                    className={cn(
                      "border-white/20 bg-white/5 font-mono tracking-widest text-white placeholder:text-white/50",
                      codeInputInvalid && "border-red-500/60"
                    )}
                  />
                </div>
              )}

              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  className="text-sm text-primary underline-offset-4 hover:underline"
                  onClick={() => {
                    setUseBackupCode((prev) => !prev);
                    setError(null);
                    setCodeInputInvalid(false);
                  }}
                >
                  {useBackupCode ? "Use authenticator app instead" : "Use a backup code instead"}
                </button>
              </div>

              {error && (
                <div
                  className="rounded-md border border-red-500/50 bg-red-500/10 px-3 py-2 text-sm text-red-300"
                  role="alert"
                >
                  {error}
                </div>
              )}

              <div className="flex flex-col gap-2 sm:flex-row-reverse sm:justify-between">
                <Button type="submit" disabled={isSubmitting} className="w-full sm:w-auto sm:min-w-[120px]">
                  {isSubmitting ? "Verifying…" : "Verify"}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  disabled={isSubmitting}
                  className="w-full border-white/20 bg-transparent text-white hover:bg-white/10 sm:w-auto"
                  onClick={handleBackToCredentials}
                >
                  Back
                </Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
