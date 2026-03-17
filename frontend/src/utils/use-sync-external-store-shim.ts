// Wrapper to provide useSyncExternalStoreWithSelector export
import { useSyncExternalStore } from 'use-sync-external-store/shim';

export function useSyncExternalStoreWithSelector(
  subscribe: (onStoreChange: () => void) => () => void,
  getSnapshot: () => any,
  getServerSnapshot: (() => any) | undefined,
  selector: (snapshot: any) => any,
  isEqual?: (a: any, b: any) => boolean
) {
  const selectedSnapshot = useSyncExternalStore(
    subscribe,
    () => selector(getSnapshot()),
    getServerSnapshot ? () => selector(getServerSnapshot()) : undefined
  );
  return selectedSnapshot;
}

// Also provide as default export for zustand and other libraries
const useSyncExternalStoreExports = { useSyncExternalStoreWithSelector };
export default useSyncExternalStoreExports;
