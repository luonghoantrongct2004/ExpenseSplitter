import { useGoogleLogin as useGoogleOAuth } from "@react-oauth/google";
import { useAuthStore } from "@/src/store/auth.store";
import { useRouter } from "next/navigation";
import toast from "react-hot-toast";

export function useGoogleLogin() {
  const router = useRouter();
  const { login } = useAuthStore();

  const googleLogin = useGoogleOAuth({
    flow: 'implicit',
    onSuccess: async (tokenResponse) => {
      try {
        const userInfo = await fetch('https://www.googleapis.com/oauth2/v3/userinfo', {
          headers: {
            Authorization: `Bearer ${tokenResponse.access_token}`,
          },
        });
        
        await userInfo.json();
        
        await login(tokenResponse.access_token);
        
        toast.success("Đăng nhập thành công! Welcome to the club 🎉");
        router.push("/");
      } catch (error) {
        toast.error("Đăng nhập thất bại");
      }
    },
    onError: () => {
      toast.error("Đăng nhập Google thất bại");
    },
  });

  return googleLogin;
}
