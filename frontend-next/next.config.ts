import type { NextConfig } from "next";
import { securityResponseHeaders } from "./src/lib/security/securityHeaders";

const nextConfig: NextConfig = {
  output: "standalone",
  async headers() {
    return [
      {
        source: "/:path*",
        headers: securityResponseHeaders(),
      },
    ];
  },
  async redirects() {
    return [
      { source: "/upload-model", destination: "/models/upload", permanent: true },
      { source: "/my-collections", destination: "/collections/mine", permanent: true },
      { source: "/my-collections/create", destination: "/collections/create", permanent: true },
      { source: "/my-collections/:id", destination: "/collections/:id", permanent: true },
      { source: "/my-collections/:id/edit", destination: "/collections/:id/edit", permanent: true },
      { source: "/moderation-dashboard", destination: "/moderation/reports", permanent: true },
    ];
  },
};

export default nextConfig;
