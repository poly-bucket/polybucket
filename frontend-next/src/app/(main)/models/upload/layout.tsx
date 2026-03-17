import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Upload Model | Polybucket",
};

export default function ModelUploadLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}
