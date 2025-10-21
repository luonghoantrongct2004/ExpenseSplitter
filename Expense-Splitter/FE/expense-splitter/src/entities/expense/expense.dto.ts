/* eslint-disable @typescript-eslint/no-explicit-any */
export enum ExpenseCategory {
  Food = 0,
  Transport = 1,
  Accommodation = 2,
  Entertainment = 3,
  Shopping = 4,
  Other = 5,
}

export enum SplitType {
  Equal = "Equal",
  Percentage = "Percentage",
  Amount = "Amount",
}

// Core Expense DTOs
export interface CreateExpenseDto {
  amount: number;
  description: string;
  note?: string;
  category?: ExpenseCategory;
  paidById: string;
  expenseDate: string;
  splits: CreateExpenseSplitDto[];
}

export interface CreateExpenseSplitDto {
  userId: string;
  amount: number;
  description?: string;
}

export interface UpdateExpenseDto {
  amount: number;
  description: string;
  note?: string;
  category?: ExpenseCategory;
  paidById: string;
  expenseDate: string;
  splits: CreateExpenseSplitDto[];
}

export interface ExpenseDto {
  id: string;
  groupId: string;
  amount: number;
  currency: string;
  description: string;
  note?: string;
  category?: ExpenseCategory;
  paidById: string;
  paidByName: string;
  paidByAvatar?: string;
  expenseDate: string;
  createdAt: string;
  createdById: string;
  createdByName: string;
  splits: ExpenseSplitDto[];
  canEdit: boolean;
  canDelete: boolean;
}

export interface ExpenseSplitDto {
  userId: string;
  userName: string;
  avatar?: string;
  amount: number;
  isSettled: boolean;
  description?: string;
}

// Filter and Search DTOs
export interface ExpenseFilterDto {
  pageNumber: number;
  pageSize: number;
  orderBy?: string;
  orderByDescending?: boolean;
  searchTerm?: string;
  startDate?: string;
  endDate?: string;
  category?: ExpenseCategory;
  paidById?: string;
  participantId?: string;
  includeDeleted?: boolean;
}

export interface ExpenseSearchDto {
  pageNumber: number;
  pageSize: number;
  orderBy?: string;
  orderByDescending?: boolean;
  searchTerm?: string;
  groupId?: string;
  startDate?: string;
  endDate?: string;
  categories?: ExpenseCategory[];
  paidByIds?: string[];
  participantIds?: string[];
  minAmount?: number;
  maxAmount?: number;
  isSettled?: boolean;
  includeDeleted?: boolean;
  sortBy?: "ExpenseDate" | "Amount" | "CreatedAt";
}

// Frontend-specific DTOs

// Lightweight DTO for list views
export interface ExpenseListItemDto {
  id: string;
  amount: number;
  currency: string;
  description: string;
  category?: ExpenseCategory;
  categoryName: string;
  paidById: string;
  paidByName: string;
  paidByAvatar?: string;
  expenseDate: string;
  createdAt: string;
  splitCount: number;
  myShare: number;
  isSettled: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

// Form DTO with UI helpers
export interface ExpenseFormDto {
  id?: string;
  amount: number;
  description: string;
  note?: string;
  category?: ExpenseCategory;
  paidById: string;
  expenseDate: string;
  splitType: SplitType;
  splits: ExpenseFormSplitDto[];
  attachmentIds?: string[];
}

export interface ExpenseFormSplitDto {
  userId: string;
  userName: string;
  avatar?: string;
  amount?: number;
  percentage?: number;
  isSelected: boolean;
  description?: string;
}

// Statistics and Analytics DTOs
export interface ExpenseStatsDto {
  totalExpenses: number;
  myTotalPaid: number;
  myTotalOwed: number;
  myBalance: number;
  expenseCount: number;
  categoryBreakdown: CategoryStatsDto[];
  monthlyTrend: MonthlyStatsDto[];
}

export interface CategoryStatsDto {
  category: ExpenseCategory;
  categoryName: string;
  amount: number;
  count: number;
  percentage: number;
  color: string;
}

export interface MonthlyStatsDto {
  year: number;
  month: number;
  monthName: string;
  totalAmount: number;
  expenseCount: number;
  averagePerExpense: number;
}

// Balance DTOs
export interface UserBalanceDto {
  userId: string;
  userName: string;
  totalPaid: number;
  totalOwed: number;
  balance: number;
  debts: UserDebtDto[];
}

export interface UserDebtDto {
  toUserId: string;
  toUserName: string;
  amount: number;
}

export interface BalanceSummaryDto {
  userId: string;
  userName: string;
  balance: number;
  details: BalanceDetailDto[];
}

export interface BalanceDetailDto {
  otherUserId: string;
  otherUserName: string;
  amount: number;
  type: "owes" | "lent";
}

// Action DTOs
export interface ExpenseActionDto {
  expenseId: string;
  action: "duplicate" | "settle" | "remind";
  actionData?: any;
}

// Split Member DTO
export interface SplitMemberDto {
  userId: string;
  userName: string;
  amount?: number;
  percentage?: number;
}

// Category mapping for UI
export const CategoryColors: Record<ExpenseCategory, string> = {
  [ExpenseCategory.Food]: "#FF6B6B",
  [ExpenseCategory.Transport]: "#4ECDC4",
  [ExpenseCategory.Accommodation]: "#45B7D1",
  [ExpenseCategory.Entertainment]: "#96CEB4",
  [ExpenseCategory.Shopping]: "#FFEAA7",
  [ExpenseCategory.Other]: "#DDA0DD",
};

export const CategoryIcons: Record<ExpenseCategory, string> = {
  [ExpenseCategory.Food]: "🍽️",
  [ExpenseCategory.Transport]: "🚗",
  [ExpenseCategory.Accommodation]: "🏠",
  [ExpenseCategory.Entertainment]: "🎬",
  [ExpenseCategory.Shopping]: "🛍️",
  [ExpenseCategory.Other]: "📦",
};

// Utility functions
export const getCategoryDisplay = (category?: ExpenseCategory) => {
  if (!category)
    return {
      name: "Other",
      color: CategoryColors[ExpenseCategory.Other],
      icon: CategoryIcons[ExpenseCategory.Other],
    };
  return {
    name: category,
    color: CategoryColors[category],
    icon: CategoryIcons[category],
  };
};

export const formatCurrency = (amount: number, currency = "VND") => {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: currency === "VND" ? "VND" : "USD",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount);
};

export const calculateSplitAmounts = (
  totalAmount: number,
  splits: ExpenseFormSplitDto[],
  splitType: SplitType
): ExpenseFormSplitDto[] => {
  const selectedSplits = splits.filter((s) => s.isSelected);

  if (splitType === SplitType.Equal) {
    const amountPerPerson = totalAmount / selectedSplits.length;
    return splits.map((split) => ({
      ...split,
      amount: split.isSelected ? amountPerPerson : 0,
      percentage: split.isSelected ? 100 / selectedSplits.length : 0,
    }));
  }

  if (splitType === SplitType.Percentage) {
    return splits.map((split) => ({
      ...split,
      amount:
        split.isSelected && split.percentage
          ? (totalAmount * split.percentage) / 100
          : 0,
    }));
  }

  return splits; // For Amount type, amounts are manually set
};
