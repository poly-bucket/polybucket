import { ApiClientFactory } from "@/lib/api/clientFactory";
import type {
  UserModelListItemDto,
  GetUserModelsResult,
  GetPublicUserCollectionsResult,
} from "@/lib/api/client";

const UUID_REGEX = /^[a-f0-9-]{36}$/i;

export interface UserProfileData {
  id?: string;
  username?: string;
  firstName?: string;
  lastName?: string;
  bio?: string;
  avatar?: string;
  profilePictureUrl?: string;
  country?: string;
  roleName?: string;
  createdAt?: Date | string;
  lastLoginAt?: Date | string;
  isBanned?: boolean;
  bannedAt?: Date | string;
  banReason?: string;
  banExpiresAt?: Date | string;
  totalModels?: number;
  totalCollections?: number;
  totalLikes?: number;
  totalDownloads?: number;
  totalFollowers?: number;
  totalFollowing?: number;
  isProfilePublic?: boolean;
  showEmail?: boolean;
  showLastLogin?: boolean;
  showStatistics?: boolean;
  websiteUrl?: string;
  twitterUrl?: string;
  instagramUrl?: string;
  youtubeUrl?: string;
  message?: string;
}

export async function fetchUserProfile(
  idOrUsername: string
): Promise<UserProfileData> {
  const client = ApiClientFactory.getApiClient();
  const isUuid = UUID_REGEX.test(idOrUsername);

  const raw = isUuid
    ? await client.getUserProfile_GetUserProfileById(idOrUsername)
    : await client.getUserProfile_GetUserProfileByUsername(idOrUsername);

  return raw as unknown as UserProfileData;
}

export async function fetchUserModels(
  username: string,
  page: number,
  pageSize: number
): Promise<GetUserModelsResult> {
  const client = ApiClientFactory.getApiClient();
  return client.getUserModels_GetUserPublicModels(username, page, pageSize);
}

export async function fetchUserCollections(
  username: string,
  page: number,
  pageSize: number,
  search?: string
): Promise<GetPublicUserCollectionsResult> {
  const client = ApiClientFactory.getApiClient();
  return client.getPublicUserCollections_GetPublicUserCollectionsByUsername(
    username,
    page,
    pageSize,
    search ?? undefined,
    "CreatedAt",
    true
  );
}

export type { UserModelListItemDto };
