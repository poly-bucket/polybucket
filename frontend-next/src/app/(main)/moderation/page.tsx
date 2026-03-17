"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function ModerationPage() {
  const router = useRouter();

  useEffect(() => {
    router.replace("/moderation/reports");
  }, [router]);

  return null;
}
