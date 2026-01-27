import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Mail, Phone, MapPin, Calendar, Award, Edit, MessageCircle } from "lucide-react"
import Link from "next/link"
import { getEmployeeById, currentUserId } from "@/lib/mock-data"
import { notFound } from "next/navigation"

export default async function EmployeeProfilePage({
  params,
}: {
  params: Promise<{ id: string }>
}) {
  const { id } = await params
  const employee = getEmployeeById(id)
  const isOwnProfile = id === currentUserId

  if (!employee) {
    notFound()
  }

  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        {/* Profile Header */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center text-center mb-6">
              <Avatar className="h-24 w-24 mb-4 ring-4 ring-primary/20">
                <AvatarImage src={employee.avatar || "/placeholder.svg"} />
                <AvatarFallback className="bg-primary text-primary-foreground text-2xl">
                  {employee.name
                    .split(" ")
                    .map((n) => n[0])
                    .join("")}
                </AvatarFallback>
              </Avatar>
              <h1 className="text-2xl font-bold mb-1">{employee.name}</h1>
              <p className="text-muted-foreground mb-2">{employee.role}</p>
              <Badge variant="secondary">{employee.department}</Badge>
            </div>

            {isOwnProfile && (
              <Button className="w-full mb-4 gap-2">
                <Edit className="h-4 w-4" />
                Edit Profile
              </Button>
            )}

            {!isOwnProfile && (
              <div className="flex gap-2 mb-4">
                <Button className="flex-1 gap-2">
                  <MessageCircle className="h-4 w-4" />
                  Message
                </Button>
                <Button variant="outline" className="flex-1 gap-2 bg-transparent">
                  <Mail className="h-4 w-4" />
                  Email
                </Button>
              </div>
            )}

            <div className="space-y-3 pt-4 border-t border-border">
              <p className="text-sm leading-relaxed text-muted-foreground">{employee.profileSummary}</p>
            </div>
          </CardContent>
        </Card>

        {/* Contact Information */}
        <Card className="mb-6">
          <CardContent className="pt-6 space-y-4">
            <h2 className="font-semibold text-lg mb-4">Contact Information</h2>

            <div className="flex items-center gap-3">
              <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-primary/10">
                <Mail className="h-5 w-5 text-primary" />
              </div>
              <div className="flex-1">
                <p className="text-xs text-muted-foreground">Email</p>
                <p className="text-sm font-medium">{employee.email}</p>
              </div>
            </div>

            {(isOwnProfile || employee.showPhone) && employee.phone && (
              <div className="flex items-center gap-3">
                <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-secondary/10">
                  <Phone className="h-5 w-5 text-secondary" />
                </div>
                <div className="flex-1">
                  <p className="text-xs text-muted-foreground">Phone</p>
                  <p className="text-sm font-medium">{employee.phone}</p>
                </div>
              </div>
            )}

            {isOwnProfile && employee.homeAddress && (
              <div className="flex items-center gap-3">
                <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-accent/10">
                  <MapPin className="h-5 w-5 text-accent" />
                </div>
                <div className="flex-1">
                  <p className="text-xs text-muted-foreground">Address</p>
                  <p className="text-sm font-medium">{employee.homeAddress}</p>
                </div>
              </div>
            )}

            {isOwnProfile && employee.dateOfBirth && (
              <div className="flex items-center gap-3">
                <div className="flex items-center justify-center h-10 w-10 rounded-lg bg-primary/10">
                  <Calendar className="h-5 w-5 text-primary" />
                </div>
                <div className="flex-1">
                  <p className="text-xs text-muted-foreground">Date of Birth</p>
                  <p className="text-sm font-medium">
                    {new Date(employee.dateOfBirth).toLocaleDateString("en-US", {
                      year: "numeric",
                      month: "long",
                      day: "numeric",
                    })}
                  </p>
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Accreditations Summary */}
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center gap-2">
                <Award className="h-5 w-5 text-primary" />
                <h2 className="font-semibold text-lg">Accreditations</h2>
              </div>
              <Link href="/accreditations">
                <Button variant="ghost" size="sm">
                  View All
                </Button>
              </Link>
            </div>
            <p className="text-sm text-muted-foreground">{employee.accreditationStatus}</p>
          </CardContent>
        </Card>
      </main>

      <MobileNav />
    </div>
  )
}
