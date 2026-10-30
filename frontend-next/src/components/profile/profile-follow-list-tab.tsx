"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { UserAvatar } from "@/components/layout/user-avatar";
import { SimplePagination } from "@/components/collections/simple-pagination";
import {
  fetchUserFollowers,
  fetchUserFollowing,
  type UserFollowListItem,
} from "@/lib/services/userFollowService";

const PAGE_SIZE = 20;

interface ProfileFollowListTabProps {
  username: string;
  mode: "followers" | "following";
  page: number;
  onPageChange: (page: number) => void;
}

export function ProfileFollowListTab({
  username,
  mode,
  page,
  onPageChange,
}: ProfileFollowListTabProps) {
  const [users, setUsers] = useState<UserFollowListItem[]>([]);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const fetcher = mode === "followers" ? fetchUserFollowers : fetchUserFollowing;
      const response = await fetcher(username, page, PAGE_SIZE);
      setUsers(response.users);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch {
      setError(`Failed to load ${mode}.`);
      setUsers([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [username, mode, page]);

  useEffect(() => {
    load();
  }, [load]);

  if (loading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <div key={i} className="h-14 animate-pulse rounded-lg bg-white/10" />
        ))}
      </div>
    );
  }

  if (users.length === 0) {
    return (
      <div className="rounded-xl border border-white/20 bg-white/5 px-8 py-12 text-center">
        <p className="text-white/60">{error ?? `No ${mode} yet`}</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <ul className="divide-y divide-white/10 rounded-xl border border-white/20 bg-white/5">
        {users.map((user) => (
          <li key={user.id}>
            <Link
              href={`/profile/${user.username}`}
              className="flex items-center gap-3 px-4 py-3 transition-colors hover:bg-white/10"
            >
              <UserAvatar username={user.username} avatar={user.avatar} size="sm" />
              <span className="font-medium text-white">@{user.username}</span>
            </Link>
          </li>
        ))}
      </ul>
      {totalPages > 1 && (
        <SimplePagination
          page={page}
          totalPages={totalPages}
          totalCount={totalCount}
          onPageChange={onPageChange}
        />
      )}
    </div>
  );
}
