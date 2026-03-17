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
    <div className="flex flex-col items-center text-center space-y-6">
      <div className="relative">
        <div className="size-16 rounded-full bg-green-500/20 flex items-center justify-center">
          <Check className="size-8 text-green-400" />
        </div>
      </div>
      <div>
        <h2 className="text-2xl font-semibold text-white">Setup complete</h2>
        <p className="mt-2 text-sm text-white/70">
          Your PolyBucket site has been configured successfully. You can start
          using it now and adjust email, moderation, and other settings later in
          the admin panel.
        </p>
      </div>
      <div className="flex flex-col sm:flex-row gap-3 w-full max-w-xs">
        <Button onClick={handleGoToDashboard} className="w-full">
          Go to Dashboard
        </Button>
        <Link href="/admin" className="w-full">
          <Button
            type="button"
            variant="outline"
            className="w-full border-white/20 text-white hover:bg-white/10"
          >
            Configure in Admin
          </Button>
        </Link>
      </div>
    </div>
  );
}
