import { apiClient } from "./client";

export interface GoogleLoginDto {
  idToken: string;
}

export interface AuthResponse {
  user: {
    id: string;
    email: string;
    name: string;
    avatarUrl?: string;
  };
  accessToken: string;
  expiresAt: string;
}

export const authApi = {
  googleLogin: (data: GoogleLoginDto) => 
    apiClient.post<AuthResponse>('/api/auth/google', data),
    
  getCurrentUser: () => 
    apiClient.get('/api/auth/me'),
    
  logout: () => 
    apiClient.post('/api/auth/logout'),
};
