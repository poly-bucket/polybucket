"use client";

import React, { createContext, useContext, useEffect, useState, useCallback } from "react";
import { useAuth } from "./AuthContext";
import {
  getUserSettings,
  updateUserSettings as updateUserSettingsApi,
  getDefaultSettings,
  type UserSettingsData,
} from "@/lib/services/userSettingsService";

interface UserSettingsContextType {
  settings: UserSettingsData | null;
  loading: boolean;
  updateSettings: (partial: Partial<UserSettingsData>) => Promise<boolean>;
  updateAutoRotate: (autoRotate: boolean) => Promise<boolean>;
}

const UserSettingsContext = createContext<UserSettingsContextType | undefined>(undefined);

export function UserSettingsProvider({ children }: { children: React.ReactNode }) {
  const { user } = useAuth();
  const [settings, setSettings] = useState<UserSettingsData | null>(null);
  const [loading, setLoading] = useState(true);

  const loadSettings = useCallback(async () => {
    if (!user?.accessToken) {
      setSettings(null);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const data = await getUserSettings();
      setSettings(data);
    } catch (error) {
      console.error("Error loading user settings:", error);
      setSettings(getDefaultSettings());
    } finally {
      setLoading(false);
    }
  }, [user?.accessToken]);

  useEffect(() => {
    loadSettings();
  }, [loadSettings]);

  const updateSettings = useCallback(
    async (partial: Partial<UserSettingsData>): Promise<boolean> => {
      if (!user?.accessToken) return false;
      const success = await updateUserSettingsApi(partial);
      if (success && settings) {
        setSettings({ ...settings, ...partial });
      }
      return success;
    },
    [user?.accessToken, settings]
  );

  const updateAutoRotate = useCallback(
    async (autoRotate: boolean): Promise<boolean> => {
      return updateSettings({ autoRotateModels: autoRotate });
    },
    [updateSettings]
  );

  const value: UserSettingsContextType = {
    settings,
    loading,
    updateSettings,
    updateAutoRotate,
  };

  return (
    <UserSettingsContext.Provider value={value}>
      {children}
    </UserSettingsContext.Provider>
  );
}

export function useUserSettings(): UserSettingsContextType {
  const context = useContext(UserSettingsContext);
  if (context === undefined) {
    throw new Error("useUserSettings must be used within UserSettingsProvider");
  }
  return context;
}
