// app/layout.tsx
import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Toaster } from "react-hot-toast";
import { cn } from "@/lib/utils";
import { Providers } from "../components/providers";
import { Header } from "../components/layouts/header";
import { Navbar } from "../components/layouts/navbar";
import { MobileNav } from "../components/layouts/mobile-nav";

const inter = Inter({
  subsets: ["latin", "vietnamese"],
  display: "swap",
  variable: "--font-inter",
});

export const metadata: Metadata = {
  title: "Expense Splitter - Ứng dụng chia tiền thông minh",
  description: "Quản lý và chia sẻ chi phí nhóm một cách dễ dàng, minh bạch và công bằng",
  keywords: ["chia tiền", "quản lý chi phí", "expense splitter", "ứng dụng tài chính"],
  openGraph: {
    title: "Expense Splitter",
    description: "Ứng dụng chia tiền thông minh",
    images: ["/og-image.png"],
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" suppressHydrationWarning>
      <body className={cn(
        "min-h-screen bg-background font-sans antialiased",
        inter.variable
      )}>
        <Providers>
          <div className="relative flex min-h-screen flex-col">
            <Header />
            <Navbar />
            <main className="flex-1">
              <div className="container mx-auto px-4 py-6 md:px-6 lg:px-8">
                {children}
              </div>
            </main>
             <MobileNav />
          </div>
          <Toaster
            position="top-right"
            toastOptions={{
              className: "!bg-background !text-foreground !border !border-border",
              duration: 4000,
            }}
          />
        </Providers>
      </body>
    </html>
  );
}
