"use client";

import { useState } from "react";
import Link from "next/link";
import { Plus, Users, Calendar, Search, Settings, LogOut } from "lucide-react";
import { useMyGroups, useLeaveGroup } from "@/src/hooks/useGroups";
import { CreateGroupModal } from "@/src/components/groups/CreateGroupModal";
import { JoinGroupModal } from "@/src/components/groups/JoinGroupModal";
import { Loading } from "@/components/ui/Loading";
import { GroupListDto } from "@/src/entities/group/group.dto";
import { formatCurrency, formatDate } from "@/src/lib/utils";

export default function GroupsPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useMyGroups(page, 12);
  const leaveGroup = useLeaveGroup();

  const groups = data?.items || [];
  const filteredGroups = groups.filter((group) =>
    group.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-500 mb-4">Không thể tải danh sách nhóm</p>
          <button
            onClick={() => window.location.reload()}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Thử lại
          </button>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return <Loading fullScreen />;
  }

  return (
    <div className="min-h-screen">
      <div className="container-fluid mx-auto px-4 py-8 max-w-7xl">
        {/* Header */}
        <div className="rounded-lg shadow-sm mb-6">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="text-3xl font-bold text-foreground">
                Nhóm của tôi
              </h1>
              <p className="text-foreground mt-1">
                Quản lý chi tiêu chung với bạn bè
              </p>
            </div>
            <div className="flex gap-3">
              <button
                onClick={() => setShowJoinModal(true)}
                className="px-4 py-2 border border-blue-600 text-blue-600 rounded-lg hover:bg-blue-50 transition-colors"
              >
                Tham gia nhóm
              </button>
              <button
                onClick={() => setShowCreateModal(true)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
              >
                <Plus className="w-5 h-5" />
                Tạo nhóm
              </button>
            </div>
          </div>

          {/* Search bar */}
          <div className="mt-6">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-foreground w-5 h-5" />
              <input
                type="text"
                placeholder="Tìm kiếm nhóm..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-3 border border-border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
            </div>
          </div>
        </div>

        {/* Groups Grid */}
        {filteredGroups.length === 0 ? (
          <div className="rounded-lg shadow-sm p-12">
            <div className="text-center">
              <Users className="w-16 h-16 mx-auto text-foreground mb-4" />
              <h3 className="text-lg font-medium text-foreground mb-2">
                {searchTerm
                  ? "Không tìm thấy nhóm nào"
                  : "Bạn chưa tham gia nhóm nào"}
              </h3>
              <p className="text-foreground mb-6">
                {searchTerm
                  ? "Thử tìm kiếm với từ khóa khác"
                  : "Tạo nhóm mới hoặc tham gia nhóm có sẵn"}
              </p>
              {!searchTerm && (
                <div className="flex gap-3 justify-center">
                  <button
                    onClick={() => setShowJoinModal(true)}
                    className="px-6 py-2 border border-border rounded-lg hover:bg-background"
                  >
                    Tham gia nhóm
                  </button>
                  <button
                    onClick={() => setShowCreateModal(true)}
                    className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                  >
                    Tạo nhóm đầu tiên
                  </button>
                </div>
              )}
            </div>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {filteredGroups.map((group) => (
                <GroupCard
                  key={group.id}
                  group={group}
                  onLeave={leaveGroup.mutate}
                />
              ))}
            </div>

            {/* Pagination */}
            {data && data.totalCount > 12 && (
              <div className="mt-8 flex justify-center">
                <nav className="flex items-center gap-2">
                  <button
                    onClick={() => setPage((p) => Math.max(1, p - 1))}
                    disabled={!data.hasPrevious}
                    className="px-3 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-background"
                  >
                    Trước
                  </button>
                  <span className="px-4 py-2">
                    Trang {data.currentPage} / {data.totalPages}
                  </span>
                  <button
                    onClick={() => setPage((p) => p + 1)}
                    disabled={!data.hasNext}
                    className="px-3 py-2 border rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-background"
                  >
                    Sau
                  </button>
                </nav>
              </div>
            )}
          </>
        )}

        <CreateGroupModal
          isOpen={showCreateModal}
          onClose={() => setShowCreateModal(false)}
        />
        <JoinGroupModal
          isOpen={showJoinModal}
          onClose={() => setShowJoinModal(false)}
        />
      </div>
    </div>
  );
}

// Group Card Component
function GroupCard({
  group,
  onLeave,
}: {
  group: GroupListDto;
  onLeave: (id: string) => void;
}) {
  const [showMenu, setShowMenu] = useState(false);

  return (
    <div className="rounded-lg shadow-sm hover:shadow-md transition-shadow">
      <Link href={`/groups/${group.id}`}>
        <div className="p-6">
          <div className="flex items-start justify-between mb-4">
            <h3 className="text-xl font-semibold text-foreground">
              {group.name}
            </h3>
            {group.isAdmin && (
              <span className="px-2 py-1 text-xs bg-blue-100 text-blue-700 rounded-full">
                Admin
              </span>
            )}
          </div>

          <div className="space-y-3">
            <div className="flex items-center justify-between text-sm">
              <div className="flex items-center gap-2 text-foreground">
                <Users className="w-4 h-4" />
                <span>{group.memberCount} thành viên</span>
              </div>
              <span
                className={`font-medium ${group.userBalance >= 0 ? "text-green-600" : "text-red-600"}`}
              >
                {formatCurrency(Math.abs(group.userBalance))}
                {group.userBalance >= 0 ? " (nhận)" : " (nợ)"}
              </span>
            </div>

            <div className="flex items-center gap-2 text-sm text-foreground">
              <Calendar className="w-4 h-4" />
              <span>Hoạt động: {formatDate(group.lastActivity)}</span>
            </div>
          </div>
        </div>
      </Link>

      <div className="border-t px-6 py-3">
        <div className="flex justify-between items-center">
          <Link
            href={`/groups/${group.id}`}
            className="text-sm text-blue-600 hover:text-blue-700"
          >
            Xem chi tiết
          </Link>
          <div className="relative">
            <button
              onClick={(e) => {
                e.preventDefault();
                setShowMenu(!showMenu);
              }}
              className="p-1 hover:bg-muted rounded"
            >
              <Settings className="w-4 h-4 text-foreground" />
            </button>

            {showMenu && (
              <>
                <div
                  className="fixed inset-0 z-10"
                  onClick={() => setShowMenu(false)}
                />
                <div className="absolute right-0 bottom-full mb-1 w-48 rounded-lg shadow-lg border py-1 z-20">
                  {group.isAdmin && (
                    <Link
                      href={`/groups/${group.id}/settings`}
                      className="block px-4 py-2 text-sm hover:bg-background"
                    >
                      Cài đặt nhóm
                    </Link>
                  )}
                  <button
                    onClick={() => {
                      if (confirm("Bạn có chắc muốn rời khỏi nhóm này?")) {
                        onLeave(group.id);
                      }
                      setShowMenu(false);
                    }}
                    className="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                  >
                    <LogOut className="w-4 h-4 inline mr-2" />
                    Rời nhóm
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
