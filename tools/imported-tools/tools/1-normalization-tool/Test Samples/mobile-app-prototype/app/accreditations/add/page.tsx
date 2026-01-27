"use client"

import { MobileHeader } from "@/components/mobile-header"
import { MobileNav } from "@/components/mobile-nav"
import { Card, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { ArrowLeft, Plus, Upload, AlertCircle } from "lucide-react"
import Link from "next/link"
import { useState } from "react"

export default function AddAccreditationPage() {
  const [category, setCategory] = useState<"Internal" | "External">("External")

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

        <div className="mb-6">
          <h1 className="text-2xl font-bold text-balance mb-2">Add Accreditation</h1>
          <p className="text-muted-foreground text-pretty">Submit a new professional credential for verification</p>
        </div>

        <Card className="mb-6">
          <CardContent className="pt-6 space-y-6">
            {/* Category Selection */}
            <div className="space-y-3">
              <Label>Accreditation Type</Label>
              <div className="flex gap-2">
                <Button
                  variant={category === "Internal" ? "default" : "outline"}
                  onClick={() => setCategory("Internal")}
                  className="flex-1"
                >
                  Internal
                </Button>
                <Button
                  variant={category === "External" ? "default" : "outline"}
                  onClick={() => setCategory("External")}
                  className="flex-1"
                >
                  External
                </Button>
              </div>
            </div>

            {/* Form Fields */}
            <div className="space-y-2">
              <Label htmlFor="name">Accreditation Name</Label>
              <Input id="name" placeholder="e.g., Certified ScrumMaster (CSM)" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                placeholder="Provide details about this accreditation..."
                rows={4}
                className="resize-none"
              />
            </div>

            {category === "External" && (
              <>
                <div className="space-y-2">
                  <Label htmlFor="issuer">Issuing Organization</Label>
                  <Input id="issuer" placeholder="e.g., Scrum Alliance" />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="issued">Issue Date</Label>
                    <Input id="issued" type="date" />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="expiry">Expiry Date</Label>
                    <Input id="expiry" type="date" />
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="credential">Credential ID (Optional)</Label>
                  <Input id="credential" placeholder="e.g., CSM-123456" />
                </div>

                <div className="space-y-2">
                  <Label>Certificate Document</Label>
                  <Button variant="outline" className="w-full gap-2 bg-transparent">
                    <Upload className="h-4 w-4" />
                    Upload Certificate
                  </Button>
                  <p className="text-xs text-muted-foreground">PDF, JPG, or PNG up to 5MB</p>
                </div>
              </>
            )}
          </CardContent>
        </Card>

        {/* Info Card */}
        <Card className="mb-6 border-secondary/20 bg-secondary/5">
          <CardContent className="p-4">
            <div className="flex gap-3">
              <AlertCircle className="h-5 w-5 text-secondary shrink-0 mt-0.5" />
              <div>
                <h3 className="font-semibold mb-1 text-sm">
                  {category === "Internal" ? "Internal Accreditation" : "Verification Required"}
                </h3>
                <p className="text-sm text-muted-foreground leading-relaxed">
                  {category === "Internal"
                    ? "Internal accreditations are earned by completing required policies and training modules."
                    : "External accreditations require verification by HR. Your submission will be reviewed within 3-5 business days."}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Submit Button */}
        <div className="space-y-3">
          <Button className="w-full gap-2">
            <Plus className="h-4 w-4" />
            Submit for Verification
          </Button>
          <Link href="/accreditations">
            <Button variant="outline" className="w-full bg-transparent">
              Cancel
            </Button>
          </Link>
        </div>
      </main>

      <MobileNav />
    </div>
  )
}
