import { describe, it, expect, vi, beforeEach } from "vitest";
import userEvent from "@testing-library/user-event";
import { AxiosError, AxiosHeaders } from "axios";
import { render, screen, waitFor, within } from "@/test/test-utils";
import type { AuthUser } from "@/lib/auth/authSession";
import type { CommentItem, CommentsPage } from "@/lib/services/commentsService";
import { ModelComments } from "./model-comments";

const mockGetComments = vi.fn();
const mockCreateComment = vi.fn();
const mockReact = vi.fn();
const mockRemoveReaction = vi.fn();
const mockReport = vi.fn();
const mockToastError = vi.fn();

vi.mock("sonner", () => ({
  toast: { error: (message: string) => mockToastError(message), success: vi.fn() },
}));

vi.mock("@/lib/services/commentsService", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@/lib/services/commentsService")>()),
  getComments: (...args: unknown[]) => mockGetComments(...args),
  createComment: (...args: unknown[]) => mockCreateComment(...args),
  reactToComment: (...args: unknown[]) => mockReact(...args),
  removeCommentReaction: (...args: unknown[]) => mockRemoveReaction(...args),
  reportComment: (...args: unknown[]) => mockReport(...args),
}));

const signedInUser: AuthUser = {
  id: "user-1",
  email: "maker@example.com",
  username: "maker",
  accessToken: "access-token",
  isEmailVerified: true,
};

function comment(overrides: Partial<CommentItem> = {}): CommentItem {
  return {
    id: "c1",
    content: "Printed great in PETG",
    authorId: "author-1",
    authorUsername: "printer",
    target: { targetId: "model-1", targetType: "Model" },
    likes: 2,
    dislikes: 0,
    isEdited: false,
    isModerated: false,
    isHidden: false,
    parentCommentId: null,
    createdAt: "2026-09-01T00:00:00Z",
    lastEditedAt: null,
    userHasLiked: false,
    userHasDisliked: false,
    canEdit: false,
    canDelete: false,
    replies: [],
    ...overrides,
  };
}

function page(comments: CommentItem[], overrides: Partial<CommentsPage> = {}): CommentsPage {
  return {
    comments,
    totalCount: comments.length,
    page: 1,
    pageSize: 20,
    totalPages: comments.length > 0 ? 1 : 0,
    statistics: { totalComments: comments.length, totalReplies: 0, totalLikes: 0, totalDislikes: 0, moderatedComments: 0 },
    ...overrides,
  };
}

