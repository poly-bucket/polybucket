"use client";

import { useState, useEffect } from "react";
import { Lock } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Input } from "@/components/primitives/input";
import { Button } from "@/components/primitives/button";

interface PasswordPromptProps {
  open: boolean;
  collectionName: string;
  onSubmit: (password: string) => void;
  onCancel: () => void;
  error?: string;
}

export function PasswordPrompt({
  open,
  collectionName,
  onSubmit,
  onCancel,
  error,
}: PasswordPromptProps) {
  const [password, setPassword] = useState("");

  useEffect(() => {
    if (open) {
      setPassword("");
    }
  }, [open]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (password.trim()) {
      onSubmit(password.trim());
    }
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onCancel()}>
      <DialogContent
        showCloseButton={true}
        className="glass-bg border-white/20"
        onPointerDownOutside={(e) => {
          e.preventDefault();
          onCancel();
        }}
      >
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <div className="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-indigo-500/20">
              <Lock className="h-6 w-6 text-indigo-400" />
            </div>
            <DialogTitle className="text-center text-white">
              Password Required
            </DialogTitle>
            <DialogDescription className="text-center text-white/70">
              This collection &quot;{collectionName}&quot; is password protected.
              Enter the password to access it.
            </DialogDescription>
          </DialogHeader>
          <div className="mt-4 space-y-4">
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              required
              autoFocus
              error={!!error}
            />
            {error && (
              <p className="text-sm text-red-400">{error}</p>
            )}
          </div>
          <DialogFooter className="mt-6 gap-2 sm:gap-0">
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={!password.trim()}
            >
              Access Collection
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
