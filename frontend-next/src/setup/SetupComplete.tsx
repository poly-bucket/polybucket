"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { Check } from "lucide-react";
import { Button } from "@/components/primitives/button";

interface SetupCompleteProps {
  onComplete: () => void;
}

export default function SetupComplete({ onComplete }: SetupCompleteProps) {
  const router = useRouter();

  const handleGoToDashboard = () => {
    onComplete();
    router.push("/");
  };

  return (
    <div className="flex w-full flex-col items-center space-y-6 text-center">
      <div className="relative">
        <div className="size-16 rounded-full bg-green-500/20 flex items-center justify-center">
          <Check className="size-8 text-green-400" />
        </div>
      </div>
      <div className="w-full">
        <h2 className="text-2xl font-semibold text-white">Setup complete</h2>
        <p className="mx-auto mt-2 max-w-sm text-sm text-white/70">
          Your PolyBucket site has been configured successfully. You can start
          using it now and adjust email, moderation, and other settings later in
          the admin panel.
        </p>
      </div>
      <div className="grid w-full max-w-sm grid-cols-1 gap-3 sm:grid-cols-2">
        <Button
          onClick={handleGoToDashboard}
          className="h-auto w-full min-w-0 whitespace-normal py-2 text-center leading-tight"
        >
          Go to Dashboard
        </Button>
        <Link href="/admin" className="w-full min-w-0">
          <Button
            type="button"
            variant="outline"
            className="h-auto w-full min-w-0 whitespace-normal border-white/20 py-2 text-center leading-tight text-white hover:bg-white/10"
          >
            Configure in Admin
          </Button>
        </Link>
      </div>
    </div>
  );
}
