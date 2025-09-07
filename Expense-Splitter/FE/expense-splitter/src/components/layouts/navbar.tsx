// components/layouts/navbar.tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { NavigationMenu, NavigationMenuItem, NavigationMenuLink, NavigationMenuList } from "@/components/ui/navigation-menu";
import {
  Home,
  Receipt,
  Users,
  PieChart,
  Clock,      // Changed from History to Clock
  CreditCard,
  Plus
} from "lucide-react";

const navItems = [
  { name: "Trang chủ", href: "/", icon: Home },
  { name: "Chi tiêu", href: "/expenses", icon: Receipt },
  { name: "Nhóm", href: "/groups", icon: Users },
  { name: "Thống kê", href: "/analytics", icon: PieChart },
  { name: "Lịch sử", href: "/history", icon: Clock },      // Changed icon
  { name: "Thanh toán", href: "/payments", icon: CreditCard },
];

export function Navbar() {
  const pathname = usePathname();

  return (
    <nav className="hidden lg:block border-b">
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
                          "group inline-flex h-10 w-max items-center justify-center rounded-md bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground focus:outline-none disabled:pointer-events-none disabled:opacity-50",
                          isActive && "bg-accent text-accent-foreground"
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
