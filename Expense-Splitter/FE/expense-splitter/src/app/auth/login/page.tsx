// app/login/page.tsx
"use client";

import { CredentialResponse } from "@react-oauth/google";
import { GoogleLogin } from "@react-oauth/google";
import { useAuthStore } from "@/src/store/auth.store";
import { useRouter } from "next/navigation";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Wallet } from "lucide-react";
import Link from "next/link";
import toast from "react-hot-toast";

export default function LoginPage() {
  const router = useRouter();
  const { login, isLoading } = useAuthStore();

  const handleGoogleSuccess = async (credentialResponse: CredentialResponse) => {
    try {
      if (!credentialResponse.credential) {
        throw new Error("No credential received");
      }
      
      await login(credentialResponse.credential);
      toast.success("Đăng nhập thành công! Cà phê sữa đá không? ☕");
      router.push("/");
    } catch (error) {
      if (error instanceof Error) {
        toast.error(error.message || "Hmm, có vẻ không được rồi. Thử lại nhé! 🔄");
      } else {
        toast.error("Hmm, có vẻ không được rồi. Thử lại nhé! 🔄");
      }
    }
  };

  const handleGoogleError = () => {
    toast.error("Đăng nhập Google thất bại");
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-primary/10 via-background to-primary/5">
      <div className="w-full max-w-md px-4">
        {/* Logo */}
        <div className="flex justify-center mb-8">
          <Link href="/" className="flex items-center gap-2">
            <div className="h-12 w-12 rounded-xl bg-primary flex items-center justify-center">
              <Wallet className="h-7 w-7 text-primary-foreground" />
            </div>
            <span className="text-2xl font-bold">Expense Splitter</span>
          </Link>
        </div>

        <Card>
          <CardHeader className="space-y-1">
            <CardTitle className="text-2xl text-center">Chào mừng bạn</CardTitle>
            <CardDescription className="text-center">
              Đăng nhập để quản lý chi tiêu nhóm của bạn
            </CardDescription>
          </CardHeader>
          <CardContent className="grid gap-4">
            <div className="flex justify-center">
              <GoogleLogin
                onSuccess={handleGoogleSuccess}
                onError={handleGoogleError}
                useOneTap
                theme="outline"
                size="large"
                text="signin_with"
                width={350}
              />
            </div>

            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <span className="w-full border-t" />
              </div>
              <div className="relative flex justify-center text-xs uppercase">
                <span className="bg-background px-2 text-muted-foreground">
                  An toàn & Bảo mật
                </span>
              </div>
            </div>

            <p className="text-center text-sm text-muted-foreground">
              Bằng việc đăng nhập, bạn đồng ý với{" "}
              <Link href="/terms" className="underline">
                Điều khoản sử dụng
              </Link>{" "}
              và{" "}
              <Link href="/privacy" className="underline">
                Chính sách bảo mật
              </Link>
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
