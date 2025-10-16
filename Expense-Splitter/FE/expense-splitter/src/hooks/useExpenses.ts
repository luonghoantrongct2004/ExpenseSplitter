/* eslint-disable @typescript-eslint/no-explicit-any */
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "react-hot-toast";
import { expenseService } from "@/src/lib/services/expenseService";
import {
  CreateExpenseDto,
  UpdateExpenseDto,
  ExpenseDto,
  ExpenseFilterDto,
  ExpenseListItemDto,
} from "@/src/entities/expense/expense.dto";

// Query keys
const expenseKeys = {
  all: ["expenses"] as const,
  lists: () => [...expenseKeys.all, "list"] as const,
  list: (groupId: string, filter?: ExpenseFilterDto) =>
    [...expenseKeys.lists(), groupId, filter] as const,
  details: () => [...expenseKeys.all, "detail"] as const,
  detail: (expenseId: string) => [...expenseKeys.details(), expenseId] as const,
  balances: (groupId: string) => ["balances", groupId] as const,
  myBalance: (groupId: string) => ["myBalance", groupId] as const,
};

// Hook to create expense
export function useCreateExpense(groupId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateExpenseDto) =>
      expenseService.createExpense(groupId, data),
    onSuccess: () => {
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: expenseKeys.list(groupId) });
      queryClient.invalidateQueries({
        queryKey: expenseKeys.balances(groupId),
      });
      queryClient.invalidateQueries({
        queryKey: expenseKeys.myBalance(groupId),
      });
      queryClient.invalidateQueries({ queryKey: ["groups"] }); // Update group balances

      toast.success("Chi tiêu đã được tạo thành công");
    },
    onError: (error: any) => {
      console.error("Error creating expense:", error);
      toast.error(error.response?.data?.message || "Không thể tạo chi tiêu");
    },
  });
}

// Hook to update expense
export function useUpdateExpense() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      expenseId,
      data,
    }: {
      expenseId: string;
      data: UpdateExpenseDto;
    }) => expenseService.updateExpense(expenseId, data),
    onSuccess: (data) => {
      // Invalidate related queries
      queryClient.invalidateQueries({ queryKey: expenseKeys.all });
      queryClient.invalidateQueries({
        queryKey: expenseKeys.balances(data.groupId),
      });
      queryClient.invalidateQueries({
        queryKey: expenseKeys.myBalance(data.groupId),
      });
      queryClient.invalidateQueries({ queryKey: ["groups"] });

      toast.success("Chi tiêu đã được cập nhật");
    },
    onError: (error: any) => {
      console.error("Error updating expense:", error);
      toast.error(
        error.response?.data?.message || "Không thể cập nhật chi tiêu"
      );
    },
  });
}

// Hook to delete expense
export function useDeleteExpense() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (expenseId: string) => expenseService.deleteExpense(expenseId),
    onSuccess: () => {
      // Invalidate all expense related queries
      queryClient.invalidateQueries({ queryKey: expenseKeys.all });
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      queryClient.invalidateQueries({ queryKey: ["balances"] });

      toast.success("Chi tiêu đã được xóa");
    },
    onError: (error: any) => {
      console.error("Error deleting expense:", error);
      toast.error(error.response?.data?.message || "Không thể xóa chi tiêu");
    },
  });
}

// Hook to get expenses for a group
export function useGroupExpenses(groupId: string, filter?: ExpenseFilterDto) {
  return useQuery({
    queryKey: expenseKeys.list(groupId, filter),
    queryFn: () => expenseService.getGroupExpenses(groupId, filter),
    enabled: !!groupId,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

// Hook to get light expenses for list view
export function useGroupExpensesLight(
  groupId: string,
  filter?: ExpenseFilterDto
) {
  return useQuery({
    queryKey: ["expensesLight", groupId, filter],
    queryFn: () => expenseService.getGroupExpensesLight(groupId, filter),
    enabled: !!groupId,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

// Hook to get expense by ID
export function useExpenseById(expenseId: string) {
  return useQuery({
    queryKey: expenseKeys.detail(expenseId),
    queryFn: () => expenseService.getExpenseById(expenseId),
    enabled: !!expenseId,
    staleTime: 1000 * 60 * 5, // 5 minutes
  });
}

// Hook to get my balance in a group
export function useMyBalance(groupId: string) {
  return useQuery({
    queryKey: expenseKeys.myBalance(groupId),
    queryFn: () => expenseService.getMyBalance(groupId),
    enabled: !!groupId,
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}

// Hook to get all balances in a group
export function useGroupBalances(groupId: string) {
  return useQuery({
    queryKey: expenseKeys.balances(groupId),
    queryFn: () => expenseService.getGroupBalances(groupId),
    enabled: !!groupId,
    staleTime: 1000 * 60 * 2, // 2 minutes
  });
}
