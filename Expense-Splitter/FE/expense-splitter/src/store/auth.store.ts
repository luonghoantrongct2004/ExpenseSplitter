import { create } from "zustand";
import { persist } from "zustand/middleware";
import { authService } from "../lib/services/auth.service";

interface User {
  id: string;
  name: string;
  email: string;
  avatarUrl?: string;
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  isInitialized: boolean; // Add this
  login: (googleToken: string) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
  initialize: () => Promise<void>; // Add this
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      isAuthenticated: false,
      isLoading: true,
      isInitialized: false,

      initialize: async () => {
        if (get().isInitialized) return;

        set({ isLoading: true });

        try {
          // Check if token exists
          const token = authService.getAccessToken();

          if (token) {
            // Try to get user from localStorage first
            const savedUser = authService.getUser();

            if (savedUser) {
              set({
                user: savedUser,
                isAuthenticated: true,
                isInitialized: true,
                isLoading: false,
              });
            } else {
              // If no saved user, fetch from API
              try {
                const userData = await authService.getCurrentUser();
                set({
                  user: userData,
                  isAuthenticated: true,
                  isInitialized: true,
                  isLoading: false,
                });
              } catch (error) {
                // Token might be invalid
                authService.clearTokens();
                set({
                  user: null,
                  isAuthenticated: false,
                  isInitialized: true,
                  isLoading: false,
                });
              }
            }
          } else {
            set({
              user: null,
              isAuthenticated: false,
              isInitialized: true,
              isLoading: false,
            });
          }
        } catch (error) {
          console.error("Initialize auth error:", error);
          set({
            user: null,
            isAuthenticated: false,
            isInitialized: true,
            isLoading: false,
          });
        }
      },

      login: async (googleToken: string) => {
        try {
          set({ isLoading: true });
          const response = await authService.googleLogin(googleToken);

          set({
            user: response.user,
            isAuthenticated: true,
            isLoading: false,
          });
        } catch (error) {
          set({ isLoading: false });
          throw error;
        }
      },

      logout: async () => {
        try {
          await authService.logout();
        } catch (error) {
          console.error("Logout error:", error);
        } finally {
          authService.clearTokens();
          set({
            user: null,
            isAuthenticated: false,
          });
        }
      },

      checkAuth: async () => {
        try {
          const token = authService.getAccessToken();

          if (!token) {
            set({ user: null, isAuthenticated: false });
            return;
          }

          const user = await authService.getCurrentUser();
          set({ user, isAuthenticated: true });
        } catch (error) {
          authService.clearTokens();
          set({ user: null, isAuthenticated: false });
        }
      },
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
