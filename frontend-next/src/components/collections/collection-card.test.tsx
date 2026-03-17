import { describe, it, expect, vi } from "vitest";
import userEvent from "@testing-library/user-event";
import { render, screen } from "@/test/test-utils";
import {
  CollectionCard,
  mapPublicUserCollectionDtoToCardData,
  mapCollectionToCardData,
  type CollectionCardData,
} from "./collection-card";

const baseCollection: CollectionCardData = {
  id: "c1",
  name: "Test Collection",
  description: "A test",
  visibility: "Public",
  modelCount: 3,
};

describe("CollectionCard", () => {
  it("renders with CollectionCardData and links to collection detail", () => {
    render(<CollectionCard collection={baseCollection} />);

    expect(screen.getByText("Test Collection")).toBeInTheDocument();
    expect(screen.getByText("3 models")).toBeInTheDocument();
    const link = screen.getByRole("link", { name: /test collection/i });
    expect(link).toHaveAttribute("href", "/collections/c1");
  });

  it("renders owner when showOwner is true", () => {
    const collection = { ...baseCollection, owner: { id: "u1", username: "alice" } };
    render(<CollectionCard collection={collection} showOwner />);

    expect(screen.getByRole("link", { name: "alice" })).toBeInTheDocument();
  });

  it("shows dropdown with Edit when onEdit is provided", async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();
    render(<CollectionCard collection={baseCollection} onEdit={onEdit} />);

    const menuButton = screen.getByRole("button");
    await user.click(menuButton);
    const editItem = await screen.findByRole("menuitem", { name: /edit/i });
    await user.click(editItem);

    expect(onEdit).toHaveBeenCalledWith(baseCollection);
  });

  it("shows dropdown with Delete when onDelete is provided", async () => {
    const user = userEvent.setup();
    const onDelete = vi.fn();
    render(<CollectionCard collection={baseCollection} onDelete={onDelete} />);

    const menuButton = screen.getByRole("button");
    await user.click(menuButton);
    const deleteItem = await screen.findByRole("menuitem", { name: /delete/i });
    await user.click(deleteItem);

    expect(onDelete).toHaveBeenCalledWith(baseCollection);
  });

  it("shows Pin to sidebar when onTogglePin is provided and not favorite", async () => {
    const user = userEvent.setup();
    const onTogglePin = vi.fn();
    render(<CollectionCard collection={baseCollection} onTogglePin={onTogglePin} />);

    const menuButton = screen.getByRole("button");
    await user.click(menuButton);
    const pinItem = await screen.findByRole("menuitem", { name: /pin to sidebar/i });
    await user.click(pinItem);

    expect(onTogglePin).toHaveBeenCalledWith(baseCollection);
  });

  it("uses thumbnail when thumbnailUrl is provided", () => {
    const collection = { ...baseCollection, thumbnailUrl: "https://example.com/img.png" };
    render(<CollectionCard collection={collection} />);

    const img = screen.getByRole("img", { name: "Test Collection" });
    expect(img).toHaveAttribute("src", "https://example.com/img.png");
  });
});

describe("mapPublicUserCollectionDtoToCardData", () => {
  it("maps DTO to CollectionCardData", () => {
    const dto = {
      id: "c1",
      name: "Public",
      description: "Desc",
      visibility: "Public",
      modelCount: 5,
    };
    const result = mapPublicUserCollectionDtoToCardData(dto);
    expect(result.id).toBe("c1");
    expect(result.name).toBe("Public");
    expect(result.modelCount).toBe(5);
  });
});

describe("mapCollectionToCardData", () => {
  it("maps Collection to CollectionCardData with thumbnail from first model", () => {
    const collection = {
      id: "c1",
      name: "Col",
      collectionModels: [{ model: { thumbnailUrl: "https://thumb.png" } }],
      favorite: true,
    };
    const result = mapCollectionToCardData(collection);
    expect(result.thumbnailUrl).toBe("https://thumb.png");
    expect(result.favorite).toBe(true);
  });
});
