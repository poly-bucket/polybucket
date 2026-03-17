"use client";

import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/primitives/button";

interface SimplePaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (page: number) => void;
}

export function SimplePagination({
  page,
  totalPages,
  totalCount,
  onPageChange,
}: SimplePaginationProps) {
  if (totalPages <= 1) return null;

  return (
    <div className="mt-6 flex items-center justify-center gap-2">
      <Button
        variant="ghost"
        size="icon-sm"
        onClick={() => onPageChange(Math.max(1, page - 1))}
        disabled={page <= 1}
      >
        <ChevronLeft className="h-4 w-4" />
      </Button>
      <span className="text-sm text-white/60">
        Page {page} of {totalPages} ({totalCount} total)
      </span>
      <Button
        variant="ghost"
        size="icon-sm"
        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        disabled={page >= totalPages}
      >
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  );
}
