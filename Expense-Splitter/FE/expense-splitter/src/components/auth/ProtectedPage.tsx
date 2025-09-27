"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/src/store/auth.store";
import { Loading } from "@/components/ui/Loading";

export function ProtectedPage({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading, isInitialized } = useAuthStore();
  const router = useRouter();

  useEffect(() => {
    if (isInitialized && !isAuthenticated && !isLoading) {
      router.push("/auth/login");
    }
  }, [isAuthenticated, isLoading, isInitialized, router]);

  if (!isInitialized || isLoading) {
    return <Loading fullScreen />;
  }

  if (!isAuthenticated) {
    return null;
  }

  return <>{children}</>;
}
