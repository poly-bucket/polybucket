"use client";

import { useState, useCallback, useRef } from "react";
import { toast } from "sonner";

export interface UseAdminMutationOptions<TArgs, TResult> {
  onSuccess?: (result: TResult, args: TArgs) => void;
  onError?: (err: unknown, args: TArgs) => void;
  successMessage?: string;
  errorMessage?: string;
  showToast?: boolean;
}

export interface UseAdminMutationResult<TArgs, TResult> {
  mutate: (args: TArgs) => Promise<TResult | undefined>;
  isLoading: boolean;
  error: string | null;
}

export function useAdminMutation<TArgs, TResult>(
  mutator: (args: TArgs) => Promise<TResult>,
  options: UseAdminMutationOptions<TArgs, TResult> = {}
): UseAdminMutationResult<TArgs, TResult> {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const inFlightRef = useRef(false);

  const {
    onSuccess,
    onError,
    successMessage,
    errorMessage = "Operation failed",
    showToast = true,
  } = options;

  const mutate = useCallback(
    async (args: TArgs): Promise<TResult | undefined> => {
      if (inFlightRef.current) return undefined;
      inFlightRef.current = true;
      setIsLoading(true);
      setError(null);

      try {
        const result = await mutator(args);
        onSuccess?.(result, args);
        if (showToast && successMessage) {
          toast.success(successMessage);
        }
        return result;
      } catch (err) {
        const message = err instanceof Error ? err.message : errorMessage;
        setError(message);
        onError?.(err, args);
        if (showToast) {
          toast.error(message);
        }
        return undefined;
      } finally {
        inFlightRef.current = false;
        setIsLoading(false);
      }
    },
    [mutator, onSuccess, onError, successMessage, errorMessage, showToast]
  );

  return { mutate, isLoading, error };
}
