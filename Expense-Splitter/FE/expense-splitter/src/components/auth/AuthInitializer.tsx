"use client";

import { useEffect } from "react";
import { useAuthStore } from "@/src/store/auth.store";

export function AuthInitializer({ children }: { children: React.ReactNode }) {
  const { initialize, isInitialized } = useAuthStore();

  useEffect(() => {
    initialize();
  }, [initialize]);

  return <>{children}</>;
}
