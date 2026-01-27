'use client';
import * as React from 'react';
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { XIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
function Dialog({ ...props }) {
  return <DialogPrimitive.Root data-slot="dialog" {...props} />;
}
function DialogTrigger({ ...props }) {
  return <DialogPrimitive.Trigger data-slot="dialog-trigger" {...props} />;
}
function DialogClose({ ...props }) {
  return <DialogPrimitive.Close data-slot="dialog-close" {...props} />;
}
function DialogContent({ className, children, showCloseButton = true, ...props }) {
  return <DialogPrimitive.Portal data-slot="dialog-portal" {...props} />;
}
function DialogHeader({ className, ...props }) {
  return <div data-slot="dialog-header" className={cn('flex flex-col gap-2 text-center sm:text-left', className)} {...props} />;
}
function DialogFooter({ className, ...props }) {
  return <div data-slot="dialog-footer" className={cn('flex flex-col-reverse gap-2 sm:flex-row sm:justify-end', className)} {...props} />;
}
function DialogTitle({ className, ...props }) {
  return <DialogPrimitive.Title data-slot="dialog-title" className={cn('text-lg leading-none font-semibold', className)} {...props} />;
}
function DialogDescription({ className, ...props }) {
  return <DialogPrimitive.Description data-slot="dialog-description" className={cn('text-muted-foreground text-sm', className)} {...props} />;
}
export { Dialog, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogOverlay, DialogPortal, DialogTitle, DialogTrigger };
