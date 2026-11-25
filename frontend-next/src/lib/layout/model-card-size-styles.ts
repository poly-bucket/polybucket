import type { ModelCardSize } from "@/lib/layout/model-browse-layout";
import { cn } from "@/lib/utils";

const SIZE_STYLES: Record<
  ModelCardSize,
  { shell: string; thumb: string; bodyPad: string; title: string }
> = {
  small: {
    shell: "h-60 sm:h-64",
    thumb: "h-32 sm:h-36",
    bodyPad: "p-2 sm:p-3",
    title: "text-xs min-h-[2rem]",
  },
  medium: {
    shell: "h-72 sm:h-80",
    thumb: "h-40 sm:h-48",
    bodyPad: "p-3 sm:p-4",
    title: "text-sm min-h-[2.5rem]",
  },
  large: {
    shell: "h-80 sm:h-96",
    thumb: "h-48 sm:h-56",
    bodyPad: "p-4 sm:p-5",
    title: "text-base min-h-[3rem]",
  },
};

export function modelCardSizeClasses(size: ModelCardSize = "medium") {
  const styles = SIZE_STYLES[size] ?? SIZE_STYLES.medium;
  return styles;
}

export function modelCardShellClass(size: ModelCardSize | undefined, className?: string) {
  const { shell } = modelCardSizeClasses(size ?? "medium");
  return cn(
    "group flex cursor-pointer flex-col overflow-hidden transition-all duration-200",
    shell,
    className
  );
}

export function modelCardSkeletonShellClass(size: ModelCardSize | undefined) {
  const { shell } = modelCardSizeClasses(size ?? "medium");
  return cn("flex flex-col overflow-hidden border-white/20 py-0", shell);
}