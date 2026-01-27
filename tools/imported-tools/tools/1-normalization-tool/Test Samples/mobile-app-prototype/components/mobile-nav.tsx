"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { Home, Users, BookOpen, Award } from "lucide-react"
import { cn } from "@/lib/utils"

const navItems = [
  { href: "/", label: "Feed", icon: Home },
  { href: "/employees", label: "People", icon: Users },
  { href: "/policies", label: "Handbook", icon: BookOpen },
  { href: "/accreditations", label: "Credentials", icon: Award },
]

export function MobileNav() {
  const pathname = usePathname()

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 bg-card border-t border-border">
      <div className="flex items-center justify-around px-2 py-2 safe-bottom">
        {navItems.map((item) => {
          const isActive = pathname === item.href || (item.href !== "/" && pathname.startsWith(item.href))
          const Icon = item.icon

          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex flex-col items-center justify-center gap-1 px-4 py-2 rounded-xl transition-colors min-w-[70px]",
                isActive
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:text-foreground hover:bg-muted",
              )}
            >
              <Icon className="h-5 w-5" />
              <span className="text-xs font-medium">{item.label}</span>
            </Link>
          )
        })}
      </div>
    </nav>
  )
}
