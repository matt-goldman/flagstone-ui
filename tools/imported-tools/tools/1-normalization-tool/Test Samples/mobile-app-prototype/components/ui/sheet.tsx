'use client';
import * as React from 'react';
import * as SheetPrimitive from '@radix-ui/react-dialog';
import { XIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
function Sheet({ ...props }) {
  return <SheetPrimitive.Root data-slot="sheet" {...props} />;
}
function SheetTrigger({ ...props }) {
  return <SheetPrimitive.Trigger data-slot="sheet-trigger" {...props} />;
}
function SheetClose({ ...props }) {
  return <SheetPrimitive.Close data-slot="sheet-close" {...props} />;
}
function SheetContent({ className, children, side = 'right', ...props }) {
  return <SheetPrimitive.Portal data-slot="sheet-portal" {...props} />;
}
function SheetHeader({ className, ...props }) {
  return <div data-slot="sheet-header" className={cn('flex flex-col gap-1.5 p-4', className)} {...props} />;
}
function SheetFooter({ className, ...props }) {
  return <div data-slot="sheet-footer" className={cn('mt-auto flex flex-col gap-2 p-4', className)} {...props} />;
}
function SheetTitle({ className, ...props }) {
  return <SheetPrimitive.Title data-slot="sheet-title" className={cn('text-foreground font-semibold', className)} {...props} />;
}
function SheetDescription({ className, ...props }) {
  return <SheetPrimitive.Description data-slot="sheet-description" className={cn('text-muted-foreground text-sm', className)} {...props} />;
}
export { Sheet, SheetTrigger, SheetClose, SheetContent, SheetHeader, SheetFooter, SheetTitle, SheetDescription };
