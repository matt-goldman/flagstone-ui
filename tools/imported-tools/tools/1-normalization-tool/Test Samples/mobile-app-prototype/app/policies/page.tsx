import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Search, Filter, BookOpen, Clock } from "lucide-react"
import Link from "next/link"
import { policies } from "@/lib/mock-data"

export const dynamic = "force-static"

// Group policies by category
const groupedPolicies = policies.reduce(
  (acc, policy) => {
    if (!acc[policy.category]) {
      acc[policy.category] = []
    }
    acc[policy.category].push(policy)
    return acc
  },
  {} as Record<string, typeof policies>,
)

export default function PoliciesPage() {
  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-balance mb-2">Policies & Handbook</h1>
          <p className="text-muted-foreground text-pretty">
            Company policies, procedures, and guidelines for employees
          </p>
        </div>

        {/* Search and Filter */}
        <div className="flex gap-2 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input placeholder="Search policies..." className="pl-9" />
          </div>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
        </div>

        {/* Policies by Category */}
        <div className="space-y-6">
          {Object.entries(groupedPolicies).map(([category, categoryPolicies]) => (
            <div key={category}>
              <h2 className="text-lg font-semibold mb-3 flex items-center gap-2">
                <BookOpen className="h-5 w-5 text-primary" />
                {category}
              </h2>

              <div className="space-y-3">
                {categoryPolicies.map((policy) => (
                  <Link key={policy.id} href={`/policies/${policy.id}`}>
                    <Card className="hover:shadow-md transition-shadow">
                      <CardContent className="p-4">
                        <div className="flex items-start justify-between gap-3 mb-2">
                          <h3 className="font-semibold leading-tight flex-1">{policy.title}</h3>
                          <Badge variant="outline" className="shrink-0 text-xs">
                            {category}
                          </Badge>
                        </div>

                        <p className="text-sm text-muted-foreground leading-relaxed mb-3">{policy.excerpt}</p>

                        <div className="flex items-center gap-4 text-xs text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            Updated{" "}
                            {new Date(policy.lastUpdated).toLocaleDateString("en-US", {
                              month: "short",
                              day: "numeric",
                              year: "numeric",
                            })}
                          </span>
                          <span>By {policy.lastUpdatedBy}</span>
                        </div>
                      </CardContent>
                    </Card>
                  </Link>
                ))}
              </div>
            </div>
          ))}
        </div>
      </main>

      <MobileNav />
    </div>
  )
}
