import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { ArrowLeft, CheckCircle, AlertCircle, Clock, Calendar, FileText, RefreshCw, Share2 } from "lucide-react"
import Link from "next/link"
import { getAccreditationById } from "@/lib/mock-data"
import { notFound } from "next/navigation"
import { cn } from "@/lib/utils"

const statusConfig = {
  Valid: {
    icon: CheckCircle,
    variant: "default" as const,
    bgColor: "bg-primary",
    textColor: "text-primary-foreground",
    label: "Active & Valid",
  },
  Expired: {
    icon: AlertCircle,
    variant: "destructive" as const,
    bgColor: "bg-destructive",
    textColor: "text-destructive-foreground",
    label: "Expired",
  },
  Pending: {
    icon: Clock,
    variant: "secondary" as const,
    bgColor: "bg-secondary",
    textColor: "text-secondary-foreground",
    label: "Pending Verification",
  },
}

export default async function AccreditationDetailPage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = await params
  const accreditation = getAccreditationById(id)

  if (!accreditation) {
    notFound()
  }

  const config = statusConfig[accreditation.status]
  const StatusIcon = config.icon

  const daysUntilExpiry = accreditation.expiryDate
    ? Math.floor((new Date(accreditation.expiryDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24))
    : null

  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        {/* Back Button */}
        <Link href="/accreditations">
          <Button variant="ghost" size="sm" className="mb-4 -ml-2 gap-1">
            <ArrowLeft className="h-4 w-4" />
            Back to Accreditations
          </Button>
        </Link>

        {/* Status Card */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className={cn("flex items-center justify-center h-16 w-16 rounded-2xl mb-4", config.bgColor)}>
              <StatusIcon className={cn("h-8 w-8", config.textColor)} />
            </div>

            <Badge variant={config.variant} className="mb-3">
              {config.label}
            </Badge>

            <h1 className="text-2xl font-bold mb-3 text-balance leading-tight">{accreditation.name}</h1>
            <p className="text-muted-foreground leading-relaxed mb-4">{accreditation.description}</p>

            <div className="flex items-center gap-2 pt-4 border-t border-border">
              <Badge variant="outline">{accreditation.category} Accreditation</Badge>
            </div>
          </CardContent>
        </Card>

        {/* Timeline Card */}
        <Card className="mb-6">
          <CardContent className="pt-6 space-y-4">
            <h2 className="font-semibold text-lg mb-4">Timeline</h2>

            {accreditation.issuedDate && (
              <div className="flex items-start gap-3">
                <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-muted">
                  <Calendar className="h-5 w-5 text-muted-foreground" />
                </div>
                <div>
                  <p className="text-sm font-medium">Issued Date</p>
                  <p className="text-sm text-muted-foreground">
                    {new Date(accreditation.issuedDate).toLocaleDateString("en-US", {
                      month: "long",
                      day: "numeric",
                      year: "numeric",
                    })}
                  </p>
                </div>
              </div>
            )}

            {accreditation.expiryDate && (
              <div className="flex items-start gap-3">
                <div
                  className={cn(
                    "flex items-center justify-center h-10 w-10 rounded-lg",
                    accreditation.status === "Expired"
                      ? "bg-destructive/10"
                      : daysUntilExpiry && daysUntilExpiry <= 60
                        ? "bg-secondary/20"
                        : "bg-muted",
                  )}
                >
                  <Clock
                    className={cn(
                      "h-5 w-5",
                      accreditation.status === "Expired"
                        ? "text-destructive"
                        : daysUntilExpiry && daysUntilExpiry <= 60
                          ? "text-secondary"
                          : "text-muted-foreground",
                    )}
                  />
                </div>
                <div>
                  <p className="text-sm font-medium">
                    {accreditation.status === "Expired" ? "Expired On" : "Expiry Date"}
                  </p>
                  <p
                    className={cn(
                      "text-sm",
                      accreditation.status === "Expired"
                        ? "text-destructive"
                        : daysUntilExpiry && daysUntilExpiry <= 60
                          ? "text-secondary font-medium"
                          : "text-muted-foreground",
                    )}
                  >
                    {new Date(accreditation.expiryDate).toLocaleDateString("en-US", {
                      month: "long",
                      day: "numeric",
                      year: "numeric",
                    })}
                    {daysUntilExpiry !== null && accreditation.status === "Valid" && daysUntilExpiry <= 60 && (
                      <span className="ml-1">({daysUntilExpiry} days remaining)</span>
                    )}
                  </p>
                </div>
              </div>
            )}

            {accreditation.status === "Pending" && (
              <div className="flex items-start gap-3 pt-4 border-t border-border">
                <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-secondary/20">
                  <FileText className="h-5 w-5 text-secondary" />
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium mb-1">Verification Status</p>
                  <p className="text-sm text-muted-foreground leading-relaxed">
                    Your accreditation is pending verification by HR. You'll be notified once it's been reviewed.
                  </p>
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Actions */}
        <div className="space-y-3">
          {accreditation.status === "Valid" && (
            <Button className="w-full gap-2">
              <RefreshCw className="h-4 w-4" />
              Renew Accreditation
            </Button>
          )}

          {accreditation.status === "Expired" && (
            <Button className="w-full gap-2">
              <RefreshCw className="h-4 w-4" />
              Recertify Now
            </Button>
          )}

          <Button variant="outline" className="w-full gap-2 bg-transparent">
            <Share2 className="h-4 w-4" />
            Share Credential
          </Button>
        </div>

        {/* Info Card */}
        {accreditation.category === "Internal" && accreditation.status !== "Valid" && (
          <Card className="mt-6 border-primary/20 bg-primary/5">
            <CardContent className="p-4">
              <h3 className="font-semibold mb-2">How to Earn This Accreditation</h3>
              <p className="text-sm text-muted-foreground leading-relaxed mb-3">
                Read the related policies in the Handbook section and complete the acknowledgment to claim this internal
                accreditation.
              </p>
              <Link href="/policies">
                <Button size="sm" variant="outline" className="bg-background">
                  View Related Policies
                </Button>
              </Link>
            </CardContent>
          </Card>
        )}
      </main>

      <MobileNav />
    </div>
  )
}
