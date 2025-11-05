import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Download, Star, Shield, Sparkles } from "lucide-react"
import Link from "next/link"
import { PluginSummary } from "@/types/plugin"
import { InstallButton } from "./install-button"

interface PluginCardProps {
  plugin: PluginSummary
  "data-testid"?: string;
}

export function PluginCard({ plugin, "data-testid": dataTestId }: PluginCardProps) {
  return (
    <Link href={`/plugin/${plugin.id}`}>
      <Card className="h-full hover:shadow-md transition-shadow cursor-pointer" data-testid={dataTestId}>
        <CardHeader>
          <div className="flex items-start justify-between gap-3">
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <h3 className="font-semibold text-lg truncate">{plugin.name}</h3>
                {plugin.isVerified && (
                  <Shield className="h-4 w-4 text-blue-500" />
                )}
                {plugin.isFeatured && (
                  <Sparkles className="h-4 w-4 text-yellow-500" />
                )}
              </div>
              <p className="text-sm text-muted-foreground line-clamp-2 text-pretty">{plugin.description}</p>
            </div>
            <Badge variant="outline" className="shrink-0">
              v{plugin.version}
            </Badge>
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-1.5 mb-4">
            <Badge variant="secondary" className="text-xs">
              {plugin.category}
            </Badge>
            {plugin.tags.slice(0, 3).map((tag) => (
              <Badge key={tag} variant="secondary" className="text-xs">
                {tag}
              </Badge>
            ))}
            {plugin.tags.length > 3 && (
              <Badge variant="secondary" className="text-xs">
                +{plugin.tags.length - 3} more
              </Badge>
            )}
          </div>
          <div className="flex items-center justify-between text-sm">
            <div className="flex items-center gap-4">
              <div className="flex items-center gap-1.5 text-muted-foreground">
                <Download className="h-4 w-4" />
                <span>{plugin.downloads.toLocaleString()}</span>
              </div>
              <div className="flex items-center gap-1.5 text-muted-foreground">
                <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                <span>{plugin.averageRating.toFixed(1)}</span>
                <span className="text-xs">({plugin.reviewCount})</span>
              </div>
            </div>
          </div>
        </CardContent>
        <CardFooter className="border-t pt-4">
          <div className="flex items-center justify-between w-full">
            <div className="flex items-center gap-2">
              <Avatar className="h-6 w-6">
                <AvatarFallback>{plugin.author[0]}</AvatarFallback>
              </Avatar>
              <span className="text-sm text-muted-foreground">{plugin.author}</span>
            </div>
            <InstallButton 
              plugin={plugin} 
              variant="outline" 
              size="sm"
              showRepositoryInfo={false}
            />
          </div>
        </CardFooter>
      </Card>
    </Link>
  )
}
