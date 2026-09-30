import axiosInstance from "@/lib/api/axiosConfig";

export type NotificationType =
  | "ModelUploaded"
  | "ModelLiked"
  | "ModelDownloaded"
  | "CommentAdded"
  | "CommentLiked"
  | "CollectionCreated"
  | "CollectionUpdated"
  | "UserFollowed"
  | "SystemMaintenance"
  | "SystemUpdate"
  | "SecurityAlert"
  | "Welcome"
  | "Custom"
  | "ModelApproved"
  | "ModelRejected"
  | "CommentReplied";

export type NotificationPriority = "Low" | "Normal" | "High" | "Urgent";

export interface NotificationItem {
  id: string;
  type: NotificationType;
  priority: NotificationPriority;
  title: string;
  message: string;
  actionUrl?: string | null;
  relatedEntityId?: string | null;
  relatedEntityType?: string | null;
  isRead: boolean;
  readAt?: string | null;
  createdAt: string;
}

export interface NotificationsPage {
  items: NotificationItem[];
  totalCount: number;
  unreadCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const NOTIFICATIONS_MAX_PAGE_SIZE = 50;

export const notificationsService = {
  async getNotifications(page = 1, pageSize = 20, unreadOnly = false): Promise<NotificationsPage> {
    const response = await axiosInstance.get<NotificationsPage>("/api/notifications", {
      params: { page, pageSize, unreadOnly },
    });
    return response.data;
  },

  async getUnreadCount(): Promise<number> {
    const response = await axiosInstance.get<{ count: number }>("/api/notifications/unread-count");
    return response.data.count;
  },

  async markRead(notificationId: string): Promise<void> {
    await axiosInstance.post(`/api/notifications/${notificationId}/read`);
  },

  async markAllRead(): Promise<number> {
    const response = await axiosInstance.post<{ updated: number }>("/api/notifications/read-all");
    return response.data.updated;
  },
};

export function formatNotificationTime(createdAt: string, now: Date = new Date()): string {
  const seconds = Math.max(0, Math.floor((now.getTime() - new Date(createdAt).getTime()) / 1000));
  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return new Date(createdAt).toLocaleDateString();
}
