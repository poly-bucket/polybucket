"use client"

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { SORT_OPTIONS } from "@/types/plugin"

interface SortSelectProps {
  value: string;
  onValueChange: (value: string) => void;
  "data-testid"?: string;
}

const SORT_LABELS = {
  [SORT_OPTIONS.DOWNLOADS]: "Most Downloads",
  [SORT_OPTIONS.RATING]: "Highest Rated", 
  [SORT_OPTIONS.CREATED]: "Newest",
  [SORT_OPTIONS.UPDATED]: "Recently Updated",
  [SORT_OPTIONS.NAME]: "Name A-Z"
}

export function SortSelect({ value, onValueChange, "data-testid": dataTestId }: SortSelectProps) {
  return (
    <Select value={value} onValueChange={onValueChange}>
      <SelectTrigger className="w-[180px]" data-testid={dataTestId}>
        <SelectValue placeholder="Sort by" />
      </SelectTrigger>
      <SelectContent>
        {Object.entries(SORT_LABELS).map(([key, label]) => (
          <SelectItem key={key} value={key}>
            {label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
