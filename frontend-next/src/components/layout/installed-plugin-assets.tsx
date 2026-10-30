"use client";

import { useEffect, useState } from "react";
import { getInstalledPlugins } from "@/lib/services/pluginsAdminService";

export function InstalledPluginAssets() {
  const [stylesheetHrefs, setStylesheetHrefs] = useState<string[]>([]);

  useEffect(() => {
    let cancelled = false;

    getInstalledPlugins()
      .then((plugins) => {
        if (cancelled) return;
        const hrefs = plugins.flatMap((plugin) =>
          (plugin.cssAssets ?? []).map(
            (assetPath) =>
              `/api/plugins/installed/${encodeURIComponent(plugin.id)}/assets/${assetPath.replace(/^\/+/, "")}`
          )
        );
        setStylesheetHrefs(hrefs);
      })
      .catch(() => {
        if (!cancelled) setStylesheetHrefs([]);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <>
      {stylesheetHrefs.map((href) => (
        <link key={href} rel="stylesheet" href={href} />
      ))}
    </>
  );
}
