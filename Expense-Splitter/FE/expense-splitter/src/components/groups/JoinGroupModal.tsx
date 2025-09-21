"use client";

import { useState } from "react";
import { X, Users } from "lucide-react";
import { Loading } from "@/components/ui/Loading";
import { useJoinGroup } from "@/src/hooks/useGroups";

interface JoinGroupModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function JoinGroupModal({ isOpen, onClose }: JoinGroupModalProps) {
  const [inviteCode, setInviteCode] = useState("");
  const joinGroup = useJoinGroup();

  if (!isOpen) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      await joinGroup.mutateAsync(inviteCode.trim());
      setInviteCode("");
      onClose();
    } catch (error) {
      // Error handled by mutation
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-card rounded-xl p-6 w-full max-w-md">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-2xl font-bold">Tham gia nhóm</h2>
          <button
            onClick={onClose}
            className="p-2 hover:bg-muted rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="text-center mb-6">
            <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <Users className="w-8 h-8 text-blue-600" />
            </div>
            <p className="text-foreground">
              Nhập mã mời để tham gia nhóm có sẵn
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-foreground mb-2">
              Mã mời nhóm
            </label>
            <input
              type="text"
              required
              value={inviteCode}
              onChange={(e) => setInviteCode(e.target.value.toUpperCase())}
              className="w-full px-4 py-3 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent text-center text-lg font-mono uppercase"
              placeholder="ABC123"
              disabled={joinGroup.isPending}
              maxLength={10}
            />
            <p className="text-sm text-foreground mt-2">
              Liên hệ với quản trị viên nhóm để nhận mã mời
            </p>
          </div>

          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 px-4 py-2.5 border border-border text-foreground rounded-lg hover:bg-background transition-colors font-medium"
              disabled={joinGroup.isPending}
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={joinGroup.isPending || !inviteCode.trim()}
              className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors font-medium flex items-center justify-center gap-2"
            >
              {joinGroup.isPending ? (
                <>
                  <Loading />
                </>
              ) : (
                "Tham gia"
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
