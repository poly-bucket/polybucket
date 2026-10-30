export type RegistrationStatusResponse = {
  allowUserRegistration?: boolean;
};

export async function fetchRegistrationStatus(): Promise<boolean | null> {
  try {
    const res = await fetch("/api/polybucket/auth/registration-status", {
      cache: "no-store",
    });
    if (!res.ok) {
      return null;
    }
    const data = (await res.json()) as RegistrationStatusResponse;
    return data.allowUserRegistration === true;
  } catch {
    return null;
  }
}

export function getRegisterApiErrorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === "object" && "message" in err) {
    const message = (err as { message?: string }).message;
    if (typeof message === "string" && message.trim()) {
      return message;
    }
  }
  return fallback;
}
