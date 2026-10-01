"use client";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

interface SupportedFormatsDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  groups: { label: string; extensions: string[] }[];
}

export function SupportedFormatsDialog({
  open,
  onOpenChange,
  groups,
}: SupportedFormatsDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent variant="glass" className="max-w-md">
        <DialogHeader>
          <DialogTitle>Supported file formats</DialogTitle>
          <DialogDescription>
            File types allowed for this site. Individual files may also have size limits.
          </DialogDescription>
        </DialogHeader>
        <div className="space-y-4 text-sm">
          {groups.map((group) => (
            <div key={group.label}>
              <p className="font-medium text-foreground">{group.label}</p>
              <p className="mt-1 text-muted-foreground break-words">
                {group.extensions.join(", ")}
              </p>
            </div>
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
}
