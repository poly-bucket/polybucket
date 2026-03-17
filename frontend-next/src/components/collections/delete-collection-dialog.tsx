"use client";

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Button } from "@/components/primitives/button";
import { Trash2 } from "lucide-react";

interface DeleteCollectionDialogProps {
  isOpen: boolean;
  collectionName: string;
  onConfirm: () => void;
  onCancel: () => void;
  isDeleting: boolean;
}

export function DeleteCollectionDialog({
  isOpen,
  collectionName,
  onConfirm,
  onCancel,
  isDeleting,
}: DeleteCollectionDialogProps) {
  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && !isDeleting && onCancel()}>
      <DialogContent
        showCloseButton={!isDeleting}
        className="glass-bg border-white/20"
        onPointerDownOutside={(e) => {
          if (isDeleting) e.preventDefault();
          else onCancel();
        }}
      >
        <DialogHeader>
          <div className="mx-auto flex h-12 w-12 shrink-0 items-center justify-center rounded-full bg-red-500/20">
            <Trash2 className="h-6 w-6 text-red-400" />
          </div>
          <DialogTitle className="text-center text-white">
            Delete Collection
          </DialogTitle>
          <DialogDescription className="text-center text-white/70">
            This action cannot be undone
          </DialogDescription>
        </DialogHeader>
        <p className="mb-6 text-center text-white/80">
          Are you sure you want to delete &quot;{collectionName}&quot;? This will
          permanently remove the collection and all its model associations.
        </p>
        <DialogFooter className="gap-2 sm:gap-0">
          <Button
            variant="outline"
            onClick={onCancel}
            disabled={isDeleting}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={onConfirm}
            disabled={isDeleting}
          >
            {isDeleting ? "Deleting..." : "Delete"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
