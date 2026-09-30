"use client";

import { useEffect, useState } from "react";
import { MailWarning, X } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { Button } from "@/components/primitives/button";
import {
  getAccountApiErrorMessage,
  getEmailVerificationStatus,
  resendVerificationEmail,
  type EmailVerificationStatus,
} from "@/lib/services/accountEmailService";

export const DISMISS_STORAGE_KEY = "polybucket.emailVerificationBanner.dismissed";

type ResendState = "idle" | "sending" | "sent" | "error";

export function EmailVerificationBanner() {
  const { user, isAuthenticated } = useAuth();
  const [status, setStatus] = useState<EmailVerificationStatus | null>(null);
  const [resendState, setResendState] = useState<ResendState>("idle");
  const [resendError, setResendError] = useState<string | null>(null);
  const [dismissed, setDismissed] = useState(false);

  const accessToken = user?.accessToken;
  const knownVerified = user?.isEmailVerified === true;

  useEffect(() => {
    if (typeof window !== "undefined") {
      setDismissed(window.sessionStorage.getItem(DISMISS_STORAGE_KEY) === "1");
    }
  }, []);

  useEffect(() => {
    if (!isAuthenticated || !accessToken || knownVerified) {
      setStatus(null);
      return;
    }
    let active = true;
    getEmailVerificationStatus()
      .then((next) => {
        if (active) {
          setStatus(next);
        }
      })
      .catch(() => {
        if (active) {
          setStatus(null);
        }
      });
    return () => {
      active = false;
    };
  }, [isAuthenticated, accessToken, knownVerified]);

  if (!status || status.isEmailVerified || !status.emailDeliveryAvailable) {
    return null;
  }

  if (dismissed && !status.emailVerificationRequired) {
    return null;
  }

  const handleResend = async () => {
    setResendState("sending");
    setResendError(null);
    try {
      await resendVerificationEmail(status.email);
      setResendState("sent");
    } catch (err) {
      setResendError(getAccountApiErrorMessage(err, "Could not send the email. Try again later."));
      setResendState("error");
    }
  };

  const handleDismiss = () => {
    window.sessionStorage.setItem(DISMISS_STORAGE_KEY, "1");
    setDismissed(true);
  };

  return (
    <div
      className="border-b border-amber-500/30 bg-amber-500/10 px-4 py-3 text-sm text-amber-100"
      role="region"
      aria-label="Email verification"
    >
      <div className="mx-auto flex max-w-7xl flex-wrap items-center gap-3">
        <MailWarning className="h-4 w-4 shrink-0" />
        <p className="flex-1">
          {status.emailVerificationRequired
            ? `Verify ${status.email} to upload models, comment, and create collections.`
            : `Verify ${status.email} so you can recover your account if you lose access.`}
          {resendState === "sent" && " A new link is on its way — check your inbox."}
          {resendState === "error" && resendError && ` ${resendError}`}
        </p>
        <Button
          size="sm"
          variant="outline"
          onClick={handleResend}
          disabled={resendState === "sending" || resendState === "sent"}
          className="border-amber-400/40 bg-transparent text-amber-100 hover:bg-amber-500/20"
        >
          {resendState === "sending" ? "Sending…" : resendState === "sent" ? "Sent" : "Resend email"}
        </Button>
        {!status.emailVerificationRequired && (
          <button
            type="button"
            aria-label="Dismiss"
            onClick={handleDismiss}
            className="text-amber-100/70 transition-colors hover:text-amber-100"
          >
            <X className="h-4 w-4" />
          </button>
        )}
      </div>
    </div>
  );
}
