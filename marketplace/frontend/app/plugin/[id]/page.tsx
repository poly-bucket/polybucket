"use client"

import { useParams } from "next/navigation"
import { PluginDetailsPage } from "@/components/plugin-details-page"

export default function PluginRoutePage() {
  const params = useParams()
  const id = typeof params.id === "string" ? params.id : Array.isArray(params.id) ? params.id[0] : ""

  if (!id) {
    return null
  }

  return <PluginDetailsPage pluginId={id} />
}
