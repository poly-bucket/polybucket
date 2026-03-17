"use client"

import {
  Pagination as PaginationRoot,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationPrevious,
  PaginationNext,
  PaginationEllipsis,
} from "@/components/ui/pagination"
import { cn } from "@/lib/utils"

export interface DataTablePaginationProps {
  page: number
  totalPages: number
  onPageChange?: (page: number) => void
  basePath?: string
  className?: string
}

export function DataTablePagination({
  page,
  totalPages,
  onPageChange,
  basePath,
  className,
}: DataTablePaginationProps) {
  const showPrev = page > 1
  const showNext = page < totalPages
  const prevPage = page - 1
  const nextPage = page + 1

  const pageNumbers: (number | "ellipsis")[] = []
  const showRange = 2
  for (let i = 1; i <= totalPages; i++) {
    if (
      i === 1 ||
      i === totalPages ||
      (i >= page - showRange && i <= page + showRange)
    ) {
      pageNumbers.push(i)
    } else if (
      pageNumbers[pageNumbers.length - 1] !== "ellipsis"
    ) {
      pageNumbers.push("ellipsis")
    }
  }

  const content = (
    <PaginationContent className={cn("text-white/70", className)}>
      {showPrev && (
        <PaginationItem>
          {onPageChange ? (
            <PaginationPrevious
              href="#"
              onClick={(e) => {
                e.preventDefault()
                onPageChange(prevPage)
              }}
              className="text-white/70 hover:text-white hover:bg-white/10"
            />
          ) : basePath ? (
            <PaginationPrevious
              href={prevPage === 1 ? basePath : `${basePath}?page=${prevPage}`}
              className="text-white/70 hover:text-white hover:bg-white/10"
            />
          ) : (
            <PaginationPrevious href="#" className="text-white/70 hover:text-white hover:bg-white/10" />
          )}
        </PaginationItem>
      )}
      {pageNumbers.map((p, i) =>
        p === "ellipsis" ? (
          <PaginationItem key={`ell-${i}`}>
            <PaginationEllipsis className="text-white/50" />
          </PaginationItem>
        ) : (
          <PaginationItem key={p}>
            {onPageChange ? (
              <PaginationLink
                href="#"
                isActive={p === page}
                onClick={(e) => {
                  e.preventDefault()
                  onPageChange(p)
                }}
                className="text-white/70 hover:text-white hover:bg-white/10 data-[active=true]:bg-white/20 data-[active=true]:text-white"
              >
                {p}
              </PaginationLink>
            ) : basePath ? (
              <PaginationLink
                href={p === 1 ? basePath : `${basePath}?page=${p}`}
                isActive={p === page}
                className="text-white/70 hover:text-white hover:bg-white/10 data-[active=true]:bg-white/20 data-[active=true]:text-white"
              >
                {p}
              </PaginationLink>
            ) : (
              <PaginationLink
                href="#"
                isActive={p === page}
                className="text-white/70 hover:text-white hover:bg-white/10 data-[active=true]:bg-white/20 data-[active=true]:text-white"
              >
                {p}
              </PaginationLink>
            )}
          </PaginationItem>
        )
      )}
      {showNext && (
        <PaginationItem>
          {onPageChange ? (
            <PaginationNext
              href="#"
              onClick={(e) => {
                e.preventDefault()
                onPageChange(nextPage)
              }}
              className="text-white/70 hover:text-white hover:bg-white/10"
            />
          ) : basePath ? (
            <PaginationNext
              href={`${basePath}?page=${nextPage}`}
              className="text-white/70 hover:text-white hover:bg-white/10"
            />
          ) : (
            <PaginationNext href="#" className="text-white/70 hover:text-white hover:bg-white/10" />
          )}
        </PaginationItem>
      )}
    </PaginationContent>
  )

  return (
    <PaginationRoot aria-label="Table pagination">
      {content}
    </PaginationRoot>
  )
}
