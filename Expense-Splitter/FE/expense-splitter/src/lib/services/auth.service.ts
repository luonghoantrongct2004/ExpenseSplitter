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
  //NOTE - Service login google
  async googleLogin(googleToken: string): Promise<AuthResponse> {
    try {
      const response = await axios.post(
        `${API_URL}/api/auth/google`,
        {
          token: googleToken,
          deviceInfo: navigator.userAgent,
        },
        {
          withCredentials: true,
        }
      );

      this.setAccessToken(response.data.accessToken);
      localStorage.setItem('user', JSON.stringify(response.data.user));
      
      return response.data;
    } catch (error: unknown) {
          if (isAxiosError(error)) {
        throw new Error(error.response?.data?.message || 'Login failed');
      }
      throw new Error('Login failed');
    }
  }
  //NOTE - Service refresh token
  async refreshToken(): Promise<string> {
    try {
      const response = await axios.post(
        `${API_URL}/api/auth/refresh`,
        {},
        {
          withCredentials: true,
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

  setAccessToken(token: string) {
    Cookies.set('accessToken', token, { 
      expires: 7,
      secure: process.env.NODE_ENV === 'production',
      sameSite: 'lax',
      path: '/'
    });
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
  //NOTE - Service logout
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
  //NOTE - Service logout all device
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

  getAccessToken(): string | undefined {
    return Cookies.get('accessToken');
  }
  //NOTE - Service get info user
  getUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  clearTokens() {
    Cookies.remove('accessToken', { path: '/' });
    localStorage.removeItem('user');
  }
  // NOTE - Service Is Authenticated
  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }
}

export const authService = new AuthService();
