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
export interface CreateGroupDto {
  name: string;
  description?: string;
  currency: string;
}

export interface UpdateGroupDto {
  name?: string;
  description?: string;
  currency?: string;
}

export interface JoinGroupDto {
  inviteCode: string;
}

// Expense DTOs
export interface CreateExpenseDto {
  groupId: string;
  amount: number;
  description: string;
  category?: string;
  paidById: string;
  participantIds: string[];
  splitType: 'EQUAL' | 'PERCENTAGE' | 'CUSTOM';
  customSplits?: CustomSplit[];
  expenseDate?: string;
}

export interface CustomSplit {
  userId: string;
  amount?: number;
  percentage?: number;
}

export interface UpdateExpenseDto {
  amount?: number;
  description?: string;
  category?: string;
  expenseDate?: string;
}

// Settlement DTOs
export interface CreateSettlementDto {
  groupId: string;
  toUserId: string;
  amount: number;
  paymentMethod?: string;
  note?: string;
}
