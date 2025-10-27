declare module 'next-themes' {
  import { ReactNode } from 'react';

  export interface ThemeProviderProps {
    children: ReactNode;
    attribute?: string;
    defaultTheme?: string;
    enableSystem?: boolean;
    disableTransitionOnChange?: boolean;
    [key: string]: unknown;
  }

  export function ThemeProvider(props: ThemeProviderProps): JSX.Element;

  export interface UseThemeReturn {
    theme: string | undefined;
    setTheme: (theme: string) => void;
    systemTheme: string | undefined;
    themes: string[];
  }

  export function useTheme(): UseThemeReturn;
}
