// components/layouts/navbar.tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
} from "@/components/ui/navigation-menu";
import {
  Home,
  Receipt,
  Users,
  PieChart,
  Clock,
  CreditCard,
  Plus,
} from "lucide-react";

const navItems = [
  { name: "Trang chủ", href: "/", icon: Home },
  { name: "Chi tiêu", href: "/expenses", icon: Receipt },
  { name: "Nhóm", href: "/groups", icon: Users },
  { name: "Thống kê", href: "/analytics", icon: PieChart },
  { name: "Lịch sử", href: "/history", icon: Clock },
  { name: "Thanh toán", href: "/payments", icon: CreditCard },
];

export function Navbar() {
  const pathname = usePathname();

  return (
    <nav className="hidden lg:block border-b bg-background">
      <div className="container mx-auto px-4">
        <div className="flex h-14 items-center justify-between">
          <NavigationMenu>
            <NavigationMenuList className="gap-1">
              {navItems.map((item) => {
                const isActive = pathname === item.href;
                const Icon = item.icon;

                return (
                  <NavigationMenuItem key={item.name}>
                    <NavigationMenuLink asChild>
                      <Link
                        href={item.href}
                        className={cn(
                          "flex overflow-hidden items-center text-sm font-medium",
                          "focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring",
                          "disabled:pointer-events-none disabled:opacity-50",
                          "h-9 px-4 py-2 whitespace-pre group relative w-full justify-center gap-2 rounded-md",
                          "transition-all duration-300 ease-out",
                          // Theme-aware styles
                          "bg-muted/50 text-foreground",
                          // Border mặc định
                          "border-2 border-transparent",
                          // Hover state với border purple
                          "hover:bg-muted hover:text-foreground",
                          "hover:border-purple-500 hover:shadow-[0_0_15px_rgba(168,85,247,0.4)]",
                          // Active state
                          isActive && [
                            "bg-primary text-primary-foreground",
                            "hover:bg-primary/90 hover:text-primary-foreground",
                            "shadow-sm",
                            "hover:border-purple-400",
                          ]
                        )}
                      >
                        <Icon className="mr-2 h-4 w-4" />
                        {item.name}
                      </Link>
                    </NavigationMenuLink>
                  </NavigationMenuItem>
                );
              })}
            </NavigationMenuList>
          </NavigationMenu>

          <Button asChild>
            <Link href="/expenses/new">
              <Plus className="mr-2 h-4 w-4" />
              Thêm chi tiêu
            </Link>
          </Button>
        </div>
      </div>
    </nav>
  );
}
