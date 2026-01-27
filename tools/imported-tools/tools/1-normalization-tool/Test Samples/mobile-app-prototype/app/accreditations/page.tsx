import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Award, Plus, AlertCircle, CheckCircle, Clock, Search, Filter } from "lucide-react"
import { Input } from "@/components/ui/input"
import Link from "next/link"
import { accreditations } from "@/lib/mock-data"
import { cn } from "@/lib/utils"

export const dynamic = "force-static"

const statusConfig = {
  Valid: {
    icon: CheckCircle,
    variant: "default" as const,
    bgColor: "bg-primary/10",
    iconColor: "text-primary",
  },
  Expired: {
    icon: AlertCircle,
    variant: "destructive" as const,
    bgColor: "bg-destructive/10",
    iconColor: "text-destructive",
  },
  Pending: {
    icon: Clock,
    variant: "secondary" as const,
    bgColor: "bg-secondary/20",
    iconColor: "text-secondary",
  },
}

export default function AccreditationsPage() {
  const validCount = accreditations.filter((a) => a.status === "Valid").length
  const expiringCount = accreditations.filter((a) => {
    if (a.status !== "Valid" || !a.expiryDate) return false
    const daysUntilExpiry = Math.floor(
      (new Date(a.expiryDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24),
    )
    return daysUntilExpiry <= 60
  }).length
  const pendingCount = accreditations.filter((a) => a.status === "Pending").length

  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-balance mb-2">My Accreditations</h1>
          <p className="text-muted-foreground text-pretty">Manage your professional credentials and certifications</p>
        </div>

        {/* Status Summary */}
        <div className="grid grid-cols-3 gap-3 mb-6">
          <Card>
            <CardContent className="p-4 text-center">
              <div className="text-2xl font-bold text-primary mb-1">{validCount}</div>
              <div className="text-xs text-muted-foreground">Active</div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <div className="text-2xl font-bold text-secondary mb-1">{expiringCount}</div>
              <div className="text-xs text-muted-foreground">Expiring Soon</div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 text-center">
              <div className="text-2xl font-bold text-muted-foreground mb-1">{pendingCount}</div>
              <div className="text-xs text-muted-foreground">Pending</div>
            </CardContent>
          </Card>
        </div>

        {/* Search and Add */}
        <div className="flex gap-2 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input placeholder="Search accreditations..." className="pl-9" />
          </div>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
          <Link href="/accreditations/add">
            <Button size="icon" className="gap-2">
              <Plus className="h-4 w-4" />
            </Button>
          </Link>
        </div>

        {/* Accreditations List */}
        <div className="space-y-3">
          {accreditations.map((accreditation) => {
            const config = statusConfig[accreditation.status]
            const StatusIcon = config.icon
            const daysUntilExpiry = accreditation.expiryDate
              ? Math.floor(
                  (new Date(accreditation.expiryDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24),
                )
              : null

            const isExpiringSoon = accreditation.status === "Valid" && daysUntilExpiry !== null && daysUntilExpiry <= 60

            return (
              <Link key={accreditation.id} href={`/accreditations/${accreditation.id}`}>
                <Card className="hover:shadow-md transition-shadow">
                  <CardContent className="p-4">
                    <div className="flex items-start gap-3">
                      <div
                        className={cn("flex items-center justify-center h-10 w-10 rounded-lg shrink-0", config.bgColor)}
                      >
                        <StatusIcon className={cn("h-5 w-5", config.iconColor)} />
                      </div>

                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between gap-2 mb-1">
                          <h3 className="font-semibold leading-tight">{accreditation.name}</h3>
                          <Badge variant={config.variant} className="shrink-0 text-xs">
                            {accreditation.status}
                          </Badge>
                        </div>

                        <p className="text-sm text-muted-foreground leading-relaxed mb-2">
                          {accreditation.description}
                        </p>

                        <div className="flex flex-wrap items-center gap-3 text-xs">
                          <Badge variant="outline" className="text-xs">
                            {accreditation.category}
                          </Badge>

                          {accreditation.expiryDate && (
                            <span
                              className={cn("text-muted-foreground", isExpiringSoon && "text-secondary font-medium")}
                            >
                              {accreditation.status === "Expired"
                                ? `Expired ${new Date(accreditation.expiryDate).toLocaleDateString("en-US", {
                                    month: "short",
                                    year: "numeric",
                                  })}`
                                : `Expires ${new Date(accreditation.expiryDate).toLocaleDateString("en-US", {
                                    month: "short",
                                    year: "numeric",
                                  })}`}
                              {isExpiringSoon && ` (${daysUntilExpiry} days)`}
                            </span>
                          )}
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </Link>
            )
          })}
        </div>

        {/* Info Card */}
        <Card className="mt-6 border-primary/20 bg-primary/5">
          <CardContent className="p-4">
            <div className="flex gap-3">
              <Award className="h-5 w-5 text-primary shrink-0 mt-0.5" />
              <div>
                <h3 className="font-semibold mb-1">Claim Internal Accreditations</h3>
                <p className="text-sm text-muted-foreground leading-relaxed">
                  You can earn internal accreditations by reading relevant policies in the Handbook section. External
                  accreditations require HR verification.
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </main>

      <MobileNav />
    </div>
  )
}
