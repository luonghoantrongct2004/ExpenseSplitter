/* eslint-disable @typescript-eslint/no-explicit-any */
import axios, { AxiosRequestConfig, AxiosError } from "axios";
import toast from "react-hot-toast";
import Router from "next/router";

const API_URL = process.env.NEXT_PUBLIC_API_URL;

// Check if we're on the client side
const isClient = typeof window !== "undefined";

// Create a separate axios instance for refresh to avoid interceptor loops
const refreshAxios = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

export const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true,
});

// Queue for failed requests during token refresh
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: any) => void;
  reject: (error?: any) => void;
}> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Request interceptor
apiClient.interceptors.request.use(
  (config) => {
    // Only access localStorage on client side
    if (isClient) {
      const token = localStorage.getItem("accessToken");
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor
apiClient.interceptors.response.use(
  (response) => {
    // Only show toast on client side
    if (isClient) {
      const method = response.config.method?.toLowerCase();
      const showSuccessFor = ["post", "put", "patch", "delete"];

      if (response.data?.message && showSuccessFor.includes(method || "")) {
        toast.success(response.data.message);
      }
    }

    return response;
  },
  async (error: AxiosError<any>) => {
    // Skip interceptor logic on server side
    if (!isClient) {
      return Promise.reject(error);
    }

    const originalRequest = error.config as AxiosRequestConfig & {
      _retry?: boolean;
    };

    // Handle 401 Unauthorized
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // Queue this request
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            if (originalRequest.headers && token) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            return apiClient(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        console.log("🔄 Attempting to refresh token...");

        // Call refresh endpoint - cookies will be sent automatically
        const response = await refreshAxios.post("/api/auth/refresh", {});

        const { accessToken, expiresAt } = response.data;

        if (accessToken) {
          console.log("✅ Token refreshed successfully");

          // Update access token
          localStorage.setItem("accessToken", accessToken);

          // Process queued requests
          processQueue(null, accessToken);

          // Retry original request
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          }

          return apiClient(originalRequest);
        }
      } catch (refreshError: any) {
        console.error("❌ Token refresh failed:", refreshError);

        processQueue(refreshError, null);

        // Clear auth data
        localStorage.removeItem("accessToken");

        // Clear auth store - dynamic import to avoid issues
        try {
          const { useAuthStore } = await import("@/src/store/auth.store");
          useAuthStore.getState().logout();
        } catch (e) {}

        if (!Router.pathname.includes("/auth/login")) {
          toast.error("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", {
            icon: "🔒",
          });
          await Router.push("/auth/login");
        }

        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    if (error.response?.data) {
      const { message, hint } = error.response.data;

      if (message) {
        const fullMessage = hint ? `${message} ${hint}` : message;

        if (error.response.status >= 500) {
          toast.error(fullMessage, {
            icon: "😵",
            duration: 5000,
          });
        } else if (error.response.status >= 400) {
          toast.error(fullMessage, {
            icon: "⚠️",
            duration: 4000,
          });
        }
      }
    } else if (error.code === "ECONNABORTED") {
      toast.error("Kết nối timeout! Vui lòng thử lại", {
        icon: "⏱️",
      });
    } else if (!navigator.onLine) {
      toast.error("Không có kết nối mạng", {
        icon: "📡",
      });
    }

    return Promise.reject(error);
  }
);

export const api = {
  get: <T = unknown>(url: string, config?: AxiosRequestConfig) =>
    apiClient.get<T>(url, config).then((res) => res.data),

  post: <T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ) => apiClient.post<T>(url, data, config).then((res) => res.data),

  put: <T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ) => apiClient.put<T>(url, data, config).then((res) => res.data),

  patch: <T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig
  ) => apiClient.patch<T>(url, data, config).then((res) => res.data),

  delete: <T = unknown>(url: string, config?: AxiosRequestConfig) =>
    apiClient.delete<T>(url, config).then((res) => res.data),
};

export default apiClient;
