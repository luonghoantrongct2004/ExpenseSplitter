export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
  timestamp: string;
}
export interface PagedList<T> {
  items: T[];
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
export interface User {
  id: string;
  name: string;
  email: string;
  avatarUrl?: string;
}

export interface AuthResponse {
  user: User;
  accessToken: string;
  expiresAt: string;
  message: string;
}
export interface ApiErrorResponse {
  message: string;
  hint?: string;
  errors?: Record<string, string[]>;
}

export interface GoogleLoginDto {
  token: string;
  deviceInfo?: string;
  ipAddress?: string;
}

export interface RefreshTokenDto {
  refreshToken: string;
}
