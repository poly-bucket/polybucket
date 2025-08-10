import { PrivacySettings } from '../services/api.client';

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

/**
 * Parses a markdown file and extracts model metadata
 * Supports a flexible format where all fields are optional
 * 
 * Expected format:
 * # Title
 * ## Description
 * ## Privacy: public|private|unlisted
 * ## License: license name
 * ## Categories: comma,separated,values
 * ### Tags: aiGenerated,wip,NSFW,remix
 */
export const parseModelMarkdown = (markdownContent: string): ParsedModelData => {
  const result: ParsedModelData = {};
  
  // Split content into lines for processing
  const lines = markdownContent.split('\n').map(line => line.trim());
  
  // Parse title (first # header)
  const titleMatch = lines.find(line => line.match(/^#\s+(.+)$/));
  if (titleMatch) {
    const match = titleMatch.match(/^#\s+(.+)$/);
    if (match) {
      result.title = match[1].trim();
    }
  }
  
  // Parse description (first ## header without a field name)
  let descriptionStartIndex = -1;
  let descriptionEndIndex = -1;
  
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i];
    
    // Find first ## header that doesn't match known field patterns
    if (line.match(/^##\s+(.+)$/) && 
        !line.toLowerCase().includes('privacy:') &&
        !line.toLowerCase().includes('license:') &&
        !line.toLowerCase().includes('categories:') &&
        descriptionStartIndex === -1) {
      
      // Check if this looks like a field (contains a colon)
      const headerContent = line.replace(/^##\s+/, '');
      if (!headerContent.includes(':')) {
        descriptionStartIndex = i;
        
        // Find where description ends (next header or empty lines)
        for (let j = i + 1; j < lines.length; j++) {
          if (lines[j].match(/^#+\s/) || lines[j] === '---') {
            descriptionEndIndex = j;
            break;
          }
        }
        
        if (descriptionEndIndex === -1) {
          descriptionEndIndex = lines.length;
        }
        break;
      }
    }
  }
  
  if (descriptionStartIndex !== -1) {
    const descriptionLines = lines.slice(descriptionStartIndex, descriptionEndIndex);
    // Remove the header line and join the rest
    const description = descriptionLines.slice(1)
      .filter(line => line.length > 0)
      .join(' ')
      .trim();
    
    if (description) {
      result.description = description;
    }
  }
  
  // Parse privacy
  const privacyMatch = lines.find(line => 
    line.toLowerCase().match(/^##\s*privacy\s*:\s*(.+)$/i)
  );
  if (privacyMatch) {
    const match = privacyMatch.toLowerCase().match(/^##\s*privacy\s*:\s*(.+)$/i);
    if (match) {
      const privacyValue = match[1].trim().toLowerCase();
      switch (privacyValue) {
        case 'public':
          result.privacy = PrivacySettings.Public;
          break;
        case 'private':
          result.privacy = PrivacySettings.Private;
          break;
        case 'unlisted':
          result.privacy = PrivacySettings.Unlisted;
          break;
      }
    }
  }
  
  // Parse license
  const licenseMatch = lines.find(line => 
    line.toLowerCase().match(/^##\s*license\s*:\s*(.+)$/i)
  );
  if (licenseMatch) {
    const match = licenseMatch.match(/^##\s*license\s*:\s*(.+)$/i);
    if (match) {
      result.license = match[1].trim();
    }
  }
  
  // Parse categories
  const categoriesMatch = lines.find(line => 
    line.toLowerCase().match(/^##\s*categories\s*:\s*(.+)$/i)
  );
  if (categoriesMatch) {
    const match = categoriesMatch.match(/^##\s*categories\s*:\s*(.+)$/i);
    if (match) {
      const categories = match[1]
        .split(',')
        .map(cat => cat.trim())
        .filter(cat => cat.length > 0);
      
      if (categories.length > 0) {
        result.categories = categories;
      }
    }
  }
  
  // Parse tags
  const tagsMatch = lines.find(line => 
    line.toLowerCase().match(/^###\s*tags\s*:\s*(.+)$/i)
  );
  if (tagsMatch) {
    const match = tagsMatch.match(/^###\s*tags\s*:\s*(.+)$/i);
    if (match) {
      const tags = match[1]
        .toLowerCase()
        .split(',')
        .map(tag => tag.trim())
        .filter(tag => tag.length > 0);
      
      // Map tags to boolean flags
      if (tags.includes('aigenerated') || tags.includes('ai-generated') || tags.includes('ai_generated')) {
        result.aiGenerated = true;
      }
      
      if (tags.includes('wip') || tags.includes('work-in-progress') || tags.includes('workinprogress')) {
        result.workInProgress = true;
      }
      
      if (tags.includes('nsfw') || tags.includes('adult')) {
        result.nsfw = true;
      }
      
      if (tags.includes('remix') || tags.includes('remixed')) {
        result.remix = true;
      }
    }
  }
  
  return result;
};

/**
 * Validates if a file is a markdown file
 */
export const isMarkdownFile = (filename: string): boolean => {
  return filename.toLowerCase().endsWith('.md') || filename.toLowerCase().endsWith('.markdown');
};

/**
 * Generates a sample markdown template for users
 */
export const generateMarkdownTemplate = (): string => {
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
};
