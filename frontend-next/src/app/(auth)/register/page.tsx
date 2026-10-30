"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Eye, EyeOff } from "lucide-react";
import { Button } from "@/components/primitives/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Input } from "@/components/primitives/input";
import { AuthCardSkeleton } from "@/components/ui/skeletons";
import { persistUser } from "@/lib/auth/authSession";
import {
  fetchRegistrationStatus,
  getRegisterApiErrorMessage,
} from "@/lib/services/registrationService";
import type { AuthUser } from "@/lib/auth/authSession";

type Phase = "loading" | "closed" | "form" | "verifyEmail";

export default function RegisterPage() {
  const router = useRouter();
  const [phase, setPhase] = useState<Phase>("loading");
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let active = true;
    void fetchRegistrationStatus().then((allowed) => {
      if (!active) {
        return;
      }
      setPhase(allowed ? "form" : "closed");
    });
    return () => {
      active = false;
    };
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const res = await fetch("/api/auth/register", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json", Accept: "application/json" },
        body: JSON.stringify({
          email: email.trim(),
          username: username.trim(),
          password,
          confirmPassword,
          userAgent: typeof navigator !== "undefined" ? navigator.userAgent : "",
        }),
      });
      const data = (await res.json()) as {
        requiresEmailVerification?: boolean;
        user?: AuthUser;
        message?: string;
      };
      if (!res.ok) {
        setError(data.message ?? "Registration failed");
        return;
      }
      if (data.requiresEmailVerification) {
        setPhase("verifyEmail");
        return;
      }
      if (data.user?.id) {
        persistUser(data.user);
        router.push("/");
        return;
      }
      setError("Invalid response from server");
    } catch (err) {
      setError(getRegisterApiErrorMessage(err, "Registration failed"));
    } finally {
      setIsSubmitting(false);
    }
  };

  if (phase === "loading") {
    return (
      <div className="w-full max-w-md space-y-8">
        <AuthCardSkeleton />
      </div>
    );
  }

  if (phase === "closed") {
    return (
      <div className="w-full max-w-md space-y-8">
        <div className="text-center">
          <h1 className="text-3xl font-semibold text-white">Registration closed</h1>
          <p className="mt-2 text-sm text-white/60">
            New accounts are not open on this instance. Contact an administrator if you need access.
          </p>
        </div>
        <Card variant="glass" className="border-white/20">
          <CardContent className="py-6 text-center">
            <Button asChild variant="outline" className="border-white/20">
              <Link href="/login">Sign in</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (phase === "verifyEmail") {
    return (
      <div className="w-full max-w-md space-y-8">
        <div className="text-center">
          <h1 className="text-3xl font-semibold text-white">Check your email</h1>
          <p className="mt-2 text-sm text-white/60">
            We sent a verification link to <span className="text-white/80">{email}</span>. Open it to
            activate your account, then sign in.
          </p>
        </div>
        <Card variant="glass" className="border-white/20">
          <CardContent className="py-6">
            <Button asChild className="w-full">
              <Link href="/login">Sign in</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md space-y-8">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-white">Create account</h1>
        <p className="mt-2 text-sm text-white/60">
          Already have an account?{" "}
          <Link href="/login" className="text-primary underline-offset-4 hover:underline">
            Sign in
          </Link>
        </p>
      </div>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle>Sign up</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="email" className="mb-1.5 block text-sm font-medium text-white/80">
                Email
              </label>
              <Input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="border-white/20 bg-white/5 text-white placeholder:text-white/50"
              />
            </div>
            <div>
              <label htmlFor="username" className="mb-1.5 block text-sm font-medium text-white/80">
                Username
              </label>
              <Input
                id="username"
                name="username"
                type="text"
                autoComplete="username"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                minLength={3}
                maxLength={50}
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
                  autoComplete="new-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  minLength={6}
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
            <div>
              <label
                htmlFor="confirmPassword"
                className="mb-1.5 block text-sm font-medium text-white/80"
              >
                Confirm password
              </label>
              <Input
                id="confirmPassword"
                name="confirmPassword"
                type={showPassword ? "text" : "password"}
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                minLength={6}
                className="border-white/20 bg-white/5 text-white placeholder:text-white/50"
              />
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
              {isSubmitting ? "Creating account…" : "Create account"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
