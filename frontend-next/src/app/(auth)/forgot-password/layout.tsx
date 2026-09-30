import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Forgot password | Polybucket",
};

export default function ForgotPasswordLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return children;
}
