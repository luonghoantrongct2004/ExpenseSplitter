/* eslint-disable @typescript-eslint/no-explicit-any */
"use client";

import { useState, useEffect } from "react";
import { X, Calendar, DollarSign, Users, FileText } from "lucide-react";
import { Loading } from "@/components/ui/Loading";
import { useCreateExpense } from "@/src/hooks/useExpenses";
import { useGroupMembers } from "@/src/hooks/useGroups";
import { toast } from "react-hot-toast";
import {
  CreateExpenseDto,
  CreateExpenseSplitDto,
  ExpenseCategory,
  SplitType,
} from "@/src/entities/expense/expense.dto";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

interface CreateExpenseModalProps {
  isOpen: boolean;
  onClose: () => void;
  groupId: string;
}

export function CreateExpenseModal({
  isOpen,
  onClose,
  groupId,
}: CreateExpenseModalProps) {
  const [formData, setFormData] = useState({
    amount: "",
    description: "",
    note: "",
    expenseDate: new Date().toISOString().split("T")[0],
    category: ExpenseCategory.Other,
    paidById: "",
    splitType: SplitType.Equal,
    selectedMembers: [] as string[],
    customSplits: {} as Record<string, number>,
  });

  const createExpense = useCreateExpense(groupId);
  const { data: members, isLoading: membersLoading } = useGroupMembers(groupId);
  const currentUser = members?.find((m: any) => m.isCurrentUser);

  // Set default paidById to current user when members are loaded
  useEffect(() => {
    if (currentUser && !formData.paidById) {
      setFormData((prev) => ({ ...prev, paidById: currentUser.id }));
    }
  }, [currentUser]);

  // Initialize selected members when modal opens
  useEffect(() => {
    if (isOpen && members && formData.selectedMembers.length === 0) {
      // Select all members by default
      setFormData((prev) => ({
        ...prev,
        selectedMembers: members.map((m: any) => m.id),
      }));
    }
  }, [isOpen, members]);

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validate form
    if (
      !formData.description ||
      !formData.amount ||
      parseFloat(formData.amount) <= 0
    ) {
      toast.error("Vui lòng điền đầy đủ thông tin chi tiêu");
      return;
    }

    if (!formData.paidById) {
      toast.error("Vui lòng chọn người chi trả");
      return;
    }

    if (formData.selectedMembers.length === 0) {
      toast.error("Vui lòng chọn ít nhất một thành viên tham gia");
      return;
    }

    // Calculate splits based on selected members and split type
    const totalAmount = parseFloat(formData.amount);
    let splits: CreateExpenseSplitDto[] = [];

    if (formData.splitType === SplitType.Equal) {
      const amountPerPerson = totalAmount / formData.selectedMembers.length;
      splits = formData.selectedMembers.map((userId) => ({
        userId,
        amount: amountPerPerson,
      }));
    } else if (formData.splitType === SplitType.Amount) {
      // For custom amounts, use the customSplits object
      splits = formData.selectedMembers.map((userId) => ({
        userId,
        amount: formData.customSplits[userId] || 0,
      }));

      // Validate that total splits equal the total amount
      const totalSplits = splits.reduce((sum, split) => sum + split.amount, 0);
      if (Math.abs(totalAmount - totalSplits) > 0.01) {
        toast.error("Tổng chia không bằng tổng chi tiêu");
        return;
      }
    } else if (formData.splitType === SplitType.Percentage) {
      // For percentage splits, use the customSplits object as percentages
      splits = formData.selectedMembers.map((userId) => {
        const percentage = formData.customSplits[userId] || 0;
        return {
          userId,
          amount: (totalAmount * percentage) / 100,
        };
      });

      // Validate that total percentages equal 100%
      const totalPercentage = Object.values(formData.customSplits).reduce(
        (sum, percentage) => sum + percentage,
        0
      );
      if (Math.abs(100 - totalPercentage) > 0.01) {
        toast.error("Tổng phần trăm phải bằng 100%");
        return;
      }
    }

    // Format the date properly
    const expenseDate = new Date(formData.expenseDate);
    // Ensure the date is valid
    if (isNaN(expenseDate.getTime())) {
      toast.error("Ngày chi tiêu không hợp lệ");
      return;
    }

    // Create the expense data object
    const expenseData: CreateExpenseDto = {
      amount: totalAmount,
      description: formData.description,
      note: formData.note,
      category: formData.category, // This is now a numeric value
      paidById: formData.paidById,
      expenseDate: expenseDate.toISOString(),
      splits,
    };

    // Wrap the data in a 'dto' field as expected by the backend
    const requestData = {
      dto: expenseData,
    };

    try {
      await createExpense.mutateAsync(requestData.dto);

      // Reset form
      setFormData({
        description: "",
        amount: "",
        expenseDate: new Date().toISOString().split("T")[0],
        note: "",
        category: ExpenseCategory.Other, // This is now 5
        paidById: currentUser?.id || "",
        splitType: SplitType.Equal,
        selectedMembers: members?.map((m: any) => m.id) || [],
        customSplits: {},
      });

      onClose();
    } catch (error) {
      // Error is handled by the mutation hook
      console.error("Error creating expense:", error);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-card rounded-xl p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold flex items-center gap-2">
            <DollarSign className="w-6 h-6" />
            Tạo chi tiêu mới
          </h2>
          <button
            onClick={onClose}
            className="p-2 hover:bg-muted rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          {/* Description */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Mô tả chi tiêu <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.description}
              onChange={(e) =>
                setFormData({ ...formData, description: e.target.value })
              }
              className="w-full px-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Ví dụ: Ăn tối, Xăng xe, Vé vào cổng..."
              disabled={createExpense.isPending}
            />
          </div>

          {/* Amount and Date */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-2">
                Số tiền <span className="text-red-500">*</span>
              </label>
              <div className="relative">
                <DollarSign className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                  type="number"
                  required
                  value={formData.amount}
                  onChange={(e) =>
                    setFormData({ ...formData, amount: e.target.value })
                  }
                  className="w-full pl-10 pr-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  placeholder="0"
                  min="0"
                  step="1000"
                  disabled={createExpense.isPending}
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-2">
                Ngày chi tiêu
              </label>
              <div className="relative">
                <Calendar className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <input
                  type="date"
                  value={formData.expenseDate}
                  onChange={(e) =>
                    setFormData({ ...formData, expenseDate: e.target.value })
                  }
                  className="w-full pl-10 pr-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  disabled={createExpense.isPending}
                />
              </div>
            </div>
          </div>

          {/* Note */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Ghi chú
            </label>
            <div className="relative">
              <FileText className="absolute left-3 top-3 w-4 h-4 text-muted-foreground" />
              <textarea
                value={formData.note}
                onChange={(e) =>
                  setFormData({ ...formData, note: e.target.value })
                }
                className="w-full pl-10 pr-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
                rows={3}
                placeholder="Thêm ghi chú về chi tiêu này..."
                disabled={createExpense.isPending}
              />
            </div>
          </div>

          {/* Category and Payer */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-foreground mb-2">
                Danh mục
              </label>
              <select
                value={formData.category}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    category: parseInt(e.target.value) as ExpenseCategory,
                  })
                }
                className="w-full px-4 py-2 border border-border bg-background text-foreground rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={createExpense.isPending}
              >
                <option value={ExpenseCategory.Food}>🍽️ Ăn uống</option>
                <option value={ExpenseCategory.Transport}>🚗 Di chuyển</option>
                <option value={ExpenseCategory.Accommodation}>🏠 Chỗ ở</option>
                <option value={ExpenseCategory.Entertainment}>
                  🎬 Giải trí
                </option>
                <option value={ExpenseCategory.Shopping}>🛍️ Mua sắm</option>
                <option value={ExpenseCategory.Other}>📦 Khác</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-foreground mb-2">
                Người chi trả <span className="text-red-500">*</span>
              </label>
              <select
                value={formData.paidById}
                onChange={(e) =>
                  setFormData({ ...formData, paidById: e.target.value })
                }
                className="w-full px-4 py-2 border border-border bg-background text-foreground rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                disabled={createExpense.isPending || membersLoading}
              >
                <option value="">Chọn người chi trả...</option>
                {members?.map((member: any) => (
                  <option key={member.id} value={member.id}>
                    {member.name}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Split Type */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Cách chia chi phí
            </label>
            <div className="grid grid-cols-3 gap-3">
              <button
                type="button"
                onClick={() =>
                  setFormData({ ...formData, splitType: SplitType.Equal })
                }
                className={`px-4 py-2 rounded-lg border transition-colors ${
                  formData.splitType === SplitType.Equal
                    ? "bg-primary text-primary-foreground border-primary"
                    : "border-border bg-background text-foreground hover:bg-muted"
                }`}
                disabled={createExpense.isPending}
              >
                <Users
                  className={`w-4 h-4 mx-auto mb-1 ${
                    formData.splitType === SplitType.Equal
                      ? "text-primary-foreground"
                      : "text-foreground"
                  }`}
                />
                <span
                  className={`text-sm ${
                    formData.splitType === SplitType.Equal
                      ? "text-primary-foreground"
                      : "text-foreground"
                  }`}
                >
                  Chia đều
                </span>
              </button>
              <button
                type="button"
                onClick={() =>
                  setFormData({ ...formData, splitType: SplitType.Amount })
                }
                className={`px-4 py-2 rounded-lg border transition-colors ${
                  formData.splitType === SplitType.Amount
                    ? "bg-primary text-primary-foreground border-primary"
                    : "border-border bg-background text-foreground hover:bg-muted"
                }`}
                disabled={createExpense.isPending}
              >
                <DollarSign
                  className={`w-4 h-4 mx-auto mb-1 ${
                    formData.splitType === SplitType.Amount
                      ? "text-primary-foreground"
                      : "text-foreground"
                  }`}
                />
                <span
                  className={`text-sm ${
                    formData.splitType === SplitType.Amount
                      ? "text-primary-foreground"
                      : "text-foreground"
                  }`}
                >
                  Tùy chỉnh
                </span>
              </button>
              <button
                type="button"
                onClick={() =>
                  setFormData({ ...formData, splitType: SplitType.Percentage })
                }
                className={`px-4 py-2 rounded-lg border transition-colors ${
                  formData.splitType === SplitType.Percentage
                    ? "bg-primary text-primary-foreground border-primary"
                    : "border-border bg-background text-foreground hover:bg-muted"
                }`}
                disabled={createExpense.isPending}
              >
                <span
                  className={`text-lg mx-auto mb-1 ${
                    formData.splitType === SplitType.Percentage
                      ? "text-primary-foreground"
                      : "text-foreground"
                  }`}
                >
                  %
                </span>
                <span
                  className={`text-sm ${
                    formData.splitType === SplitType.Percentage
                      ? "text-primary-foreground"
                      : "text-foreground"
                  }`}
                >
                  Phần trăm
                </span>
              </button>
            </div>
          </div>

          {/* Member Selection */}
          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Chia cho ai? <span className="text-red-500">*</span>
            </label>
            <div className="space-y-2 max-h-40 overflow-y-auto border border-border rounded-lg p-3">
              {membersLoading ? (
                <div className="text-center py-2">Chờ xíu...</div>
              ) : members && members.length > 0 ? (
                members.map((member: any) => (
                  <div
                    key={member.id}
                    className="flex items-center justify-between"
                  >
                    <label className="flex items-center space-x-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={formData.selectedMembers.includes(member.id)}
                        onChange={(e) => {
                          if (e.target.checked) {
                            setFormData((prev) => ({
                              ...prev,
                              selectedMembers: [
                                ...prev.selectedMembers,
                                member.id,
                              ],
                            }));
                          } else {
                            setFormData((prev) => ({
                              ...prev,
                              selectedMembers: prev.selectedMembers.filter(
                                (id) => id !== member.id
                              ),
                            }));
                          }
                        }}
                        className="rounded"
                        disabled={createExpense.isPending}
                      />
                      <Avatar className="h-6 w-6">
                        <AvatarImage src={member.avatar} />
                        <AvatarFallback className="text-xs">
                          {member.name?.substring(0, 2).toUpperCase()}
                        </AvatarFallback>
                      </Avatar>
                      <span className="text-sm">{member.name}</span>
                    </label>
                    {/* In the member selection section, update the conditional rendering */}
                    {formData.splitType === SplitType.Amount &&
                      formData.selectedMembers.includes(member.id) && (
                        <input
                          type="number"
                          value={formData.customSplits[member.id] || ""}
                          onChange={(e) => {
                            setFormData((prev) => ({
                              ...prev,
                              customSplits: {
                                ...prev.customSplits,
                                [member.id]: parseFloat(e.target.value) || 0,
                              },
                            }));
                          }}
                          placeholder="0"
                          className="w-24 px-2 py-1 text-sm border border-border rounded"
                          disabled={createExpense.isPending}
                        />
                      )}
                    {formData.splitType === SplitType.Percentage &&
                      formData.selectedMembers.includes(member.id) && (
                        <div className="flex items-center">
                          <input
                            type="number"
                            value={formData.customSplits[member.id] || ""}
                            onChange={(e) => {
                              setFormData((prev) => ({
                                ...prev,
                                customSplits: {
                                  ...prev.customSplits,
                                  [member.id]: parseFloat(e.target.value) || 0,
                                },
                              }));
                            }}
                            placeholder="0"
                            className="w-20 px-2 py-1 text-sm border border-border rounded"
                            disabled={createExpense.isPending}
                          />
                          <span className="ml-1 text-sm">%</span>
                        </div>
                      )}
                  </div>
                ))
              ) : (
                <div className="text-center py-2 text-muted-foreground">
                  Không có thành viên trong nhóm
                </div>
              )}
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2.5 border border-border text-foreground rounded-lg hover:bg-background transition-colors font-medium"
              disabled={createExpense.isPending}
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={
                createExpense.isPending ||
                !formData.description.trim() ||
                !formData.amount
              }
              className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium flex items-center justify-center gap-2"
            >
              {createExpense.isPending ? (
                <>
                  <Loading />
                </>
              ) : (
                "Tạo chi tiêu"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
