import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ModelReactions } from "./model-reactions";

const mockReact = jest.fn();
const mockRemove = jest.fn();

jest.mock("@/lib/services/modelReactionsService", () => ({
  reactToModel: (...args: unknown[]) => mockReact(...args),
  removeModelReaction: (...args: unknown[]) => mockRemove(...args),
}));

describe("ModelReactions", () => {
  const onUpdate = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    mockReact.mockResolvedValue({
      likes: 2,
      dislikes: 0,
      userHasLiked: true,
      userHasDisliked: false,
      changed: true,
    });
    mockRemove.mockResolvedValue({
      likes: 1,
      dislikes: 0,
      userHasLiked: false,
      userHasDisliked: false,
      changed: true,
    });
  });

  it("likes a model and then removes the like on a second click", async () => {
    const user = userEvent.setup();
    render(
      <ModelReactions
        modelId="m1"
        reactions={{ likes: 1, dislikes: 0, userHasLiked: false, userHasDisliked: false }}
        isOwner={false}
        isFederated={false}
        isAuthenticated={true}
        onUpdate={onUpdate}
      />
    );

    await user.click(screen.getByRole("button", { name: /^like/i }));
    expect(mockReact).toHaveBeenCalledWith("m1", "like");

    render(
      <ModelReactions
        modelId="m1"
        reactions={{ likes: 2, dislikes: 0, userHasLiked: true, userHasDisliked: false }}
        isOwner={false}
        isFederated={false}
        isAuthenticated={true}
        onUpdate={onUpdate}
      />
    );
    await user.click(screen.getByRole("button", { name: /like \(2\)/i }));
    expect(mockRemove).toHaveBeenCalledWith("m1", "like");
  });

  it("does not allow the owner to react", () => {
    render(
      <ModelReactions
        modelId="m1"
        reactions={{ likes: 0, dislikes: 0, userHasLiked: false, userHasDisliked: false }}
        isOwner={true}
        isFederated={false}
        isAuthenticated={true}
        onUpdate={onUpdate}
      />
    );

    expect(screen.getByRole("button", { name: /^like/i })).toBeDisabled();
    expect(screen.getByRole("button", { name: /^dislike/i })).toBeDisabled();
  });
});
