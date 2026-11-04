"use client";

import { useState } from "react";
import { Trash2 } from "lucide-react";
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

export const BULK_DELETE_CONFIRM_PHRASE = "DELETE ALL";

type AdminBulkDeleteDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  stepOneDescription: string;
  confirmPhrase?: string;
  onConfirm: (adminPassword: string) => Promise<void>;
  confirmButtonLabel?: string;
};

export function AdminBulkDeleteDialog({
  open,
  onOpenChange,
  title,
  stepOneDescription,
  confirmPhrase = BULK_DELETE_CONFIRM_PHRASE,
  onConfirm,
  confirmButtonLabel = "Delete All",
}: AdminBulkDeleteDialogProps) {
  const [step, setStep] = useState<1 | 2>(1);
  const [confirmText, setConfirmText] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const confirmTextMatch = confirmText.trim() === confirmPhrase;

  const reset = () => {
    setStep(1);
    setConfirmText("");
    setPassword("");
    setLoading(false);
  };

  const handleOpenChange = (next: boolean) => {
    if (!next) {
      reset();
    }
    onOpenChange(next);
  };

  const handleConfirm = async () => {
    if (!password.trim()) return;
    setLoading(true);
    try {
      await onConfirm(password);
      reset();
      onOpenChange(false);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent variant="glass">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>
            {step === 1
              ? stepOneDescription
              : "Enter your admin password to confirm this destructive action."}
          </DialogDescription>
        </DialogHeader>
        {step === 1 ? (
          <div>
            <label className="text-sm font-medium text-white/80">
              Confirmation text
            </label>
            <Input
              variant="glass"
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              placeholder={confirmPhrase}
              className="mt-1 text-white"
            />
          </div>
        ) : (
          <div>
            <label className="text-sm font-medium text-white/80">
              Admin password
            </label>
            <Input
              variant="glass"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter admin password"
              className="mt-1 text-white"
            />
          </div>
        )}
        <DialogFooter>
          <Button variant="outline" onClick={() => handleOpenChange(false)}>
            Cancel
          </Button>
          {step === 1 ? (
            <Button
              variant="destructive"
              onClick={() => setStep(2)}
              disabled={!confirmTextMatch}
            >
              Next
            </Button>
          ) : (
            <Button
              variant="destructive"
              onClick={handleConfirm}
              disabled={loading || !password.trim()}
            >
              {loading ? "Deleting..." : confirmButtonLabel}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

type BulkDeleteOperationRowProps = {
  title: string;
  description: string;
  buttonLabel: string;
  dialogTitle: string;
  dialogStepOneDescription: string;
  onConfirm: (adminPassword: string) => Promise<void>;
  successMessage?: string;
  errorMessage?: string;
};

export function BulkDeleteOperationRow({
  title,
  description,
  buttonLabel,
  dialogTitle,
  dialogStepOneDescription,
  onConfirm,
}: BulkDeleteOperationRowProps) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <div className="flex items-center justify-between py-4 border-t border-white/10 first:border-t-0 first:pt-0">
        <div>
          <p className="font-medium text-white">{title}</p>
          <p className="text-sm text-white/60">{description}</p>
        </div>
        <Button
          variant="destructive"
          onClick={() => setOpen(true)}
          className="text-white shrink-0 ml-4"
        >
          <Trash2 className="h-4 w-4 mr-2" />
          {buttonLabel}
        </Button>
      </div>
      <AdminBulkDeleteDialog
        open={open}
        onOpenChange={setOpen}
        title={dialogTitle}
        stepOneDescription={dialogStepOneDescription}
        onConfirm={onConfirm}
      />
    </>
  );
}
