"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { ApiClientFactory } from "@/lib/api/clientFactory";
import AdminAccountSetupStep from "@/setup/AdminAccountSetupStep";
import SiteSecurityStep from "@/setup/SiteSecurityStep";
import EmailSetupStep from "@/setup/EmailSetupStep";
import SiteEssentialsStep from "@/setup/SiteEssentialsStep";
import SetupComplete from "@/setup/SetupComplete";
import { Card, CardContent } from "@/components/primitives/card";
import { AuthCardSkeleton } from "@/components/ui/skeletons";

type StepIndex = 0 | 1 | 2 | 3;

function resumeStepFromStatus(status: {
  isAdminConfigured?: boolean;
  isSiteConfigured?: boolean;
  isEmailConfigured?: boolean;
}): StepIndex {
  if (!status.isAdminConfigured) return 0;
  if (!status.isSiteConfigured) return 1;
  if (!status.isEmailConfigured) return 2;
  return 3;
}

const STEP_NAMES: Record<StepIndex, string> = {
  0: "Admin Account Setup",
  1: "Site security",
  2: "Email",
  3: "Site essentials",
};
const TOTAL_STEPS = Object.keys(STEP_NAMES).length;

export default function SetupPage() {
  const router = useRouter();
  const { user, isLoading: isAuthLoading } = useAuth();
  const [currentStep, setCurrentStep] = useState<StepIndex>(0);
  const [setupData, setSetupData] = useState<Record<string, unknown>>({});
  const [isComplete, setIsComplete] = useState(false);
  const [isLoadingStatus, setIsLoadingStatus] = useState(true);
  const [isCompletingSetup, setIsCompletingSetup] = useState(false);

  const isAdmin = !!user?.roles?.some((r) => r?.toLowerCase() === "admin");

  useEffect(() => {
    if (isAuthLoading) return;
    if (!user?.id) {
      router.replace("/login");
      return;
    }
    if (!isAdmin) {
      router.replace("/");
      return;
    }

    const checkStatus = async () => {
      try {
        const client = ApiClientFactory.getApiClient();
        const status = await client.systemSetup_GetSetupStatus();

        if (status?.isFirstTimeSetup && status?.isSiteConfigured) {
          setIsCompletingSetup(true);
          try {
            const response = await client.systemSetup_CompleteSetup();
            if (response?.success) {
              setIsComplete(true);
            }
          } catch {
            setCurrentStep(resumeStepFromStatus(status));
          } finally {
            setIsCompletingSetup(false);
            setIsLoadingStatus(false);
          }
          return;
        }

        setCurrentStep(resumeStepFromStatus(status ?? {}));
      } catch {
        setCurrentStep(0);
      } finally {
        setIsLoadingStatus(false);
      }
    };

    checkStatus();
  }, [user?.id, isAuthLoading, isAdmin, router]);

  const handleAdminAccountComplete = (data: Record<string, unknown>) => {
    setSetupData((prev) => ({ ...prev, ...data }));
    setCurrentStep(1);
  };

  const handleSiteSecurityComplete = (data: Record<string, unknown>) => {
    setSetupData((prev) => ({ ...prev, ...data }));
    setCurrentStep(2);
  };

  const handleEmailComplete = (data: Record<string, unknown>) => {
    setSetupData((prev) => ({ ...prev, ...data }));
    setCurrentStep(3);
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
      <div className="flex w-full max-w-md justify-center">
        <AuthCardSkeleton />
      </div>
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
    <div className={`w-full space-y-6 ${currentStep === 2 ? "max-w-2xl" : "max-w-lg"}`}>
        <div className="text-center">
          <h1 className="text-2xl font-semibold text-white">
            Setup Wizard
          </h1>
          <p className="mt-1 text-sm text-white/60">
            Step {currentStep + 1} of {TOTAL_STEPS}: {STEP_NAMES[currentStep]}
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
              <EmailSetupStep
                onComplete={handleEmailComplete}
                onBack={() => setCurrentStep(1)}
                defaultTestRecipient={user?.email ?? ""}
              />
            )}
            {currentStep === 3 && (
              <SiteEssentialsStep
                onComplete={handleSiteComplete}
                onBack={() => setCurrentStep(2)}
                data={setupData}
              />
            )}
          </CardContent>
        </Card>
    </div>
  );
}
