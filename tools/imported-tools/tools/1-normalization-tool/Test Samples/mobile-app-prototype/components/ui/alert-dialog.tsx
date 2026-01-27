'use client';
import * as React from 'react';
import * as AlertDialogPrimitive from '@radix-ui/react-alert-dialog';
import { cn } from '@/lib/utils';
import { buttonVariants } from '@/components/ui/button';
function AlertDialog({ ...props }) {
  return <AlertDialogPrimitive.Root data-slot="alert-dialog" {...props} />;
}
function AlertDialogTrigger({ ...props }) {
  return <AlertDialogPrimitive.Trigger data-slot="alert-dialog-trigger" {...props} />;
}
function AlertDialogContent({ className, ...props }) {
  return <AlertDialogPrimitive.Portal data-slot="alert-dialog-portal" {...props} />;
}
function AlertDialogHeader({ className, ...props }) {
  return <div data-slot="alert-dialog-header" className={cn('flex flex-col gap-2 text-center sm:text-left', className)} {...props} />;
}
function AlertDialogFooter({ className, ...props }) {
  return <div data-slot="alert-dialog-footer" className={cn('flex flex-col-reverse gap-2 sm:flex-row sm:justify-end', className)} {...props} />;
}
function AlertDialogTitle({ className, ...props }) {
  return <AlertDialogPrimitive.Title data-slot="alert-dialog-title" className={cn('text-lg font-semibold', className)} {...props} />;
}
function AlertDialogDescription({ className, ...props }) {
  return <AlertDialogPrimitive.Description data-slot="alert-dialog-description" className={cn('text-muted-foreground text-sm', className)} {...props} />;
}
function AlertDialogAction({ className, ...props }) {
  return <AlertDialogPrimitive.Action className={cn(buttonVariants(), className)} {...props} />;
}
function AlertDialogCancel({ className, ...props }) {
  return <AlertDialogPrimitive.Cancel className={cn(buttonVariants({ variant: 'outline' }), className)} {...props} />;
}
export { AlertDialog, AlertDialogPortal, AlertDialogOverlay, AlertDialogTrigger, AlertDialogContent, AlertDialogHeader, AlertDialogFooter, AlertDialogTitle, AlertDialogDescription, AlertDialogAction, AlertDialogCancel };
