import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { Button } from "@/components/ui/button"
import { Heart, MessageCircle, Share2 } from "lucide-react"

// Mock data for company updates
const updates = [
  {
    id: 1,
    title: "Q1 2024 All-Hands Meeting - Register Now",
    body: "Join us for our quarterly all-hands meeting where we'll share company updates, celebrate wins, and outline our roadmap for the coming quarter. Don't miss this opportunity to connect with leadership and ask questions!",
    author: {
      name: "Sarah Johnson",
      role: "Chief Executive Officer",
      avatar: "/female-executive.png",
    },
    publishedDate: "2 hours ago",
    likes: 47,
    comments: 12,
    isLiked: false,
  },
  {
    id: 2,
    title: "New Wellness Program Launching Next Week",
    body: "We're excited to announce our enhanced wellness program! Starting next week, all employees will have access to virtual fitness classes, mental health resources, and wellness coaching. Check the Handbook for full details.",
    author: {
      name: "Michael Chen",
      role: "Head of People & Culture",
      avatar: "/asian-professional.jpg",
    },
    publishedDate: "5 hours ago",
    likes: 89,
    comments: 24,
    isLiked: true,
  },
  {
    id: 3,
    title: "Q: Best practices for remote client meetings?",
    body: "I've been doing more virtual meetings with clients lately. What are your go-to tips for keeping them engaged and productive? Any tools or techniques you swear by?",
    author: {
      name: "Alex Rivera",
      role: "Senior Account Manager",
      avatar: "/hispanic-professional.jpg",
    },
    publishedDate: "1 day ago",
    likes: 34,
    comments: 18,
    isLiked: false,
  },
]

export default function FeedPage() {
  return (
    <div className="min-h-screen pb-20">
      <MobileHeader />

      <main className="px-4 py-6 max-w-2xl mx-auto">
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-balance mb-2">Company Feed</h1>
          <p className="text-muted-foreground text-pretty">Stay connected with the latest updates and conversations</p>
        </div>

        <div className="space-y-4">
          {updates.map((update) => (
            <Card key={update.id} className="overflow-hidden">
              <CardHeader className="space-y-3 pb-3">
                <div className="flex items-start gap-3">
                  <Avatar className="h-10 w-10">
                    <AvatarImage src={update.author.avatar || "/placeholder.svg"} />
                    <AvatarFallback className="bg-primary text-primary-foreground">
                      {update.author.name
                        .split(" ")
                        .map((n) => n[0])
                        .join("")}
                    </AvatarFallback>
                  </Avatar>
                  <div className="flex-1 min-w-0">
                    <p className="font-semibold leading-tight">{update.author.name}</p>
                    <p className="text-sm text-muted-foreground leading-tight">{update.author.role}</p>
                    <p className="text-xs text-muted-foreground mt-0.5">{update.publishedDate}</p>
                  </div>
                </div>
              </CardHeader>

              <CardContent className="space-y-4">
                <div>
                  <h3 className="font-semibold text-lg mb-2 text-balance leading-tight">{update.title}</h3>
                  <p className="text-muted-foreground leading-relaxed text-pretty">{update.body}</p>
                </div>

                <div className="flex items-center gap-1 pt-2 border-t border-border">
                  <Button variant={update.isLiked ? "default" : "ghost"} size="sm" className="flex-1 gap-2">
                    <Heart className={cn("h-4 w-4", update.isLiked && "fill-current")} />
                    <span>{update.likes}</span>
                  </Button>
                  <Button variant="ghost" size="sm" className="flex-1 gap-2">
                    <MessageCircle className="h-4 w-4" />
                    <span>{update.comments}</span>
                  </Button>
                  <Button variant="ghost" size="sm" className="flex-1 gap-2">
                    <Share2 className="h-4 w-4" />
                    <span>Share</span>
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </main>

      <MobileNav />
    </div>
  )
}

function cn(...classes: (string | boolean | undefined)[]) {
  return classes.filter(Boolean).join(" ")
}
