export interface UploadDefaults {
  privacy: "Public" | "Private" | "Unlisted";
  license: string;
  aiGenerated: boolean;
  workInProgress: boolean;
  nsfw: boolean;
  remix: boolean;
}

export interface CollectionDefaults {
  visibility: "Public" | "Private" | "Unlisted";
}

const UPLOAD_KEY = "polybucket.uploadDefaults.v1";
const COLLECTION_KEY = "polybucket.collectionDefaults.v1";

const uploadDefaults: UploadDefaults = {
  privacy: "Public",
  license: "MIT",
  aiGenerated: false,
  workInProgress: false,
  nsfw: false,
  remix: false,
};

const collectionDefaults: CollectionDefaults = {
  visibility: "Private",
};

export function getUploadDefaults(): UploadDefaults {
  if (typeof window === "undefined") {
    return uploadDefaults;
  }
  try {
    const raw = window.localStorage.getItem(UPLOAD_KEY);
    if (!raw) {
      return uploadDefaults;
    }
    const parsed = JSON.parse(raw) as Partial<UploadDefaults>;
    return {
      privacy: parsed.privacy ?? uploadDefaults.privacy,
      license: parsed.license ?? uploadDefaults.license,
      aiGenerated: parsed.aiGenerated ?? uploadDefaults.aiGenerated,
      workInProgress: parsed.workInProgress ?? uploadDefaults.workInProgress,
      nsfw: parsed.nsfw ?? uploadDefaults.nsfw,
      remix: parsed.remix ?? uploadDefaults.remix,
    };
  } catch {
    return uploadDefaults;
  }
}

export function setUploadDefaults(next: UploadDefaults): void {
  if (typeof window === "undefined") {
    return;
  }
  window.localStorage.setItem(UPLOAD_KEY, JSON.stringify(next));
}

export function getCollectionDefaults(): CollectionDefaults {
  if (typeof window === "undefined") {
    return collectionDefaults;
  }
  try {
    const raw = window.localStorage.getItem(COLLECTION_KEY);
    if (!raw) {
      return collectionDefaults;
    }
    const parsed = JSON.parse(raw) as Partial<CollectionDefaults>;
    return {
      visibility: parsed.visibility ?? collectionDefaults.visibility,
    };
  } catch {
    return collectionDefaults;
  }
}

export function setCollectionDefaults(next: CollectionDefaults): void {
  if (typeof window === "undefined") {
    return;
  }
  window.localStorage.setItem(COLLECTION_KEY, JSON.stringify(next));
}
