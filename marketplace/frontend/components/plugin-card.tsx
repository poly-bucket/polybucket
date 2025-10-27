import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Download, Star } from "lucide-react"
import Link from "next/link"

interface PluginCardProps {
  plugin: {
    id: string
    name: string
    description: string
    author: string
    authorAvatar: string
    downloads: number
    rating: number
    version: string
    tags: string[]
    category: string
  }
}

export function PluginCard({ plugin }: PluginCardProps) {
  return (
    <Link href={`/plugin/${plugin.id}`}>
      <Card className="h-full hover:shadow-md transition-shadow cursor-pointer">
        <CardHeader>
          <div className="flex items-start justify-between gap-3">
            <div className="flex-1 min-w-0">
              <h3 className="font-semibold text-lg mb-1 truncate">{plugin.name}</h3>
              <p className="text-sm text-muted-foreground line-clamp-2 text-pretty">{plugin.description}</p>
            </div>
            <Badge variant="outline" className="shrink-0">
              v{plugin.version}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-1.5 mb-4">
            {plugin.tags.map((tag) => (
              <Badge key={tag} variant="secondary" className="text-xs">
                {tag}
              </Badge>
            ))}
          </div>
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-1.5 text-muted-foreground">
                <Download className="h-4 w-4" />
                <span>{plugin.downloads.toLocaleString()}</span>
              </div>
              <div className="flex items-center gap-1.5 text-muted-foreground">
                <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                <span>{plugin.rating}</span>
              </div>
            </div>
          </div>
        </CardContent>
        <CardFooter className="border-t pt-4">
          <div className="flex items-center gap-2">
            <Avatar className="h-6 w-6">
              <AvatarImage src={plugin.authorAvatar || "/placeholder.svg"} />
              <AvatarFallback>{plugin.author[0]}</AvatarFallback>
            </Avatar>
            <span className="text-sm text-muted-foreground">{plugin.author}</span>
          </div>
        </CardFooter>
      </Card>
    </Link>
  )
}
