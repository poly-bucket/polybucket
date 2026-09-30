"use client";

import { useState } from "react";
import Link from "next/link";
import { Button } from "@/components/primitives/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";
import { Input } from "@/components/primitives/input";
import { getAccountApiErrorMessage, requestPasswordReset } from "@/lib/services/accountEmailService";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await requestPasswordReset(email.trim());
      setSubmitted(true);
    } catch (err) {
      setError(getAccountApiErrorMessage(err, "Could not send the reset email. Try again."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="w-full max-w-md space-y-8">
      <div className="text-center">
        <h1 className="text-3xl font-semibold text-white">Forgot password</h1>
      </div>

      <Card variant="glass" className="border-white/20">
        <CardHeader>
          <CardTitle>{submitted ? "Check your inbox" : "Reset your password"}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {submitted ? (
            <p className="text-sm text-white/70" role="status">
              If an account exists for <span className="text-white">{email.trim()}</span>, a password reset
              link is on its way. The link expires in one hour. If nothing arrives, ask your server
              administrator for a reset link.
            </p>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-4">
              <p className="text-sm text-white/70">
                Enter the email address on your account and we&apos;ll send you a link to choose a new password.
              </p>
              <div>
                <label htmlFor="email" className="mb-1.5 block text-sm font-medium text-white/80">
                  Email
                </label>
                <Input
                  id="email"
                  type="email"
                  autoComplete="email"
                  placeholder="you@example.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
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
                {isSubmitting ? "Sending…" : "Send reset link"}
              </Button>
            </form>
          )}
          <p className="text-center text-sm text-white/60">
            <Link href="/login" className="text-primary underline-offset-4 hover:underline">
              Back to sign in
            </Link>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
