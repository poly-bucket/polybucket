import React from "react";
import { render, type RenderOptions } from "@testing-library/react";
import { Providers } from "@/components/providers/providers";
import { AuthContext, type AuthUser } from "@/contexts/AuthContext";
import type { LoginResult } from "@/contexts/AuthContext";
import { vi } from "vitest";

export interface MockAuthValue {
  user: AuthUser | null;
  isLoading?: boolean;
  isAuthenticated?: boolean;
  login?: (emailOrUsername: string, password: string) => Promise<LoginResult>;
  logout?: () => void;
  refreshUserFromMe?: () => Promise<void>;
}

function createMockAuthProvider(mockValue: MockAuthValue) {
  const value = {
    user: mockValue.user,
    isLoading: mockValue.isLoading ?? false,
    isAuthenticated: mockValue.isAuthenticated ?? !!mockValue.user?.accessToken,
    login: mockValue.login ?? vi.fn().mockResolvedValue({ success: true } as LoginResult),
    logout: mockValue.logout ?? vi.fn(),
    refreshUserFromMe: mockValue.refreshUserFromMe ?? vi.fn().mockResolvedValue(undefined),
  };

  return function MockAuthProvider({ children }: { children: React.ReactNode }) {
    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
  };
}

export interface CustomRenderOptions extends Omit<RenderOptions, "wrapper"> {
  mockAuth?: MockAuthValue | null;
}

function createWrapper(mockAuth: MockAuthValue | null) {
  if (mockAuth) {
    const MockProvider = createMockAuthProvider(mockAuth);
    return function Wrapper({ children }: { children: React.ReactNode }) {
      return (
        <MockProvider>
          {children}
        </MockProvider>
      );
    };
  }
  return function Wrapper({ children }: { children: React.ReactNode }) {
    return <Providers>{children}</Providers>;
  };
}

export function customRender(
  ui: React.ReactElement,
  options: CustomRenderOptions = {}
) {
  const { mockAuth = null, ...renderOptions } = options;
  const Wrapper = createWrapper(mockAuth);

  return {
    ...render(ui, {
      wrapper: Wrapper,
      ...renderOptions,
    }),
  };
}

export * from "@testing-library/react";
export { customRender as render };
