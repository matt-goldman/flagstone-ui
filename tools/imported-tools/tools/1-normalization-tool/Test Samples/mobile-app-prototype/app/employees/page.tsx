import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Search, Filter, Mail, Phone } from "lucide-react"
import { Badge } from "@/components/ui/badge"
import Link from "next/link"
import { employees } from "@/lib/mock-data"

export const dynamic = "force-static"

export default function EmployeesPage() {
  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-balance mb-2">Employee Directory</h1>
          <p className="text-muted-foreground text-pretty">Connect with colleagues across the company</p>
        </div>

        {/* Search and Filter */}
        <div className="flex gap-2 mb-6">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
            <Input placeholder="Search by name, role, or department..." className="pl-9" />
          </div>
          <Button variant="outline" size="icon">
            <Filter className="h-4 w-4" />
          </Button>
        </div>

        {/* Employee List */}
        <div className="space-y-3">
          {employees.map((employee) => (
            <Link key={employee.id} href={`/employees/${employee.id}`}>
              <Card className="hover:shadow-md transition-shadow">
                <CardContent className="p-4">
                  <div className="flex items-start gap-3">
                    <Avatar className="h-12 w-12 ring-2 ring-primary/10">
                      <AvatarImage src={employee.avatar || "/placeholder.svg"} />
                      <AvatarFallback className="bg-primary text-primary-foreground">
                        {employee.name
                          .split(" ")
                          .map((n) => n[0])
                          .join("")}
                      </AvatarFallback>
                    </Avatar>

                    <div className="flex-1 min-w-0">
                      <div className="flex items-start justify-between gap-2 mb-1">
                        <div className="min-w-0">
                          <h3 className="font-semibold leading-tight truncate">{employee.name}</h3>
                          <p className="text-sm text-muted-foreground leading-tight">{employee.role}</p>
                        </div>
                        <Badge variant="secondary" className="shrink-0 text-xs">
                          {employee.department}
                        </Badge>
                      </div>

                      <div className="flex items-center gap-3 mt-2 text-xs text-muted-foreground">
                        <span className="flex items-center gap-1">
                          <Mail className="h-3 w-3" />
                          <span className="truncate max-w-[150px]">{employee.email.split("@")[0]}</span>
                        </span>
                        {employee.showPhone && employee.phone && (
                          <span className="flex items-center gap-1">
                            <Phone className="h-3 w-3" />
                            Available
                          </span>
                        )}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      </main>

      <MobileNav />
    </div>
  )
}
