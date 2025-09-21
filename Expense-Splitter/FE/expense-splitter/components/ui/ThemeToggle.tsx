"use client";

import { Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";
import { useEffect, useState } from "react";

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null;

  return (
    <button
      onClick={() => setTheme(theme === "light" ? "dark" : "light")}
      className="p-2 rounded-lg bg-muted dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
      aria-label="Thay đổi giao diện"
    >
      {theme === "light" ? (
        <Moon className="w-5 h-5 text-foreground dark:text-gray-200" />
      ) : (
        <Sun className="w-5 h-5 text-foreground dark:text-gray-200" />
      )}
    </button>
  );
}
