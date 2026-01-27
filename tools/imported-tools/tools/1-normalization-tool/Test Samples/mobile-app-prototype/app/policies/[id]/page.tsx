import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { ArrowLeft, Clock, User, Share2, Bookmark } from "lucide-react"
import Link from "next/link"
import { getPolicyById } from "@/lib/mock-data"
import { notFound } from "next/navigation"
import ReactMarkdown from "react-markdown"

export default async function PolicyDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = await params
  const policy = getPolicyById(id)

  if (!policy) {
    notFound()
  }

  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        {/* Back Button */}
        <Link href="/policies">
          <Button variant="ghost" size="sm" className="mb-4 -ml-2 gap-1">
            <ArrowLeft className="h-4 w-4" />
            Back to Policies
          </Button>
        </Link>

        {/* Policy Header */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <Badge variant="secondary" className="mb-3">
              {policy.category}
            </Badge>
            <h1 className="text-2xl font-bold mb-4 text-balance leading-tight">{policy.title}</h1>

            <div className="flex flex-wrap gap-4 text-sm text-muted-foreground mb-4">
              <span className="flex items-center gap-1.5">
                <Clock className="h-4 w-4" />
                Last updated{" "}
                {new Date(policy.lastUpdated).toLocaleDateString("en-US", {
                  month: "long",
                  day: "numeric",
                  year: "numeric",
                })}
              </span>
              <span className="flex items-center gap-1.5">
                <User className="h-4 w-4" />
                {policy.lastUpdatedBy}
              </span>
            </div>

            <div className="flex gap-2 pt-4 border-t border-border">
              <Button variant="outline" size="sm" className="flex-1 gap-2 bg-transparent">
                <Share2 className="h-4 w-4" />
                Share
              </Button>
              <Button variant="outline" size="sm" className="flex-1 gap-2 bg-transparent">
                <Bookmark className="h-4 w-4" />
                Save
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Policy Content */}
        <Card>
          <CardContent className="pt-6">
            <div className="prose prose-sm max-w-none">
              <ReactMarkdown
                components={{
                  h2: ({ ...props }) => <h2 className="text-xl font-bold mt-6 mb-3 text-foreground" {...props} />,
                  h3: ({ ...props }) => <h3 className="text-lg font-semibold mt-4 mb-2 text-foreground" {...props} />,
                  p: ({ ...props }) => <p className="text-muted-foreground leading-relaxed mb-4" {...props} />,
                  ul: ({ ...props }) => (
                    <ul className="list-disc list-inside space-y-2 mb-4 text-muted-foreground" {...props} />
                  ),
                  ol: ({ ...props }) => (
                    <ol className="list-decimal list-inside space-y-2 mb-4 text-muted-foreground" {...props} />
                  ),
                  li: ({ ...props }) => <li className="leading-relaxed" {...props} />,
                  strong: ({ ...props }) => <strong className="font-semibold text-foreground" {...props} />,
                }}
              >
                {policy.content}
              </ReactMarkdown>
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card className="mt-6">
          <CardContent className="pt-6">
            <h2 className="font-semibold mb-3">Need Help?</h2>
            <p className="text-sm text-muted-foreground mb-4">
              If you have questions about this policy, reach out to HR or your manager.
            </p>
            <Button variant="outline" className="w-full bg-transparent">
              Contact HR
            </Button>
          </CardContent>
        </Card>
      </main>

      <MobileNav />
    </div>
  )
}
