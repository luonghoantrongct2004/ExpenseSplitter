import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { authService } from '../lib/services/auth.service';

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
  login: (googleToken: string) => Promise<void>;
  logout: () => Promise<void>;
  checkAuth: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,
      isLoading: false,

      login: async (googleToken: string) => {
        try {
          set({ isLoading: true });
          const response = await authService.googleLogin(googleToken);
          
          set({ 
            user: response.user, 
            isAuthenticated: true,
            isLoading: false 
          });
        } catch (error) {
          set({ isLoading: false });
          throw error;
        }
      },

      logout: async () => {
        try {
          await authService.logout();
          set({ user: null, isAuthenticated: false });
        } catch (error) {
          // Still clear local state
          set({ user: null, isAuthenticated: false });
        }
      },

      checkAuth: async () => {
        try {
          if (!authService.isAuthenticated()) {
            set({ user: null, isAuthenticated: false });
            return;
          }

          const user = await authService.getCurrentUser();
          set({ user, isAuthenticated: true });
        } catch (error) {
          set({ user: null, isAuthenticated: false });
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ user: state.user }),
    }
  )
);
