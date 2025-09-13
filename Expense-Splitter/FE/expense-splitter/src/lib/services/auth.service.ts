import axios, { isAxiosError } from 'axios'; 
import Cookies from 'js-cookie';

const API_URL = process.env.NEXT_PUBLIC_API_URL;

interface AuthResponse {
  user: {
    id: string;
    name: string;
    email: string;
    avatarUrl?: string;
  };
  accessToken: string;
  expiresAt: string;
  message: string;
}

class AuthService {
  async googleLogin(googleToken: string): Promise<AuthResponse> {
    try {
      const response = await axios.post(
        `${API_URL}/api/auth/google`,
        {
          token: googleToken,
          deviceInfo: navigator.userAgent,
        },
        {
          withCredentials: true, // Để gửi và nhận cookies
        }
      );

      // Lưu access token
      this.setAccessToken(response.data.accessToken);
      
      return response.data;
    } catch (error: unknown) {
          if (isAxiosError(error)) {
        throw new Error(error.response?.data?.message || 'Login failed');
      }
      throw new Error('Login failed');
    }
  }

  async refreshToken(): Promise<string> {
    try {
      const response = await axios.post(
        `${API_URL}/api/auth/refresh`,
        {},
        {
          withCredentials: true, // Gửi refresh token từ cookie
        }
      );

      const { accessToken } = response.data;
      this.setAccessToken(accessToken);
      
      return accessToken;
    } catch (error) {
      this.logout();
      throw error;
    }
  }

  async getCurrentUser() {
    try {
      const response = await axios.get(`${API_URL}/api/auth/me`, {
        headers: {
          Authorization: `Bearer ${this.getAccessToken()}`,
        },
      });
      
      return response.data.data;
    } catch (error) {
      throw error;
    }
  }

  async logout() {
    try {
      await axios.post(
        `${API_URL}/api/auth/logout`,
        {},
        {
          headers: {
            Authorization: `Bearer ${this.getAccessToken()}`,
          },
          withCredentials: true,
        }
      );
    } catch (error) {
        if (isAxiosError(error)) {
        throw new Error(error.response?.data?.message || 'Login failed');
      }
      throw new Error('Login failed');
    } finally {
      this.clearTokens();
    }
  }

  async logoutAllDevices() {
    try {
      await axios.post(
        `${API_URL}/api/auth/logout-all`,
        {},
        {
          headers: {
            Authorization: `Bearer ${this.getAccessToken()}`,
          },
          withCredentials: true,
        }
      );
      
      this.clearTokens();
    } catch (error) {
      throw error;
    }
  }

  // Token management
  setAccessToken(token: string) {
    Cookies.set('accessToken', token, { 
      secure: true,
      sameSite: 'strict'
    });
  }

  getAccessToken(): string | undefined {
    return Cookies.get('accessToken');
  }

  clearTokens() {
    Cookies.remove('accessToken');
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
}

export const authService = new AuthService();
