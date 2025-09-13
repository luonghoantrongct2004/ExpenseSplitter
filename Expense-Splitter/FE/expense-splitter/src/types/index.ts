export interface User {
  id: string;
  email: string;
  name: string;
  avatarUrl?: string;
  lastLoginAt?: string;
}

export interface Group {
  id: string;
  name: string;
  description?: string;
  currency: string;
  inviteCode: string;
  members: GroupMember[];
  createdAt: string;
  updatedAt: string;
}

export interface GroupMember {
  id: string;
  userId: string;
  user: User;
  groupId: string;
  role: 'ADMIN' | 'MEMBER';
  joinedAt: string;
}

export interface Expense {
  id: string;
  groupId: string;
  amount: number;
  description: string;
  category?: string;
  paidById: string;
  paidBy: User;
  splits: ExpenseSplit[];
  attachments: string[];
  expenseDate: string;
  createdAt: string;
  updatedAt: string;
}

export interface ExpenseSplit {
  id: string;
  expenseId: string;
  userId: string;
  user: User;
  amount: number;
  percentage?: number;
  isSettled: boolean;
}

export interface Balance {
  userId: string;
  user: User;
  amount: number; // positive = owed, negative = owes
}

export interface Settlement {
  id: string;
  groupId: string;
  fromUserId: string;
  fromUser: User;
  toUserId: string;
  toUser: User;
  amount: number;
  paymentMethod?: 'CASH' | 'BANK_TRANSFER' | 'MOMO' | 'ZALOPAY' | 'OTHER';
  referenceCode?: string;
  note?: string;
  status: 'PENDING' | 'COMPLETED' | 'CANCELLED';
  settledAt?: string;
  createdAt: string;
}

export interface Notification<T = unknown> {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  data?: T;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export enum NotificationType {
  EXPENSE_ADDED = 'EXPENSE_ADDED',
  EXPENSE_UPDATED = 'EXPENSE_UPDATED',
  EXPENSE_DELETED = 'EXPENSE_DELETED',
  PAYMENT_RECEIVED = 'PAYMENT_RECEIVED',
  PAYMENT_SENT = 'PAYMENT_SENT',
  PAYMENT_REMINDER = 'PAYMENT_REMINDER',
  MEMBER_JOINED = 'MEMBER_JOINED',
  MEMBER_LEFT = 'MEMBER_LEFT',
  GROUP_INVITE = 'GROUP_INVITE',
  SYSTEM = 'SYSTEM'
}
