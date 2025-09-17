import { Group } from "./model";

// types/api.types.ts
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

export interface ExpenseResponse {
  id: string;
  title: string;
  amount: number;
  groupId: string;
  createdBy: User;
  participants: User[];
  createdAt: string;
}

export interface GroupResponse {
  id: string;
  name: string;
  members: User[];
  totalExpenses: number;
  createdAt: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
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

// Group DTOs

export interface CreateGroupRequest {
  name: string;
  description?: string;
  currency?: string;
}

export interface UpdateGroupRequest {
  name?: string;
  description?: string;
  currency?: string;
}

export interface InviteMemberRequest {
  email?: string;
  inviteCode?: string;
}

export interface GroupListResponse {
  groups: Group[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
}