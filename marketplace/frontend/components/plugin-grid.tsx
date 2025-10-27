import { PluginCard } from "@/components/plugin-card"

const MOCK_PLUGINS = [
  {
    id: "1",
    name: "Advanced Data Tables",
    description: "Powerful data table component with sorting, filtering, and pagination built-in",
    author: "Sarah Chen",
    authorAvatar: "/developer-avatar.png",
    downloads: 12453,
    rating: 4.8,
    version: "2.3.1",
    tags: ["react", "tables", "ui-components"],
    category: "UI Components",
  },
  {
    id: "2",
    name: "Auth Flow Manager",
    description: "Complete authentication solution with OAuth, 2FA, and session management",
    author: "Michael Rodriguez",
    authorAvatar: "/developer-avatar-2.png",
    downloads: 8921,
    rating: 4.9,
    version: "1.5.0",
    tags: ["authentication", "security", "oauth"],
    category: "Authentication",
  },
  {
    id: "3",
    name: "Chart Builder Pro",
    description: "Beautiful, responsive charts and graphs with real-time data support",
    author: "Emily Watson",
    authorAvatar: "/developer-avatar-3.png",
    downloads: 15678,
    rating: 4.7,
    version: "3.0.2",
    tags: ["charts", "data-viz", "analytics"],
    category: "Data Visualization",
  },
  {
    id: "4",
    name: "Notification Center",
    description: "Elegant notification system with toast messages, alerts, and in-app notifications",
    author: "David Kim",
    authorAvatar: "/developer-avatar-4.jpg",
    downloads: 9234,
    rating: 4.6,
    version: "1.8.3",
    tags: ["notifications", "ui-components", "alerts"],
    category: "UI Components",
  },
  {
    id: "5",
    name: "API Integration Hub",
    description: "Simplify third-party API integrations with pre-built connectors and middleware",
    author: "Lisa Anderson",
    authorAvatar: "/developer-avatar-5.jpg",
    downloads: 6789,
    rating: 4.5,
    version: "2.1.0",
    tags: ["api", "integrations", "middleware"],
    category: "Integrations",
  },
  {
    id: "6",
    name: "Form Validator Plus",
    description: "Advanced form validation with custom rules, async validation, and error handling",
    author: "James Wilson",
    authorAvatar: "/developer-avatar-6.jpg",
    downloads: 11234,
    rating: 4.8,
    version: "1.9.5",
    tags: ["forms", "validation", "typescript"],
    category: "Productivity",
  },
]

export function PluginGrid() {
  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <p className="text-sm text-muted-foreground">Showing {MOCK_PLUGINS.length} components</p>
        <select className="text-sm border rounded-md px-3 py-1.5 bg-background">
          <option>Most Downloads</option>
          <option>Highest Rated</option>
          <option>Recently Updated</option>
          <option>Newest</option>
        </select>
      </div>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {MOCK_PLUGINS.map((plugin) => (
          <PluginCard key={plugin.id} plugin={plugin} />
        ))}
      </div>
    </div>
  )
}
