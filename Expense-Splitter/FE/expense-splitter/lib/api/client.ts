// lib/api/client.ts
import axios, { AxiosRequestConfig, AxiosResponse, AxiosError } from 'axios';
import Cookies from 'js-cookie';
import toast from 'react-hot-toast';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Define types
interface ApiError extends AxiosError {
  response?: AxiosResponse<{
    message?: string;
    hint?: string;
  }>;
}

interface ApiResponse<T = unknown> {
  data: T;
  message?: string;
}

export const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true,
});

// Request interceptor
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken') || Cookies.get('accessToken');
    
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
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
    const method = response.config.method?.toLowerCase();
    const showSuccessFor = ['post', 'put', 'patch', 'delete'];
    
    if (response.data?.message && showSuccessFor.includes(method || '')) {
      toast.success(response.data.message);
    }
    
    return response;
  },
  async (error: ApiError) => {
    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };
    
    // Handle 401
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const response = await apiClient.post('/api/auth/refresh');
        
        const { accessToken } = response.data;
        if (accessToken) {
          localStorage.setItem('accessToken', accessToken);
          Cookies.set('accessToken', accessToken, { secure: true, sameSite: 'strict' });
          
          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${accessToken}`;
          }
          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        localStorage.removeItem('accessToken');
        Cookies.remove('accessToken');
        
        if (!window.location.pathname.includes('/login')) {
          window.location.href = '/login';
        }
        
        return Promise.reject(refreshError);
      }
    }
    
    // Handle error messages
    if (error.response?.data) {
      const { message, hint } = error.response.data;
      
      if (message) {
        const fullMessage = hint ? `${message} ${hint}` : message;
        
        if (error.response.status >= 500) {
          toast.error(fullMessage, {
            icon: '😵',
            duration: 5000,
          });
        } else if (error.response.status >= 400) {
          toast.error(fullMessage, {
            icon: '⚠️',
            duration: 4000,
          });
        }
      }
    } else if (error.code === 'ECONNABORTED') {
      toast.error('Kết nối timeout! Vui lòng thử lại', {
        icon: '⏱️',
      });
    } else if (!navigator.onLine) {
      toast.error('Không có kết nối mạng', {
        icon: '📡',
      });
    } else {
      toast.error('Có lỗi xảy ra, vui lòng thử lại', {
        icon: '❌',
      });
    }
    
    return Promise.reject(error);
  }
);

// Debug logging
if (process.env.NODE_ENV === 'development') {
  apiClient.interceptors.request.use(
    (config) => {
      console.log(`🚀 [API] ${config.method?.toUpperCase()} ${config.url}`, {
        params: config.params,
        data: config.data,
      });
      return config;
    }
  );

  apiClient.interceptors.response.use(
    (response) => {
      console.log(`✅ [API] ${response.config.method?.toUpperCase()} ${response.config.url}`, {
        status: response.status,
        data: response.data,
      });
      return response;
    },
    (error: AxiosError) => {
      if (error.response) {
        console.error(`❌ [API] ${error.config?.method?.toUpperCase()} ${error.config?.url}`, {
          status: error.response.status,
          data: error.response.data,
        });
      }
      return Promise.reject(error);
    }
  );
}

// Helper functions with proper types
export const api = {
  get: <T = unknown>(url: string, config?: AxiosRequestConfig) => 
    apiClient.get<T>(url, config).then(res => res.data),
    
  post: <T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) => 
    apiClient.post<T>(url, data, config).then(res => res.data),
    
  put: <T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) => 
    apiClient.put<T>(url, data, config).then(res => res.data),
    
  patch: <T = unknown>(url: string, data?: unknown, config?: AxiosRequestConfig) => 
    apiClient.patch<T>(url, data, config).then(res => res.data),
    
  delete: <T = unknown>(url: string, config?: AxiosRequestConfig) => 
    apiClient.delete<T>(url, config).then(res => res.data),
};

export default apiClient;
