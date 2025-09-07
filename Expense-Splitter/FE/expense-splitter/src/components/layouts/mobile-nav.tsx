// components/layouts/mobile-nav.tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import {
  Home,
  Receipt,
  Users,
  PieChart,
  Menu,
  Plus,
  CreditCard,
  Clock,
  UserCircle
} from "lucide-react";
import { useState } from "react";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";

const bottomNavItems = [
  { id: "home", name: "Trang chủ", href: "/", icon: Home },
  { id: "expenses", name: "Chi tiêu", href: "/expenses", icon: Receipt },
  { id: "add-expense", name: "Thêm", href: "#", icon: Plus, isAction: true },
  { id: "groups", name: "Nhóm", href: "/groups", icon: Users },
  { id: "menu", name: "Menu", href: "#", icon: Menu, isMenu: true }, // Đổi tên từ "Thêm" thành "Menu"
];

const menuItems = [
  { id: "analytics", name: "Thống kê", href: "/analytics", icon: PieChart },
  { id: "history", name: "Lịch sử", href: "/history", icon: Clock },
  { id: "payments", name: "Thanh toán", href: "/payments", icon: CreditCard },
  { id: "profile", name: "Hồ sơ", href: "/profile", icon: UserCircle },
];

export function MobileNav() {
  const pathname = usePathname();
  const [sheetOpen, setSheetOpen] = useState(false);

  return (
    <>
      {/* Bottom Navigation Bar */}
      <nav className="fixed bottom-0 left-0 right-0 z-50 bg-background border-t lg:hidden">
        <div className="grid grid-cols-5 h-16">
          {bottomNavItems.map((item) => {
            const Icon = item.icon;
            const isActive = pathname === item.href;

            // Special handling for action button
            if (item.isAction) {
              return (
                <Link
                  key={item.id} // Dùng id thay vì name
                  href="/expenses/new"
                  className="flex flex-col items-center justify-center"
                >
                  <div className="relative">
                    <div className="absolute -inset-2 bg-primary rounded-full" />
                    <Icon className="relative h-6 w-6 text-primary-foreground" />
                  </div>
                </Link>
              );
            }

            // Special handling for menu button
            if (item.isMenu) {
              return (
                <Sheet key={item.id} open={sheetOpen} onOpenChange={setSheetOpen}>
                  <SheetTrigger asChild>
                    <button className="flex flex-col items-center justify-center text-muted-foreground">
                      <Icon className="h-5 w-5" />
                      <span className="text-xs mt-1">{item.name}</span>
                    </button>
                  </SheetTrigger>
                  <SheetContent side="bottom" className="h-auto">
                    <SheetHeader>
                      <SheetTitle>Menu</SheetTitle>
                    </SheetHeader>
                    <div className="grid gap-4 py-4">
                      {menuItems.map((menuItem) => {
                        const MenuIcon = menuItem.icon;
                        return (
                          <Link
                            key={menuItem.id} // Dùng id thay vì name
                            href={menuItem.href}
                            onClick={() => setSheetOpen(false)}
                            className="flex items-center space-x-3 rounded-lg p-3 hover:bg-accent"
                          >
                            <MenuIcon className="h-5 w-5" />
                            <span className="font-medium">{menuItem.name}</span>
                          </Link>
                        );
                      })}
                    </div>
                  </SheetContent>
                </Sheet>
              );
            }

            return (
              <Link
                key={item.id} // Dùng id thay vì name
                href={item.href}
                className={cn(
                  "flex flex-col items-center justify-center",
                  isActive
                    ? "text-primary"
                    : "text-muted-foreground hover:text-foreground"
                )}
              >
                <Icon className="h-5 w-5" />
                <span className="text-xs mt-1">{item.name}</span>
              </Link>
            );
          })}
        </div>
      </nav>

      {/* Add padding to main content to account for fixed bottom nav */}
      <div className="h-16 lg:hidden" />
    </>
  );
}
