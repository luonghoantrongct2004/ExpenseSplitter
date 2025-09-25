"use client";

import { GoogleLogin, CredentialResponse } from "@react-oauth/google";
import { useAuthStore } from "@/src/store/auth.store";
import { useRouter } from "next/navigation";
import { useTheme } from "next-themes";
import toast from "react-hot-toast";

export function GoogleLoginButton() {
  const router = useRouter();
  const { login } = useAuthStore();
  const { theme } = useTheme();

  const handleSuccess = async (credentialResponse: CredentialResponse) => {
    try {
      if (!credentialResponse.credential) {
        throw new Error("No credential received");
      }

      await login(credentialResponse.credential);

      toast.success("Đăng nhập thành công! Welcome to the club 🎉");
      router.push("/");
    } catch (error) {
      console.error("Login error:", error);
      toast.error("Đăng nhập thất bại");
    }
  };

  return (
    <GoogleLogin
      onSuccess={handleSuccess}
      onError={() => toast.error("Đăng nhập Google thất bại")}
      theme={theme === "dark" ? "filled_black" : "outline"}
      size="medium"
      text="signin"
      shape="rectangular"
      logo_alignment="left"
    />
  );
}