describe("ModelComments", () => {
  beforeEach(() => {
    mockGetComments.mockReset().mockResolvedValue(page([comment()]));
    mockCreateComment.mockReset();
    mockReact.mockReset();
    mockRemoveReaction.mockReset();
    mockReport.mockReset();
    mockToastError.mockReset();
  });

  it("prompts anonymous visitors to sign in without loading comments", async () => {
    // Arrange
    const props = { modelId: "model-1" };

    // Act
    render(<ModelComments {...props} />, { mockAuth: { user: null } });

    // Assert
    expect(screen.getByRole("link", { name: /sign in/i })).toHaveAttribute("href", "/login");
    await waitFor(() => expect(mockGetComments).not.toHaveBeenCalled());
  });

  it("loads and lists comments for the model", async () => {
    // Arrange
    const props = { modelId: "model-1" };

    // Act
    render(<ModelComments {...props} />, { mockAuth: { user: signedInUser } });

    // Assert
    expect(await screen.findByRole("article", { name: /comment by printer/i })).toHaveTextContent("Printed great in PETG");
    expect(mockGetComments).toHaveBeenCalledWith({ targetId: "model-1", targetType: "Model" }, 1, 20);
    expect(screen.getByText(/comments \(1\)/i)).toBeInTheDocument();
  });

  it("posts a trimmed comment and shows it at the top", async () => {
    // Arrange
    mockGetComments.mockResolvedValue(page([]));
    mockCreateComment.mockResolvedValue(comment({ id: "c2", content: "Nice model", authorUsername: "maker", canDelete: true }));
    const user = userEvent.setup();
    render(<ModelComments modelId="model-1" />, { mockAuth: { user: signedInUser } });
    await screen.findByText(/no comments yet/i);

    // Act
    await user.type(screen.getByRole("textbox", { name: /write a comment/i }), "  Nice model  ");
    await user.click(screen.getByRole("button", { name: /post comment/i }));

    // Assert
    expect(mockCreateComment).toHaveBeenCalledWith({ targetId: "model-1", targetType: "Model" }, "Nice model");
    expect(await screen.findByRole("article", { name: /comment by maker/i })).toHaveTextContent("Nice model");
    expect(screen.getByRole("textbox", { name: /write a comment/i })).toHaveValue("");
  });

  it("likes a comment and then removes the like on a second click", async () => {
    // Arrange
    mockReact.mockResolvedValue({ likes: 3, dislikes: 0, userHasLiked: true, userHasDisliked: false, changed: true });
    mockRemoveReaction.mockResolvedValue({ likes: 2, dislikes: 0, userHasLiked: false, userHasDisliked: false, changed: true });
    const user = userEvent.setup();
    render(<ModelComments modelId="model-1" />, { mockAuth: { user: signedInUser } });
    const article = await screen.findByRole("article", { name: /comment by printer/i });

    // Act
    await user.click(within(article).getByRole("button", { name: /^like/i }));
    const liked = await within(article).findByRole("button", { name: /like \(3\)/i });
    await user.click(liked);

    // Assert
    expect(mockReact).toHaveBeenCalledWith("c1", "like");
    expect(mockRemoveReaction).toHaveBeenCalledWith("c1", "like");
    expect(await within(article).findByRole("button", { name: /^like \(2\)/i })).toHaveAttribute("aria-pressed", "false");
  });

  it("shows a verification message when an unverified user tries to comment", async () => {
    // Arrange
    mockGetComments.mockResolvedValue(page([]));
    mockCreateComment.mockRejectedValue(
      new AxiosError("Forbidden", "ERR_BAD_REQUEST", undefined, undefined, {
        status: 403,
        statusText: "Forbidden",
        data: { code: "email_unverified" },
        headers: {},
        config: { headers: new AxiosHeaders() },
      })
    );
    const user = userEvent.setup();
    render(<ModelComments modelId="model-1" />, { mockAuth: { user: { ...signedInUser, isEmailVerified: false } } });
    await screen.findByText(/no comments yet/i);

    // Act
    await user.type(screen.getByRole("textbox", { name: /write a comment/i }), "Hello");
    await user.click(screen.getByRole("button", { name: /post comment/i }));

    // Assert
    await waitFor(() => expect(mockToastError).toHaveBeenCalledWith("Verify your email address to join the discussion."));
    expect(screen.getByRole("textbox", { name: /write a comment/i })).toHaveValue("Hello");
  });

  it("replies to a top-level comment and shows the reply beneath it", async () => {
    // Arrange
    mockCreateComment.mockResolvedValue(comment({ id: "r1", content: "Thanks!", authorUsername: "maker", parentCommentId: "c1" }));
    const user = userEvent.setup();
    render(<ModelComments modelId="model-1" />, { mockAuth: { user: signedInUser } });
    const article = await screen.findByRole("article", { name: /comment by printer/i });

    // Act
    await user.click(within(article).getByRole("button", { name: /reply/i }));
    await user.type(screen.getByRole("textbox", { name: /reply to printer/i }), "Thanks!");
    await user.click(screen.getByRole("button", { name: /post reply/i }));

    // Assert
    expect(mockCreateComment).toHaveBeenCalledWith({ targetId: "model-1", targetType: "Model" }, "Thanks!", "c1");
    expect(await screen.findByRole("article", { name: /comment by maker/i })).toHaveTextContent("Thanks!");
    expect(screen.queryByRole("textbox", { name: /reply to printer/i })).not.toBeInTheDocument();
  });

  it("reports a comment with the reason the user enters", async () => {
    // Arrange
    mockReport.mockResolvedValue(undefined);
    vi.spyOn(window, "prompt").mockReturnValue("  Spam  ");
    const user = userEvent.setup();
    render(<ModelComments modelId="model-1" />, { mockAuth: { user: signedInUser } });
    const article = await screen.findByRole("article", { name: /comment by printer/i });

    // Act
    await user.click(within(article).getByRole("button", { name: /report/i }));

    // Assert
    await waitFor(() => expect(mockReport).toHaveBeenCalledWith("c1", "Spam"));
  });

  it("offers to load more when there are further pages", async () => {
    // Arrange
    mockGetComments
      .mockResolvedValueOnce(page([comment()], { totalCount: 2, totalPages: 2 }))
      .mockResolvedValueOnce(page([comment({ id: "c3", authorUsername: "second" })], { page: 2, totalCount: 2, totalPages: 2 }));
    const user = userEvent.setup();
    render(<ModelComments modelId="model-1" />, { mockAuth: { user: signedInUser } });

    // Act
    await user.click(await screen.findByRole("button", { name: /load more comments/i }));

    // Assert
    expect(await screen.findByRole("article", { name: /comment by second/i })).toBeInTheDocument();
    expect(screen.getByRole("article", { name: /comment by printer/i })).toBeInTheDocument();
    expect(mockGetComments).toHaveBeenLastCalledWith({ targetId: "model-1", targetType: "Model" }, 2, 20);
    expect(screen.queryByRole("button", { name: /load more comments/i })).not.toBeInTheDocument();
  });
});
