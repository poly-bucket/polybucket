"use client";

/**
 * Settings danger zone: export account data (GET /api/auth/account/export),
 * sign out all sessions (POST /api/auth/account/sessions/revoke-all),
 * and delete account (POST /api/auth/account/delete).
 */

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AlertTriangle, Download, LogOut, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { SettingsSection } from "@/components/settings/settings-section";
import { Button } from "@/components/primitives/button";
import { Input } from "@/components/primitives/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  deleteAccount,
  exportAccountData,
  getActiveSessions,
  revokeSession,
  revokeAllSessions,
  type ActiveSession,
} from "@/lib/services/dangerZoneService";
import { twoFactorAuthService } from "@/services/twoFactorAuthService";
import { cn } from "@/lib/utils";

export default function DangerZoneSettingsPage() {
  const { user, logout } = useAuth();
  const router = useRouter();
  const [exporting, setExporting] = useState(false);
  const [revokeOpen, setRevokeOpen] = useState(false);
  const [revoking, setRevoking] = useState(false);
  const [sessionsLoading, setSessionsLoading] = useState(true);
  const [sessions, setSessions] = useState<ActiveSession[]>([]);
  const [revokingSessionId, setRevokingSessionId] = useState<string | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [deletePassword, setDeletePassword] = useState("");
  const [deleteConfirm, setDeleteConfirm] = useState("");
  const [totp, setTotp] = useState("");
  const [backupCode, setBackupCode] = useState("");
  const [useBackup, setUseBackup] = useState(false);
  const [twoFactorEnabled, setTwoFactorEnabled] = useState(false);

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      try {
        const s = await twoFactorAuthService.getStatus();
        if (!cancelled) {
          setTwoFactorEnabled(s.isEnabled);
        }
      } catch {
        if (!cancelled) {
          setTwoFactorEnabled(false);
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;
    void (async () => {
      try {
        setSessionsLoading(true);
        const next = await getActiveSessions();
        if (!cancelled) {
          setSessions(next);
        }
      } catch {
        if (!cancelled) {
          setSessions([]);
        }
      } finally {
        if (!cancelled) {
          setSessionsLoading(false);
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const signOutAndGoLogin = useCallback(() => {
    logout();
    router.push("/login");
  }, [logout, router]);

  const handleExport = async () => {
    setExporting(true);
    try {
      const data = await exportAccountData();
      const blob = new Blob([JSON.stringify(data, null, 2)], { type: "application/json" });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `polybucket-account-export-${new Date().toISOString().slice(0, 10)}.json`;
      a.click();
      URL.revokeObjectURL(url);
      toast.success("Download started");
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Export failed");
    } finally {
      setExporting(false);
    }
  };

  const handleRevokeAll = async () => {
    setRevoking(true);
    try {
      await revokeAllSessions();
      toast.success("All other sessions have been signed out");
      setRevokeOpen(false);
      signOutAndGoLogin();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Could not revoke sessions");
    } finally {
      setRevoking(false);
    }
  };

  const handleRevokeSession = async (sessionId: string) => {
    setRevokingSessionId(sessionId);
    try {
      await revokeSession(sessionId);
      setSessions((prev) => prev.filter((s) => s.sessionId !== sessionId));
      toast.success("Session revoked");
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Could not revoke session");
    } finally {
      setRevokingSessionId(null);
    }
  };

  const handleDelete = async () => {
    if (!user?.username) {
      toast.error("Missing username");
      return;
    }
    if (deleteConfirm.trim() !== user.username) {
      toast.error("Confirmation must match your username exactly");
      return;
    }
    if (!deletePassword.trim()) {
      toast.error("Enter your password");
      return;
    }
    if (twoFactorEnabled) {
      if (useBackup) {
        if (!backupCode.trim()) {
          toast.error("Enter a backup code");
          return;
        }
      } else if (totp.replace(/\D/g, "").length !== 6) {
        toast.error("Enter your 6-digit authenticator code");
        return;
      }
    }

    setDeleting(true);
    try {
      await deleteAccount({
        password: deletePassword,
        twoFactorToken: useBackup ? undefined : totp.replace(/\D/g, ""),
        backupCode: useBackup ? backupCode.trim() : undefined,
      });
      toast.success("Your account has been closed");
      setDeleteOpen(false);
      signOutAndGoLogin();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Could not delete account");
    } finally {
      setDeleting(false);
    }
  };

  const resetDeleteDialog = () => {
    setDeletePassword("");
    setDeleteConfirm("");
    setTotp("");
    setBackupCode("");
    setUseBackup(false);
  };

  return (
    <div className="space-y-8">
      <SettingsSection
        title="Export data"
        description="Download a JSON snapshot of your profile and settings."
      >
        <div className="flex flex-wrap items-center gap-3">
          <Button
            type="button"
            variant="glass"
            size="sm"
            disabled={exporting}
            onClick={() => void handleExport()}
          >
            <Download className="h-4 w-4" />
            {exporting ? "Preparing…" : "Download export"}
          </Button>
        </div>
      </SettingsSection>

      <SettingsSection
        title="Active sessions"
        description="Review active sessions and revoke one at a time."
      >
        <div className="space-y-3">
          {sessionsLoading ? (
            <p className="text-sm text-white/60">Loading sessions…</p>
          ) : sessions.length === 0 ? (
            <p className="text-sm text-white/60">No active sessions found.</p>
          ) : (
            sessions.map((session) => (
              <div
                key={session.sessionId}
                className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-white/10 bg-white/5 p-3"
              >
                <div className="text-sm text-white/80">
                  <p className="font-medium">{session.createdByIp}</p>
                  <p className="text-xs text-white/60">
                    Started {new Date(session.createdAt).toLocaleString()} - Expires {new Date(session.expiresAt).toLocaleString()}
                  </p>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  disabled={revokingSessionId === session.sessionId}
                  onClick={() => void handleRevokeSession(session.sessionId)}
                >
                  {revokingSessionId === session.sessionId ? "Revoking…" : "Revoke"}
                </Button>
              </div>
            ))
          )}
        </div>
      </SettingsSection>

      <SettingsSection
        title="Sign out everywhere"
        description="Revoke all refresh tokens for your account. You will need to sign in again on this device."
      >
        <Button
          type="button"
          variant="outline"
          size="sm"
          className="border-amber-500/40 text-white hover:bg-amber-500/10"
          onClick={() => setRevokeOpen(true)}
        >
          <LogOut className="h-4 w-4" />
          Sign out of all sessions
        </Button>

        <Dialog open={revokeOpen} onOpenChange={setRevokeOpen}>
          <DialogContent className="border-white/20 bg-zinc-950/95 text-white">
            <DialogHeader>
              <DialogTitle>Sign out everywhere?</DialogTitle>
              <DialogDescription className="text-white/70">
                This invalidates all active sessions. You will be redirected to the sign-in page.
              </DialogDescription>
            </DialogHeader>
            <DialogFooter className="gap-2 sm:gap-0">
              <Button type="button" variant="ghost" onClick={() => setRevokeOpen(false)}>
                Cancel
              </Button>
              <Button
                type="button"
                variant="glass"
                disabled={revoking}
                onClick={() => void handleRevokeAll()}
              >
                {revoking ? "Signing out…" : "Confirm"}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </SettingsSection>

      <div className="rounded-xl border border-red-500/30 bg-red-950/20 p-1">
        <SettingsSection
          title="Delete account"
          description="Permanently close your account. Your profile is anonymized and you will not be able to sign in again."
          className="border-red-500/20"
        >
          <Button
            type="button"
            variant="destructive"
            size="sm"
            className="bg-red-600 hover:bg-red-700"
            onClick={() => {
              resetDeleteDialog();
              setDeleteOpen(true);
            }}
          >
            <Trash2 className="h-4 w-4" />
            Delete my account
          </Button>
        </SettingsSection>
      </div>

      <Dialog
        open={deleteOpen}
        onOpenChange={(open) => {
          setDeleteOpen(open);
          if (!open) {
            resetDeleteDialog();
          }
        }}
      >
        <DialogContent className="max-h-[90vh] overflow-y-auto border-white/20 bg-zinc-950/95 text-white">
          <DialogHeader>
            <div className="flex items-center gap-2">
              <AlertTriangle className="h-6 w-6 text-red-400" />
              <DialogTitle>Delete account</DialogTitle>
            </div>
            <DialogDescription className="text-left text-white/70">
              This cannot be undone. Type your username <span className="font-medium text-white">{user?.username ?? "…"}</span> to
              confirm, then enter your password
              {twoFactorEnabled ? " and two-factor code" : ""}.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-2">
            <div>
              <label htmlFor="delete-confirm" className="mb-1.5 block text-sm font-medium text-white/80">
                Confirm username
              </label>
              <Input
                id="delete-confirm"
                value={deleteConfirm}
                onChange={(e) => setDeleteConfirm(e.target.value)}
                autoComplete="off"
                placeholder={user?.username ?? ""}
                className="border-white/20 bg-white/5 text-white"
              />
            </div>
            <div>
              <label htmlFor="delete-password" className="mb-1.5 block text-sm font-medium text-white/80">
                Current password
              </label>
              <Input
                id="delete-password"
                type="password"
                value={deletePassword}
                onChange={(e) => setDeletePassword(e.target.value)}
                autoComplete="current-password"
                className="border-white/20 bg-white/5 text-white"
              />
            </div>

            {twoFactorEnabled && (
              <div className="space-y-3 rounded-lg border border-white/10 p-3">
                <button
                  type="button"
                  className="text-sm text-primary underline-offset-4 hover:underline"
                  onClick={() => setUseBackup((u) => !u)}
                >
                  {useBackup ? "Use authenticator code" : "Use a backup code instead"}
                </button>
                {useBackup ? (
                  <div>
                    <label htmlFor="delete-backup" className="mb-1.5 block text-sm font-medium text-white/80">
                      Backup code
                    </label>
                    <Input
                      id="delete-backup"
                      value={backupCode}
                      onChange={(e) => setBackupCode(e.target.value)}
                      className="border-white/20 bg-white/5 text-white"
                    />
                  </div>
                ) : (
                  <div>
                    <label htmlFor="delete-totp" className="mb-1.5 block text-sm font-medium text-white/80">
                      Authenticator code
                    </label>
                    <Input
                      id="delete-totp"
                      inputMode="numeric"
                      maxLength={6}
                      value={totp}
                      onChange={(e) => setTotp(e.target.value.replace(/\D/g, "").slice(0, 6))}
                      className={cn(
                        "border-white/20 bg-white/5 font-mono tracking-widest text-white"
                      )}
                    />
                  </div>
                )}
              </div>
            )}
          </div>

          <DialogFooter className="gap-2 sm:gap-0">
            <Button type="button" variant="ghost" onClick={() => setDeleteOpen(false)}>
              Cancel
            </Button>
            <Button
              type="button"
              variant="destructive"
              className="bg-red-600 hover:bg-red-700"
              disabled={deleting}
              onClick={() => void handleDelete()}
            >
              {deleting ? "Deleting…" : "Delete account permanently"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
