"use client";

import { useState, useEffect, useCallback, useRef } from "react";

export interface UseAdminQueryResult<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export function useAdminQuery<T>(
  fetcher: () => Promise<T>,
  deps: unknown[] = []
): UseAdminQueryResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const inFlightRef = useRef<Promise<void> | null>(null);
  const mountedRef = useRef(true);
  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  const execute = useCallback(async () => {
    if (inFlightRef.current) {
      await inFlightRef.current;
      return;
    }

    const promise = (async () => {
      try {
        setIsLoading(true);
        setError(null);
        const result = await fetcherRef.current();
        if (mountedRef.current) {
          setData(result);
        }
      } catch (err) {
        if (mountedRef.current) {
          setError(err instanceof Error ? err.message : "An error occurred");
        }
      } finally {
        inFlightRef.current = null;
        if (mountedRef.current) {
          setIsLoading(false);
        }
      }
    })();

    inFlightRef.current = promise;
    await promise;
  }, deps);

  useEffect(() => {
    mountedRef.current = true;
    execute();
    return () => {
      mountedRef.current = false;
    };
  }, [execute]);

  const refetch = useCallback(async () => {
    inFlightRef.current = null;
    await execute();
  }, [execute]);

  return { data, isLoading, error, refetch };
}
