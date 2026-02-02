import { parseModelMarkdown, isMarkdownFile, generateMarkdownTemplate } from '../markdownParser';
import { PrivacySettings } from '../api/client';

describe('markdownParser', () => {
  describe('isMarkdownFile', () => {
    it('should detect markdown files', () => {
      expect(isMarkdownFile('model.md')).toBe(true);
      expect(isMarkdownFile('MODEL.MD')).toBe(true);
      expect(isMarkdownFile('model.markdown')).toBe(true);
      expect(isMarkdownFile('model.txt')).toBe(false);
      expect(isMarkdownFile('model.stl')).toBe(false);
    });
  });

  describe('parseModelMarkdown', () => {
    it('should parse a complete markdown file', () => {
      const markdown = `# Test Model Title

## This is a detailed description of the test model with multiple sentences. It should be parsed correctly.

## Privacy: public

## License: Creative Commons

## Categories: art, technology, toys

### Tags: aiGenerated, wip, remix

---

Additional content that should be ignored.`;

      const result = parseModelMarkdown(markdown);

      expect(result).toEqual({
        title: 'Test Model Title',
        description: 'This is a detailed description of the test model with multiple sentences. It should be parsed correctly.',
        privacy: PrivacySettings.Public,
        license: 'Creative Commons',
        categories: ['art', 'technology', 'toys'],
        aiGenerated: true,
        workInProgress: true,
        nsfw: false,
        remix: true
      });
    });

    it('should handle missing fields gracefully', () => {
      const markdown = `# Just a Title

## Privacy: private`;

      const result = parseModelMarkdown(markdown);

      expect(result).toEqual({
        title: 'Just a Title',
        privacy: PrivacySettings.Private
      });
    });

    it('should handle different privacy settings', () => {
      expect(parseModelMarkdown('## Privacy: public').privacy).toBe(PrivacySettings.Public);
      expect(parseModelMarkdown('## Privacy: private').privacy).toBe(PrivacySettings.Private);
      expect(parseModelMarkdown('## Privacy: unlisted').privacy).toBe(PrivacySettings.Unlisted);
      expect(parseModelMarkdown('## Privacy: invalid')).toEqual({});
    });

    it('should handle various tag formats', () => {
      const markdown1 = '### Tags: aiGenerated, wip, nsfw, remix';
      const result1 = parseModelMarkdown(markdown1);
      expect(result1.aiGenerated).toBe(true);
      expect(result1.workInProgress).toBe(true);
      expect(result1.nsfw).toBe(true);
      expect(result1.remix).toBe(true);

      const markdown2 = '### Tags: ai-generated, work-in-progress, adult, remixed';
      const result2 = parseModelMarkdown(markdown2);
      expect(result2.aiGenerated).toBe(true);
      expect(result2.workInProgress).toBe(true);
      expect(result2.nsfw).toBe(true);
      expect(result2.remix).toBe(true);
    });

    it('should handle categories with various spacing', () => {
      const markdown = '## Categories: art,technology , toys  , tools';
      const result = parseModelMarkdown(markdown);
      expect(result.categories).toEqual(['art', 'technology', 'toys', 'tools']);
    });

    it('should handle case insensitive field names', () => {
      const markdown = `## PRIVACY: PUBLIC
## LICENSE: MIT
## CATEGORIES: Art, Technology
### TAGS: AIGENERATED`;

      const result = parseModelMarkdown(markdown);
      expect(result.privacy).toBe(PrivacySettings.Public);
      expect(result.license).toBe('MIT');
      expect(result.categories).toEqual(['Art', 'Technology']);
      expect(result.aiGenerated).toBe(true);
    });

    it('should ignore malformed sections', () => {
      const markdown = `# Valid Title
## Privacy invalid format
## License: Valid License
## Categories invalid format
### Tags invalid format`;

      const result = parseModelMarkdown(markdown);
      expect(result).toEqual({
        title: 'Valid Title',
        license: 'Valid License'
      });
    });
  });

  describe('generateMarkdownTemplate', () => {
    it('should generate a valid template', () => {
      const template = generateMarkdownTemplate();
      expect(template).toContain('# Your Model Title Here');
      expect(template).toContain('## Privacy: public');
      expect(template).toContain('## License: Creative Commons');
      expect(template).toContain('## Categories: art, technology, toys');
      expect(template).toContain('### Tags: aiGenerated, remix');
    });
  });
});

