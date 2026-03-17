"use client";

import { useState, useEffect } from "react";
import { useRef } from "react";
import { Eye, EyeOff, CheckCircle, AlertCircle } from "lucide-react";
import QRCode from "react-qr-code";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import { cn } from "@/lib/utils";
import {
  twoFactorAuthService,
  type TwoFactorAuthStatus,
  type InitializeTwoFactorAuthResponse,
} from "@/services/twoFactorAuthService";

function Dialog({
  open,
  onClose,
  title,
  children,
  className,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  className?: string;
}) {
  const dialogRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    if (open) window.addEventListener("keydown", handleEscape);
    return () => window.removeEventListener("keydown", handleEscape);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div
        className="absolute inset-0 bg-black/40 backdrop-blur-sm"
        onClick={onClose}
        aria-hidden
      />
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="dialog-title"
        className={cn(
          "relative z-50 w-full max-w-md rounded-xl border border-white/20 bg-white/10 p-6 shadow-xl backdrop-blur-md mx-4",
          className
        )}
      >
        <h3 id="dialog-title" className="mb-4 text-lg font-semibold text-white">
          {title}
        </h3>
        {children}
      </div>
    </div>
  );
}

export function TwoFactorAuth() {
  const [status, setStatus] = useState<TwoFactorAuthStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [showInitDialog, setShowInitDialog] = useState(false);
  const [initData, setInitData] = useState<InitializeTwoFactorAuthResponse | null>(null);
  const [initLoading, setInitLoading] = useState(false);

  const [showEnableDialog, setShowEnableDialog] = useState(false);
  const [enableToken, setEnableToken] = useState("");
  const [enableLoading, setEnableLoading] = useState(false);
  const [showToken, setShowToken] = useState(false);

  const [enabledBackupCodes, setEnabledBackupCodes] = useState<string[]>([]);
  const [showBackupCodesDialog, setShowBackupCodesDialog] = useState(false);
  const [copiedIndex, setCopiedIndex] = useState<number | null>(null);

  const [showDisableDialog, setShowDisableDialog] = useState(false);
  const [disableLoading, setDisableLoading] = useState(false);

  const loadStatus = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await twoFactorAuthService.getStatus();
      setStatus(data);
    } catch (err) {
      setError("Failed to load 2FA status");
      console.error("Error loading 2FA status:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStatus();
  }, []);

  const handleInitialize = async () => {
    try {
      setInitLoading(true);
      setError(null);
      const data = await twoFactorAuthService.initialize();
      setInitData(data);
      setShowInitDialog(true);
    } catch (err) {
      setError("Failed to initialize 2FA");
      console.error("Error initializing 2FA:", err);
    } finally {
      setInitLoading(false);
    }
  };

  const handleEnable = async () => {
    if (!enableToken.trim()) {
      setError("Please enter a valid 2FA token");
      return;
    }
    try {
      setEnableLoading(true);
      setError(null);
      const response = await twoFactorAuthService.enable(enableToken);
      if (response.success) {
        setSuccess("Two-factor authentication enabled successfully");
        setShowEnableDialog(false);
        setEnableToken("");
        if (response.backupCodes?.length) {
          setEnabledBackupCodes(response.backupCodes);
          setShowBackupCodesDialog(true);
        }
        await loadStatus();
      } else {
        setError(response.message || "Failed to enable 2FA");
      }
    } catch (err) {
      setError("Failed to enable 2FA. Please check your token and try again.");
      console.error("Error enabling 2FA:", err);
    } finally {
      setEnableLoading(false);
    }
  };

  const handleDisable = async () => {
    try {
      setDisableLoading(true);
      setError(null);
      await twoFactorAuthService.disable();
      setSuccess("Two-factor authentication disabled successfully");
      setShowDisableDialog(false);
      await loadStatus();
    } catch (err) {
      setError("Failed to disable 2FA");
      console.error("Error disabling 2FA:", err);
    } finally {
      setDisableLoading(false);
    }
  };

  const copyCode = async (text: string, index: number) => {
    try {
      await navigator.clipboard.writeText(text);
      setCopiedIndex(index);
      setTimeout(() => setCopiedIndex(null), 2000);
    } catch {
      console.error("Failed to copy");
    }
  };

  const copyAllCodes = async () => {
    if (!enabledBackupCodes.length) return;
    try {
      await navigator.clipboard.writeText(enabledBackupCodes.join("\n"));
      setSuccess("All backup codes copied to clipboard");
    } catch {
      console.error("Failed to copy");
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center py-6">
        <div className="h-8 w-8 animate-spin rounded-full border-2 border-white/30 border-t-white" />
      </div>
    );
  }

  if (!status) {
    return (
      <div className="rounded-md border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-400">
        Failed to load 2FA status
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {error && (
        <div className="rounded-md border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-400 flex items-center justify-between">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="text-red-400 hover:text-red-300">
            ×
          </button>
        </div>
      )}
      {success && (
        <div className="rounded-md border border-green-500/30 bg-green-500/10 px-4 py-3 text-sm text-green-400 flex items-center justify-between">
          <span>{success}</span>
          <button type="button" onClick={() => setSuccess(null)} className="text-green-400 hover:text-green-300">
            ×
          </button>
        </div>
      )}

      <div className="flex items-center gap-3">
        {status.isEnabled ? (
          <CheckCircle className="h-5 w-5 text-green-400" />
        ) : (
          <AlertCircle className="h-5 w-5 text-white/60" />
        )}
        <span className="text-white font-medium">
          {status.isEnabled ? "Enabled" : "Disabled"}
        </span>
        {status.isEnabled && status.enabledAt && (
          <span className="text-sm text-white/60">
            Enabled on {new Date(status.enabledAt).toLocaleDateString()}
          </span>
        )}
      </div>

      {status.isEnabled && (
        <p className="text-sm text-white/60">
          Remaining backup codes: {status.remainingBackupCodes}
        </p>
      )}

      <div className="flex flex-wrap gap-2">
        {!status.isInitialized ? (
          <Button onClick={handleInitialize} disabled={initLoading} size="sm">
            {initLoading ? "Initializing..." : "Setup 2FA"}
          </Button>
        ) : !status.isEnabled ? (
          <>
            <Button onClick={() => setShowEnableDialog(true)} size="sm">
              Enable 2FA
            </Button>
            <Button variant="outline" onClick={handleInitialize} disabled={initLoading} size="sm">
              {initLoading ? "Refreshing..." : "Refresh QR Code"}
            </Button>
          </>
        ) : (
          <>
            <Button variant="destructive" onClick={() => setShowDisableDialog(true)} size="sm">
              Disable 2FA
            </Button>
            <Button variant="outline" onClick={handleInitialize} disabled={initLoading} size="sm">
              {initLoading ? "Refreshing..." : "Refresh QR Code"}
            </Button>
          </>
        )}
      </div>

      <Dialog
        open={showInitDialog}
        onClose={() => setShowInitDialog(false)}
        title="Setup Two-Factor Authentication"
      >
        {initData && (
          <div className="space-y-4">
            <p className="text-sm text-white/70">
              Scan this QR code with your authenticator app (Google Authenticator, Authy, or Microsoft Authenticator):
            </p>
            <div className="flex justify-center p-4 bg-white rounded-lg">
              <QRCode value={initData.qrCodeUrl} size={200} level="M" />
            </div>
            <p className="text-xs text-white/60">
              After scanning, click &quot;Enable 2FA&quot; and enter the 6-digit code.
            </p>
            <div className="flex gap-2 justify-end">
              <Button variant="outline" onClick={() => setShowInitDialog(false)} size="sm">
                Close
              </Button>
              <Button
                size="sm"
                onClick={() => {
                  setShowInitDialog(false);
                  setShowEnableDialog(true);
                }}
              >
                Enable 2FA
              </Button>
            </div>
          </div>
        )}
      </Dialog>

      <Dialog
        open={showEnableDialog}
        onClose={() => setShowEnableDialog(false)}
        title="Enable Two-Factor Authentication"
      >
        <div className="space-y-4">
          <p className="text-sm text-white/70">
            Enter the 6-digit code from your authenticator app:
          </p>
          <div className="relative">
            <Input
              variant="glass"
              type={showToken ? "text" : "password"}
              value={enableToken}
              onChange={(e) => setEnableToken(e.target.value.replace(/\D/g, "").slice(0, 6))}
              placeholder="000000"
              maxLength={6}
              className="pr-10"
            />
            <button
              type="button"
              onClick={() => setShowToken(!showToken)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-white/60 hover:text-white"
            >
              {showToken ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
            </button>
          </div>
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={() => setShowEnableDialog(false)} size="sm">
              Cancel
            </Button>
            <Button
              size="sm"
              onClick={handleEnable}
              disabled={enableLoading || enableToken.length !== 6}
            >
              {enableLoading ? "Enabling..." : "Enable 2FA"}
            </Button>
          </div>
        </div>
      </Dialog>

      <Dialog
        open={showBackupCodesDialog}
        onClose={() => setShowBackupCodesDialog(false)}
        title="Backup Codes"
      >
        <div className="space-y-4">
          <p className="text-sm text-white/70">
            Save these backup codes in a secure location. You can use them to access your account if you lose your authenticator device. Each code can only be used once.
          </p>
          <div className="grid grid-cols-2 gap-2">
            {enabledBackupCodes.map((code, i) => (
              <button
                key={i}
                type="button"
                onClick={() => copyCode(code, i)}
                className="flex items-center justify-between rounded-md border border-white/20 bg-white/5 px-3 py-2 text-sm font-mono text-white hover:bg-white/10"
              >
                {code}
                {copiedIndex === i && <CheckCircle className="h-4 w-4 text-green-400" />}
              </button>
            ))}
          </div>
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={copyAllCodes} size="sm">
              Copy All
            </Button>
            <Button size="sm" onClick={() => setShowBackupCodesDialog(false)}>
              I&apos;ve Saved My Codes
            </Button>
          </div>
        </div>
      </Dialog>

      <Dialog
        open={showDisableDialog}
        onClose={() => setShowDisableDialog(false)}
        title="Disable Two-Factor Authentication"
      >
        <div className="space-y-4">
          <p className="text-sm text-white/70">
            Disabling two-factor authentication will reduce the security of your account. Are you sure?
          </p>
          <div className="flex gap-2 justify-end">
            <Button variant="outline" onClick={() => setShowDisableDialog(false)} size="sm">
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDisable} disabled={disableLoading} size="sm">
              {disableLoading ? "Disabling..." : "Disable 2FA"}
            </Button>
          </div>
        </div>
      </Dialog>
    </div>
  );
}
