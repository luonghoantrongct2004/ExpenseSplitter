'use client';

import { useTheme } from 'next-themes';
import { useEffect, useState } from 'react';

export function BackgroundGlow({ children }: { children: React.ReactNode }) {
  const { theme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) {
    return <>{children}</>;
  }

  return (
    <div className="relative min-h-screen">
      {/* Light Mode Background */}
      <div
        className={`fixed inset-0 z-0 transition-opacity duration-500 ${
          theme === 'light' ? 'opacity-100' : 'opacity-0'
        }`}
        style={{
          background: "#ffffff",
          backgroundImage: `
            radial-gradient(
              circle at top center,
              rgba(173, 109, 244, 0.5),
              transparent 70%
            )
          `,
          filter: "blur(80px)",
          backgroundRepeat: "no-repeat",
        }}
      />

      {/* Dark Mode Background */}
      <div
        className={`fixed inset-0 z-0 transition-opacity duration-500 ${
          theme === 'dark' ? 'opacity-100' : 'opacity-0'
        }`}
        style={{
          background: "radial-gradient(ellipse 80% 60% at 50% 0%, rgba(139, 92, 246, 0.25), transparent 70%), #000000",
        }}
      />

      {/* Content */}
      <div className="relative z-10">
        {children}
      </div>
    </div>
  );
}
