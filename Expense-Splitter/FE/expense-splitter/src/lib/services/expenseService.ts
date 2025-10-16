/* eslint-disable @typescript-eslint/no-explicit-any */
import { ApiResponse, PagedList } from "@/src/entities/dto";
import axios from "../axios";
import {
  CreateExpenseDto,
  UpdateExpenseDto,
  ExpenseDto,
  ExpenseFilterDto,
  ExpenseSearchDto,
  ExpenseListItemDto,
  ExpenseStatsDto,
  UserBalanceDto,
  BalanceSummaryDto,
  ExpenseActionDto,
  CategoryStatsDto,
  MonthlyStatsDto,
} from "@/src/entities/expense/expense.dto";

export const expenseService = {
  //NOTE - Tạo chi tiêu mới
  async createExpense(
    groupId: string,
    request: CreateExpenseDto
  ): Promise<ExpenseDto> {
    try {
      const response = await axios.post<ApiResponse<ExpenseDto>>(
        `/api/expenses/groups/${groupId}`,
        request
      );
      if (!response.data.success) {
        throw new Error(response.data.message || "Failed to create expense");
      }

      if (!response.data.data) {
        throw new Error("No data returned from server");
      }
      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy danh sách chi tiêu của nhóm
  async getGroupExpenses(
    groupId: string,
    filter?: ExpenseFilterDto
  ): Promise<PagedList<ExpenseDto>> {
    try {
      const params: any = {
        pageNumber: filter?.pageNumber || 1,
        pageSize: filter?.pageSize || 10,
      };

      if (filter?.startDate) params.startDate = filter.startDate;
      if (filter?.endDate) params.endDate = filter.endDate;
      if (filter?.category) params.category = filter.category;
      if (filter?.paidById) params.paidById = filter.paidById;
      if (filter?.participantId) params.participantId = filter.participantId;
      if (filter?.searchTerm) params.searchTerm = filter.searchTerm;
      if (filter?.includeDeleted !== undefined)
        params.includeDeleted = filter.includeDeleted;

      const response = await axios.get<ApiResponse<PagedList<ExpenseDto>>>(
        `/api/expenses/groups/${groupId}`,
        { params }
      );

      if (!response.data.success) {
        throw new Error(response.data.message || "Failed to fetch expenses");
      }

      if (!response.data.data) {
        return {
          items: [],
          currentPage: filter?.pageNumber || 1,
          totalPages: 0,
          pageSize: filter?.pageSize || 10,
          totalCount: 0,
          hasPrevious: false,
          hasNext: false,
        };
      }

      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy danh sách chi tiêu nhẹ cho list view
  async getGroupExpensesLight(
    groupId: string,
    filter?: ExpenseFilterDto
  ): Promise<PagedList<ExpenseListItemDto>> {
    try {
      const params: any = {
        pageNumber: filter?.pageNumber || 1,
        pageSize: filter?.pageSize || 10,
      };

      if (filter?.startDate) params.startDate = filter.startDate;
      if (filter?.endDate) params.endDate = filter.endDate;
      if (filter?.category) params.category = filter.category;
      if (filter?.paidById) params.paidById = filter.paidById;
      if (filter?.participantId) params.participantId = filter.participantId;
      if (filter?.searchTerm) params.searchTerm = filter.searchTerm;

      const response = await axios.get<
        ApiResponse<PagedList<ExpenseListItemDto>>
      >(`/api/expenses/groups/${groupId}/list`, { params });

      if (!response.data.success) {
        throw new Error(response.data.message || "Failed to fetch expenses");
      }

      if (!response.data.data) {
        return {
          items: [],
          currentPage: filter?.pageNumber || 1,
          totalPages: 0,
          pageSize: filter?.pageSize || 10,
          totalCount: 0,
          hasPrevious: false,
          hasNext: false,
        };
      }

      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy chi tiêu theo ID
  async getExpenseById(expenseId: string): Promise<ExpenseDto> {
    try {
      const response = await axios.get<ApiResponse<ExpenseDto>>(
        `/api/expenses/${expenseId}`
      );
      if (!response.data.success) {
        throw new Error(response.data.message);
      }

      if (!response.data.data) {
        throw new Error("Expense not found");
      }
      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Cập nhật chi tiêu
  async updateExpense(
    expenseId: string,
    request: UpdateExpenseDto
  ): Promise<ExpenseDto> {
    try {
      const response = await axios.put<ApiResponse<ExpenseDto>>(
        `/api/expenses/${expenseId}`,
        request
      );
      if (!response.data.success) {
        throw new Error(response.data.message);
      }

      if (!response.data.data) {
        throw new Error("Failed to update expense");
      }
      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Xóa chi tiêu
  async deleteExpense(expenseId: string): Promise<void> {
    try {
      const response = await axios.delete<ApiResponse<void>>(
        `/api/expenses/${expenseId}`
      );
      if (!response.data.success) {
        throw new Error(response.data.message);
      }
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy số dư của người dùng trong nhóm
  async getMyBalance(groupId: string): Promise<UserBalanceDto> {
    try {
      const response = await axios.get<ApiResponse<UserBalanceDto>>(
        `/api/expenses/groups/${groupId}/balances/me`
      );
      if (!response.data.success) {
        throw new Error(response.data.message);
      }

      if (!response.data.data) {
        throw new Error("Failed to get balance");
      }
      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy số dư của một thành viên
  async getUserBalance(
    groupId: string,
    memberId: string
  ): Promise<UserBalanceDto> {
    try {
      const response = await axios.get<ApiResponse<UserBalanceDto>>(
        `/api/expenses/groups/${groupId}/balances/${memberId}`
      );
      if (!response.data.success) {
        throw new Error(response.data.message);
      }

      if (!response.data.data) {
        throw new Error("Failed to get user balance");
      }
      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy bảng cân đối của tất cả thành viên trong nhóm
  async getGroupBalances(groupId: string): Promise<BalanceSummaryDto[]> {
    try {
      const response = await axios.get<ApiResponse<BalanceSummaryDto[]>>(
        `/api/expenses/groups/${groupId}/balances`
      );
      if (!response.data.success) {
        throw new Error(response.data.message);
      }

      if (!response.data.data) {
        return [];
      }
      return response.data.data;
    } catch (error: any) {
      throw error;
    }
  },

  // //NOTE - Lấy thống kê chi tiêu
  // async getExpenseStats(
  //   groupId: string,
  //   startDate?: string,
  //   endDate?: string
  // ): Promise<ExpenseStatsDto> {
  //   try {
  //     const params: any = {};
  //     if (startDate) params.startDate = startDate;
  //     if (endDate) params.endDate = endDate;

  //     const response = await axios.get<ApiResponse<ExpenseStatsDto>>(
  //       `/api/expenses/groups/${groupId}/stats`,
  //       { params }
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }

  //     if (!response.data.data) {
  //       throw new Error("Failed to get statistics");
  //     }
  //     return response.data.data;
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Lấy thống kê theo danh mục
  // async getCategoryStats(
  //   groupId: string,
  //   startDate?: string,
  //   endDate?: string
  // ): Promise<CategoryStatsDto[]> {
  //   try {
  //     const params: any = {};
  //     if (startDate) params.startDate = startDate;
  //     if (endDate) params.endDate = endDate;

  //     const response = await axios.get<ApiResponse<CategoryStatsDto[]>>(
  //       `/api/expenses/groups/${groupId}/categories`,
  //       { params }
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }

  //     return response.data.data || [];
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Lấy xu hướng chi tiêu theo tháng
  // async getMonthlyTrend(
  //   groupId: string,
  //   months: number = 6
  // ): Promise<MonthlyStatsDto[]> {
  //   try {
  //     const response = await axios.get<ApiResponse<MonthlyStatsDto[]>>(
  //       `/api/expenses/groups/${groupId}/monthly-trend`,
  //       { params: { months } }
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }

  //     return response.data.data || [];
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Tìm kiếm chi tiêu
  // async searchExpenses(
  //   searchDto: ExpenseSearchDto
  // ): Promise<PagedList<ExpenseDto>> {
  //   try {
  //     const response = await axios.post<ApiResponse<PagedList<ExpenseDto>>>(
  //       `/api/expenses/search`,
  //       searchDto
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }

  //     if (!response.data.data) {
  //       return {
  //         items: [],
  //         currentPage: searchDto.pageNumber || 1,
  //         totalPages: 0,
  //         pageSize: searchDto.pageSize || 10,
  //         totalCount: 0,
  //         hasPrevious: false,
  //         hasNext: false,
  //       };
  //     }

  //     return response.data.data;
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Nhân đôi chi tiêu
  // async duplicateExpense(expenseId: string): Promise<ExpenseDto> {
  //   try {
  //     const response = await axios.post<ApiResponse<ExpenseDto>>(
  //       `/api/expenses/${expenseId}/duplicate`
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }

  //     if (!response.data.data) {
  //       throw new Error("Failed to duplicate expense");
  //     }
  //     return response.data.data;
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Thanh toán chi tiêu
  // async settleExpense(expenseId: string, userId?: string): Promise<void> {
  //   try {
  //     const response = await axios.post<ApiResponse<void>>(
  //       `/api/expenses/${expenseId}/settle`,
  //       userId ? { userId } : undefined
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Gửi nhắc nhở thanh toán
  // async sendReminder(expenseId: string, userIds?: string[]): Promise<void> {
  //   try {
  //     const response = await axios.post<ApiResponse<void>>(
  //       `/api/expenses/${expenseId}/remind`,
  //       userIds ? { userIds } : undefined
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Thực hiện action trên chi tiêu
  // async performAction(action: ExpenseActionDto): Promise<any> {
  //   switch (action.action) {
  //     case "duplicate":
  //       return this.duplicateExpense(action.expenseId);
  //     case "settle":
  //       return this.settleExpense(action.expenseId, action.actionData?.userId);
  //     case "remind":
  //       return this.sendReminder(action.expenseId, action.actionData?.userIds);
  //     default:
  //       throw new Error(`Unknown action: ${action.action}`);
  //   }
  // },

  // //NOTE - Xuất chi tiêu
  // async exportExpenses(
  //   groupId: string,
  //   startDate?: string,
  //   endDate?: string,
  //   format: "csv" | "xlsx" = "csv"
  // ): Promise<Blob> {
  //   try {
  //     const params: any = { format };
  //     if (startDate) params.startDate = startDate;
  //     if (endDate) params.endDate = endDate;

  //     const response = await axios.get(
  //       `/api/expenses/groups/${groupId}/export`,
  //       {
  //         params,
  //         responseType: "blob",
  //       }
  //     );

  //     return response.data;
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Lấy chi tiêu gần đây
  // async getRecentExpenses(
  //   groupId: string,
  //   limit: number = 5
  // ): Promise<ExpenseListItemDto[]> {
  //   try {
  //     const response = await axios.get<ApiResponse<ExpenseListItemDto[]>>(
  //       `/api/expenses/groups/${groupId}/recent`,
  //       { params: { limit } }
  //     );
  //     if (!response.data.success) {
  //       throw new Error(response.data.message);
  //     }

  //     return response.data.data || [];
  //   } catch (error: any) {
  //     throw error;
  //   }
  // },

  // //NOTE - Validate splits
  // validateSplits(
  //   totalAmount: number,
  //   splits: { userId: string; amount: number }[]
  // ): boolean {
  //   const totalSplits = splits.reduce((sum, split) => sum + split.amount, 0);
  //   return Math.abs(totalAmount - totalSplits) < 0.01; // Allow for small rounding differences
  // },
};
