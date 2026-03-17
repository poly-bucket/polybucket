import type { Metadata } from "next";
import { fetchUserProfile } from "@/lib/services/userProfileService";
import { ProfilePageContent } from "@/components/profile/profile-page-content";

type PageProps = {
  params: Promise<{ idOrUsername: string }>;
};

export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  try {
    const { idOrUsername } = await params;
    const profile = await fetchUserProfile(idOrUsername);
    const username = profile.username ?? idOrUsername;
    return {
      title: `${username}'s Profile | Polybucket`,
      description: profile.bio?.slice(0, 160) ?? undefined,
    };
  } catch {
    return { title: "Profile | Polybucket" };
  }
}

export default function ProfilePage() {
  return <ProfilePageContent />;
}
