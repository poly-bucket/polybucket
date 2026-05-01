"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import AdminAccountSetupStep from "@/setup/AdminAccountSetupStep";
import SiteSecurityStep from "@/setup/SiteSecurityStep";
import SiteEssentialsStep from "@/setup/SiteEssentialsStep";
import SetupComplete from "@/setup/SetupComplete";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/primitives/card";

type StepIndex = 0 | 1 | 2;
const STEP_NAMES: Record<StepIndex, string> = {
  0: "Admin Account Setup",
  1: "Site security",
  2: "Site essentials",
};

export default function SetupPage() {
  const router = useRouter();
  const { user, isLoading: isAuthLoading } = useAuth();
  const [currentStep, setCurrentStep] = useState<StepIndex>(0);
  const [setupData, setSetupData] = useState<Record<string, unknown>>({});
  const [isComplete, setIsComplete] = useState(false);
  const [isLoadingStatus, setIsLoadingStatus] = useState(true);
  const [isCompletingSetup, setIsCompletingSetup] = useState(false);
  const [completeError, setCompleteError] = useState<string | null>(null);
  const [siteConfiguredPendingComplete, setSiteConfiguredPendingComplete] =
    useState(false);

  const retryCompleteSetup = async () => {
    setCompleteError(null);
    setIsCompletingSetup(true);
    try {
      const client = ApiClientFactory.getApiClient();
      const response = await client.systemSetup_CompleteSetup();
      if (response?.success) {
        setIsComplete(true);
      } else {
        setCompleteError(response?.message ?? "Failed to complete setup");
      }
    } catch {
      setCompleteError("Failed to complete setup. Please try again.");
    } finally {
      setIsCompletingSetup(false);
    }
  };

  useEffect(() => {
    if (isAuthLoading) return;
    if (!user?.accessToken) {
      router.replace("/login");
      return;
    }

    const checkStatus = async () => {
      try {
        const client = ApiClientFactory.getApiClient();
        const status = await client.systemSetup_GetSetupStatus();

        if (status?.isModerationConfigured) {
          router.replace("/");
          return;
        }

        if (status?.isSiteConfigured) {
          setSiteConfiguredPendingComplete(true);
          setIsCompletingSetup(true);
          setCompleteError(null);
          try {
            const response = await client.systemSetup_CompleteSetup();
            if (response?.success) {
              setIsComplete(true);
            } else {
              setCompleteError(response?.message ?? "Failed to complete setup");
            }
          } catch {
            setCompleteError("Failed to complete setup. Please try again.");
          } finally {
            setIsCompletingSetup(false);
            setIsLoadingStatus(false);
          }
          return;
        }

        if (status?.isAdminConfigured) {
          setCurrentStep(1);
        } else {
          setCurrentStep(0);
        }
      } catch {
        setCurrentStep(0);
      } finally {
        setIsLoadingStatus(false);
      }
    };

    checkStatus();
  }, [user?.accessToken, isAuthLoading, router]);

  const handleAdminAccountComplete = (data: Record<string, unknown>) => {
    setSetupData((prev) => ({ ...prev, ...data }));
    setCurrentStep(1);
  };

  const handleSiteSecurityComplete = (data: Record<string, unknown>) => {
    setSetupData((prev) => ({ ...prev, ...data }));
    setCurrentStep(2);
  };

  const handleSiteComplete = async (data: Record<string, unknown>) => {
    setSetupData((prev) => ({ ...prev, ...data }));
    setIsCompletingSetup(true);
    try {
      const client = ApiClientFactory.getApiClient();
      const response = await client.systemSetup_CompleteSetup();
      if (response?.success) {
        setIsComplete(true);
      }
    } finally {
      setIsCompletingSetup(false);
    }
  };

  const handleSetupCompleteRedirect = () => {
    router.push("/");
  };

  if (isAuthLoading || isLoadingStatus || isCompletingSetup) {
    return (
      <div className="flex flex-col items-center gap-4">
        <div className="size-8 animate-spin rounded-full border-2 border-white/30 border-t-white" />
        <p className="text-white/70">Checking setup status...</p>
      </div>
    );
  }

  if (siteConfiguredPendingComplete && completeError) {
    return (
      <Card variant="glass" className="w-full max-w-md border-white/20">
          <CardHeader>
            <CardTitle className="text-white">Complete setup</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <p className="text-sm text-red-300">{completeError}</p>
            <button
              type="button"
              onClick={retryCompleteSetup}
              disabled={isCompletingSetup}
              className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {isCompletingSetup ? "Completing..." : "Retry"}
            </button>
          </CardContent>
        </Card>
    );
  }

  if (isComplete) {
    return (
      <Card variant="glass" className="w-full max-w-md border-white/20">
        <CardContent className="pt-8">
          <SetupComplete onComplete={handleSetupCompleteRedirect} />
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="w-full max-w-lg space-y-6">
        <div className="text-center">
          <h1 className="text-2xl font-semibold text-white">
            Setup Wizard
          </h1>
          <p className="mt-1 text-sm text-white/60">
            Step {currentStep + 1} of 3: {STEP_NAMES[currentStep]}
          </p>
        </div>

        <Card variant="glass" className="border-white/20">
          <CardContent>
            {currentStep === 0 && (
              <AdminAccountSetupStep
                onComplete={handleAdminAccountComplete}
                onBack={() => setCurrentStep(0)}
                isFirstStep={true}
              />
            )}
            {currentStep === 1 && (
              <SiteSecurityStep
                onComplete={handleSiteSecurityComplete}
                onBack={() => setCurrentStep(0)}
                data={setupData}
              />
            )}
            {currentStep === 2 && (
              <SiteEssentialsStep
                onComplete={handleSiteComplete}
                onBack={() => setCurrentStep(1)}
                data={setupData}
              />
            )}
          </CardContent>
        </Card>
    </div>
  );
}
