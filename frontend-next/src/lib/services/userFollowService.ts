import axiosInstance from "@/lib/api/axiosConfig";

export interface UserFollowListItem {
  id: string;
  username: string;
  avatar?: string;
  followedAt?: string;
}

export interface UserFollowListPage {
  users: UserFollowListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface FollowerHistoryPoint {
  date: string;
  followerCount: number;
}

export interface FollowerHistoryResult {
  days: number;
  points: FollowerHistoryPoint[];
}

export async function followUser(userId: string): Promise<void> {
  await axiosInstance.post(`/api/users/${userId}/follow`);
}

export async function unfollowUser(userId: string): Promise<void> {
  await axiosInstance.delete(`/api/users/${userId}/follow`);
}

export async function fetchUserFollowers(
  username: string,
  page: number,
  pageSize: number
): Promise<UserFollowListPage> {
  const { data } = await axiosInstance.get<UserFollowListPage>(
    `/api/users/profile/${encodeURIComponent(username)}/followers`,
    { params: { page, pageSize } }
  );
  return {
    users: data.users ?? [],
    totalCount: data.totalCount ?? 0,
    page: data.page ?? page,
    pageSize: data.pageSize ?? pageSize,
    totalPages: data.totalPages ?? 1,
  };
}

export async function fetchUserFollowing(
  username: string,
  page: number,
  pageSize: number
): Promise<UserFollowListPage> {
  const { data } = await axiosInstance.get<UserFollowListPage>(
    `/api/users/profile/${encodeURIComponent(username)}/following`,
    { params: { page, pageSize } }
  );
  return {
    users: data.users ?? [],
    totalCount: data.totalCount ?? 0,
    page: data.page ?? page,
    pageSize: data.pageSize ?? pageSize,
    totalPages: data.totalPages ?? 1,
  };
}

export async function fetchFollowerHistory(
  userId: string,
  days: 7 | 30 | 90
): Promise<FollowerHistoryResult> {
  const { data } = await axiosInstance.get<FollowerHistoryResult>(
    `/api/users/${userId}/followers/history`,
    { params: { days } }
  );
  return {
    days: data.days ?? days,
    points: data.points ?? [],
  };
}
