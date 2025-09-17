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
  isActive: boolean;
  createdById: string;
  createdBy: User;
  createdAt: string;
  updatedAt: string;
  members: GroupMember[];
  totalExpenses?: number;
  memberCount?: number;
}


export interface GroupMember {
  id: string;
  groupId: string;
  userId: string;
  role: 'Admin' | 'Member';
  joinedAt: string;
  leftAt?: string;
  isActive: boolean;
  user: User;
}
