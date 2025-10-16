import { User } from "../dto";

export interface PagedList<T> {
  items: T[];
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface GroupListDto {
  id: string;
  name: string;
  memberCount: number;
  userBalance: number;
  lastActivity: string;
  isAdmin: boolean;
}

export interface GroupDetailDto {
  id: string;
  name: string;
  description?: string;
  currency: string;
  inviteCode: string;
  isActive: boolean;
  createdById: string;
  createdAt: string;
  members: GroupMemberDto[];
  totalExpenses: number;
  userBalance: number;
}

export interface GroupMemberDto {
  id: string;
  userId: string;
  groupId: string;
  role: "Admin" | "Member";
  joinedAt: string;
  user: User;
  avatarUrl: string;
  balance: number;
}

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

export interface JoinGroupRequest {
  inviteCode: string;
}

export interface InviteMemberRequest {
  email: string;
}
