"use client"

import { Bell, Search } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import Link from "next/link"

export function MobileHeader() {
  return (
    <header className="sticky top-0 z-40 bg-card border-b border-border">
      <div className="flex items-center justify-between px-4 py-3">
        <Link href="/" className="flex items-center gap-2">
          <div className="flex items-center justify-center w-9 h-9 rounded-xl bg-gradient-to-br from-primary to-secondary">
            <span className="text-lg font-bold text-primary-foreground">M</span>
          </div>
          <span className="text-lg font-bold bg-gradient-to-r from-primary to-secondary bg-clip-text text-transparent">
            MyWorkplace
          </span>
        </Link>

        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon" className="h-9 w-9 rounded-full">
            <Search className="h-5 w-5" />
          </Button>
          <Button variant="ghost" size="icon" className="h-9 w-9 rounded-full relative">
            <Bell className="h-5 w-5" />
            <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-destructive rounded-full" />
          </Button>
          <Link href="/employees/me">
            <Avatar className="h-9 w-9 cursor-pointer ring-2 ring-primary/20">
              <AvatarImage src="/professional-avatar.png" />
              <AvatarFallback className="bg-primary text-primary-foreground text-sm">JD</AvatarFallback>
            </Avatar>
          </Link>
        </div>
      </div>
    </header>
  )
}
