import type { Components } from "react-markdown";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import rehypeSanitize from "rehype-sanitize";
import { safeExternalHref } from "@/lib/security/safeUrl";

const allowHttpImages = process.env.NODE_ENV !== "production";

function safeMarkdownUrl(url: string): string {
  const external = safeExternalHref(url);
  if (external) return external;
  return "";
}

const markdownComponents: Components = {
  a: ({ href, children, ...props }) => {
    const safe = href ? safeExternalHref(href) : null;
    if (!safe) {
      return <span {...props}>{children}</span>;
    }
    return (
      <a href={safe} rel="noopener noreferrer" target="_blank" {...props}>
        {children}
      </a>
    );
  },
  img: ({ src, alt, ...props }) => {
    if (!src) return null;
    const external = safeExternalHref(src);
    if (external && (external.startsWith("https:") || (allowHttpImages && external.startsWith("http:")))) {
      return <img src={external} alt={alt ?? ""} {...props} />;
    }
    return null;
  },
};

export function MarkdownContent({ content }: { content: string }) {
  return (
    <ReactMarkdown
      remarkPlugins={[remarkGfm]}
      rehypePlugins={[rehypeSanitize]}
      urlTransform={safeMarkdownUrl}
      components={markdownComponents}
    >
      {content}
    </ReactMarkdown>
  );
}
