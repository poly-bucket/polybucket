import { PrivacySettings } from "@/lib/api/client";

export interface ParsedModelData {
  title?: string;
  description?: string;
  privacy?: PrivacySettings;
  license?: string;
  categories?: string[];
  aiGenerated?: boolean;
  workInProgress?: boolean;
  nsfw?: boolean;
  remix?: boolean;
}

export function parseModelMarkdown(
  markdownContent: string
): ParsedModelData {
  const result: ParsedModelData = {};
  const lines = markdownContent.split("\n").map((line) => line.trim());

  const titleMatch = lines.find((line) => line.match(/^#\s+(.+)$/));
  if (titleMatch) {
    const match = titleMatch.match(/^#\s+(.+)$/);
    if (match) result.title = match[1].trim();
  }

  let descriptionStartIndex = -1;
  let descriptionEndIndex = -1;

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    if (
      line.match(/^##\s+(.+)$/) &&
      !line.toLowerCase().includes("privacy:") &&
      !line.toLowerCase().includes("license:") &&
      !line.toLowerCase().includes("categories:") &&
      descriptionStartIndex === -1
    ) {
      const headerContent = line.replace(/^##\s+/, "");
      if (!headerContent.includes(":")) {
        descriptionStartIndex = i;
        for (let j = i + 1; j < lines.length; j++) {
          if (lines[j].match(/^#+\s/) || lines[j] === "---") {
            descriptionEndIndex = j;
            break;
          }
        }
        if (descriptionEndIndex === -1)
          descriptionEndIndex = lines.length;
        break;
      }
    }
  }

  if (descriptionStartIndex !== -1) {
    const descriptionLines = lines.slice(
      descriptionStartIndex,
      descriptionEndIndex
    );
    const description = descriptionLines
      .slice(1)
      .filter((line) => line.length > 0)
      .join(" ")
      .trim();
    if (description) result.description = description;
  }

  const privacyMatch = lines.find((line) =>
    line.toLowerCase().match(/^##\s*privacy\s*:\s*(.+)$/i)
  );
  if (privacyMatch) {
    const match = privacyMatch
      .toLowerCase()
      .match(/^##\s*privacy\s*:\s*(.+)$/i);
    if (match) {
      const privacyValue = match[1].trim().toLowerCase();
      if (privacyValue === "public")
        result.privacy = PrivacySettings.Public;
      else if (privacyValue === "private")
        result.privacy = PrivacySettings.Private;
      else if (privacyValue === "unlisted")
        result.privacy = PrivacySettings.Unlisted;
    }
  }

  const licenseMatch = lines.find((line) =>
    line.toLowerCase().match(/^##\s*license\s*:\s*(.+)$/i)
  );
  if (licenseMatch) {
    const match = licenseMatch.match(/^##\s*license\s*:\s*(.+)$/i);
    if (match) result.license = match[1].trim();
  }

  const categoriesMatch = lines.find((line) =>
    line.toLowerCase().match(/^##\s*categories\s*:\s*(.+)$/i)
  );
  if (categoriesMatch) {
    const match = categoriesMatch.match(/^##\s*categories\s*:\s*(.+)$/i);
    if (match) {
      const categories = match[1]
        .split(",")
        .map((cat) => cat.trim())
        .filter((cat) => cat.length > 0);
      if (categories.length > 0) result.categories = categories;
    }
  }

  const tagsMatch = lines.find((line) =>
    line.toLowerCase().match(/^###\s*tags\s*:\s*(.+)$/i)
  );
  if (tagsMatch) {
    const match = tagsMatch.match(/^###\s*tags\s*:\s*(.+)$/i);
    if (match) {
      const tags = match[1]
        .toLowerCase()
        .split(",")
        .map((tag) => tag.trim())
        .filter((tag) => tag.length > 0);
      if (
        tags.includes("aigenerated") ||
        tags.includes("ai-generated") ||
        tags.includes("ai_generated")
      )
        result.aiGenerated = true;
      if (
        tags.includes("wip") ||
        tags.includes("work-in-progress") ||
        tags.includes("workinprogress")
      )
        result.workInProgress = true;
      if (tags.includes("nsfw") || tags.includes("adult"))
        result.nsfw = true;
      if (tags.includes("remix") || tags.includes("remixed"))
        result.remix = true;
    }
  }

  return result;
}

export function isMarkdownFile(filename: string): boolean {
  return (
    filename.toLowerCase().endsWith(".md") ||
    filename.toLowerCase().endsWith(".markdown")
  );
}

export function generateMarkdownTemplate(): string {
  return `# Your Model Title Here

## Your detailed model description goes here. Explain what makes your model special, how it should be used, any special features, etc.

## Privacy: public

## License: Creative Commons

## Categories: art, technology, toys

### Tags: aiGenerated, remix

---

## Additional Information (Optional)

You can add any additional information below this line. This section will not be parsed but can contain:

- Print settings
- Assembly instructions  
- Credits and attributions
- Version history
- etc.
`;
}
