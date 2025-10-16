"use client";

import { useState } from "react";
import { X } from "lucide-react";
import { Loading } from "@/components/ui/Loading";
import { useCreateGroup } from "@/src/hooks/useGroups";

interface CreateGroupModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function CreateGroupModal({ isOpen, onClose }: CreateGroupModalProps) {
  const [formData, setFormData] = useState({
    name: "",
    description: "",
    currency: "VND",
  });

  const createGroup = useCreateGroup();

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await createGroup.mutateAsync(formData);
      setFormData({ name: "", description: "", currency: "VND" });
      onClose();
    } catch (error) {
      // Error handled by mutation hook
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-card rounded-xl p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold">Tạo nhóm mới</h2>
          <button
            onClick={onClose}
            className="p-2 hover:bg-muted rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Tên nhóm <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              required
              value={formData.name}
              onChange={(e) =>
                setFormData({ ...formData, name: e.target.value })
              }
              className="w-full px-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="Ví dụ: Chuyến du lịch Đà Lạt"
              disabled={createGroup.isPending}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Mô tả
            </label>
            <textarea
              value={formData.description}
              onChange={(e) =>
                setFormData({ ...formData, description: e.target.value })
              }
              className="w-full px-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
              rows={3}
              placeholder="Mô tả về nhóm..."
              disabled={createGroup.isPending}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Đơn vị tiền tệ
            </label>
            <select
              value={formData.currency}
              onChange={(e) =>
                setFormData({ ...formData, currency: e.target.value })
              }
              className="w-full px-4 py-2 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              disabled={createGroup.isPending}
            >
              <option value="VND">VND - Việt Nam Đồng</option>
              <option value="USD">USD - US Dollar</option>
              <option value="EUR">EUR - Euro</option>
            </select>
          </div>

          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2.5 border border-border text-foreground rounded-lg hover:bg-background transition-colors font-medium"
              disabled={createGroup.isPending}
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={createGroup.isPending || !formData.name.trim()}
              className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium flex items-center justify-center gap-2"
            >
              {createGroup.isPending ? (
                <>
                  <Loading />
                  <span>Đang tạo...</span>
                </>
              ) : (
                "Tạo nhóm"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
