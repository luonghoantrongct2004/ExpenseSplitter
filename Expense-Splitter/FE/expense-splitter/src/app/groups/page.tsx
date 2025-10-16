"use client";

import { useState } from "react";
import {
  Plus,
  Users,
  Search,
  Settings,
  LogOut,
  Send,
  MoreVertical,
  UserPlus,
  Receipt,
  DollarSign,
  Paperclip,
  Smile,
  Archive,
} from "lucide-react";
import { useMyGroups, useLeaveGroup } from "@/src/hooks/useGroups";
import { CreateGroupModal } from "@/src/components/groups/CreateGroupModal";
import { JoinGroupModal } from "@/src/components/groups/JoinGroupModal";
import { CreateExpenseModal } from "@/src/components/groups/CreateExpenseModal";
import { Loading } from "@/components/ui/Loading";
import { GroupListDto } from "@/src/entities/group/group.dto";
import { formatCurrency, formatDate } from "@/src/lib/utils";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";

export default function GroupsPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showJoinModal, setShowJoinModal] = useState(false);
  const [showExpenseModal, setShowExpenseModal] = useState(false);
  const [selectedGroupId, setSelectedGroupId] = useState<string | null>(null);
  const [message, setMessage] = useState("");
  const [page, setPage] = useState(1);

  const { data, isLoading, error } = useMyGroups(page, 12);
  const leaveGroup = useLeaveGroup();

  const groups = data?.items || [];
  const filteredGroups = groups.filter((group) =>
    group.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  const selectedGroup = groups.find((g) => g.id === selectedGroupId);

  if (error) {
    return (
      <div className="h-[calc(100vh-8rem)] flex items-center justify-center">
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
    <div className="flex h-[calc(100vh-8rem)] antialiased">
      <div className="flex flex-row h-full w-full overflow-x-hidden">
        {/* Sidebar */}
        <div className="flex flex-col py-8 pl-6 pr-2 w-64 bg-background border-r flex-shrink-0">
          {/* Logo */}
          <div className="flex flex-row items-center justify-center h-12 w-full">
            <div className="flex items-center justify-center rounded-2xl text-primary bg-primary/10 h-10 w-10">
              <Users className="w-6 h-6" />
            </div>
            <div className="ml-2 font-bold text-2xl">Nhóm</div>
          </div>

          {/* Groups Section */}
          <div className="flex flex-col mt-8 flex-1 min-h-0">
            <div className="flex flex-row items-center justify-between text-xs">
              <span className="font-bold">Nhóm đang tham gia</span>
              <span className="flex items-center justify-center bg-muted h-4 w-4 rounded-full text-[10px]">
                {groups.length}
              </span>
            </div>

            {/* Search */}
            <div className="relative mt-4">
              <Search className="absolute left-2 top-1/2 transform -translate-y-1/2 text-muted-foreground w-4 h-4" />
              <input
                type="text"
                placeholder="Tìm kiếm..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-8 pr-2 py-1.5 text-sm bg-muted/30 rounded-lg focus:outline-none focus:ring-1 focus:ring-primary"
              />
            </div>

            {/* Group List */}
            <div className="flex flex-col space-y-1 mt-4 -mx-2 overflow-y-auto flex-1">
              {filteredGroups.map((group) => (
                <button
                  key={group.id}
                  onClick={() => setSelectedGroupId(group.id)}
                  className={cn(
                    "flex flex-row items-center hover:bg-muted/50 rounded-xl p-2",
                    selectedGroupId === group.id && "bg-muted"
                  )}
                >
                  <Avatar className="h-8 w-8">
                    <AvatarFallback
                      className={cn(
                        "text-xs",
                        selectedGroupId === group.id
                          ? "bg-primary text-primary-foreground"
                          : "bg-muted"
                      )}
                    >
                      {group.name.substring(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div className="ml-2 text-sm font-semibold truncate flex-1 text-left">
                    {group.name}
                  </div>
                  {group.userBalance !== 0 && (
                    <div
                      className={cn(
                        "flex items-center justify-center ml-auto text-xs text-white h-4 min-w-[1rem] px-1 rounded",
                        group.userBalance > 0 ? "bg-green-500" : "bg-red-500"
                      )}
                    >
                      {formatCurrency(Math.abs(group.userBalance), "compact")}
                    </div>
                  )}
                </button>
              ))}
            </div>

            {/* Action Buttons */}
            <div className="flex gap-2 mt-4">
              <Button
                size="sm"
                variant="outline"
                onClick={() => setShowJoinModal(true)}
                className="flex-1"
              >
                <UserPlus className="w-4 h-4 mr-1" />
                Tham gia
              </Button>
              <Button
                size="sm"
                onClick={() => setShowCreateModal(true)}
                className="flex-1"
              >
                <Plus className="w-4 h-4 mr-1" />
                Tạo mới
              </Button>
            </div>
          </div>
        </div>

        {/* Chat Area */}
        <div className="flex flex-col flex-auto h-full p-6">
          {selectedGroup ? (
            <div className="flex flex-col flex-auto flex-shrink-0 rounded-2xl bg-muted/20 h-full p-4">
              {/* Chat Header */}
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-3">
                  <Avatar>
                    <AvatarFallback className="bg-primary text-primary-foreground">
                      {selectedGroup.name.substring(0, 2).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                  <div>
                    <h3 className="font-semibold">{selectedGroup.name}</h3>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <span>{selectedGroup.memberCount} thành viên</span>
                      <span>•</span>
                      <span
                        className={cn(
                          "font-medium",
                          selectedGroup.userBalance >= 0
                            ? "text-green-600"
                            : "text-red-600"
                        )}
                      >
                        {formatCurrency(Math.abs(selectedGroup.userBalance))}
                        {selectedGroup.userBalance >= 0 ? " (nhận)" : " (nợ)"}
                      </span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Button
                    size="icon"
                    variant="ghost"
                    onClick={() => setShowExpenseModal(true)}
                    title="Tạo chi tiêu mới"
                  >
                    <Receipt className="h-5 w-5" />
                  </Button>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button size="icon" variant="ghost">
                        <MoreVertical className="h-5 w-5" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem>
                        <UserPlus className="mr-2 h-4 w-4" />
                        Thêm thành viên
                      </DropdownMenuItem>
                      {selectedGroup.isAdmin && (
                        <DropdownMenuItem>
                          <Settings className="mr-2 h-4 w-4" />
                          Cài đặt nhóm
                        </DropdownMenuItem>
                      )}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem
                        className="text-red-600"
                        onClick={() => {
                          if (confirm("Bạn có chắc muốn rời khỏi nhóm này?")) {
                            leaveGroup.mutate(selectedGroup.id);
                            setSelectedGroupId(null);
                          }
                        }}
                      >
                        <LogOut className="mr-2 h-4 w-4" />
                        Rời nhóm
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </div>

              {/* Messages Container */}
              <div className="flex flex-col h-full overflow-x-auto mb-4 min-h-0">
                <div className="flex flex-col h-full overflow-y-auto">
                  <div className="grid grid-cols-12 gap-y-2">
                    {/* Message from other */}
                    <div className="col-start-1 col-end-8 p-3 rounded-lg">
                      <div className="flex flex-row items-start">
                        <Avatar className="h-10 w-10">
                          <AvatarFallback className="bg-primary text-primary-foreground text-sm">
                            HN
                          </AvatarFallback>
                        </Avatar>
                        <div className="relative ml-3 text-sm bg-background py-2 px-4 shadow rounded-xl">
                          <div className="font-semibold text-xs text-muted-foreground mb-1">
                            Hùng Nguyễn
                          </div>
                          <div>Mình vừa thêm chi tiêu ăn tối 450k nhé!</div>
                        </div>
                      </div>
                    </div>

                    {/* Message from me */}
                    <div className="col-start-6 col-end-13 p-3 rounded-lg">
                      <div className="flex items-start justify-start flex-row-reverse">
                        <Avatar className="h-10 w-10">
                          <AvatarFallback>ME</AvatarFallback>
                        </Avatar>
                        <div className="relative mr-3 text-sm bg-primary/10 py-2 px-4 shadow rounded-xl">
                          <div>Ok mình đã chuyển tiền rồi nhé!</div>
                          <div className="absolute text-xs bottom-0 right-0 -mb-5 mr-2 text-muted-foreground">
                            Đã xem
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* System message */}
                    <div className="col-start-1 col-end-13 p-3">
                      <div className="flex justify-center">
                        <div className="bg-muted/50 rounded-full px-4 py-1 text-xs">
                          Hôm nay
                        </div>
                      </div>
                    </div>

                    {/* Expense notification */}
                    <div className="col-start-1 col-end-8 p-3 rounded-lg">
                      <div className="flex flex-row items-start">
                        <div className="flex items-center justify-center h-10 w-10 rounded-full bg-green-500 text-white">
                          <Receipt className="h-5 w-5" />
                        </div>
                        <div className="relative ml-3 text-sm bg-green-50 dark:bg-green-950 py-3 px-4 shadow rounded-xl">
                          <div className="font-semibold text-green-700 dark:text-green-400">
                            Chi tiêu mới
                          </div>
                          <div className="mt-1">
                            <span className="font-medium">Ăn tối</span> -{" "}
                            <span className="font-semibold">450,000đ</span>
                          </div>
                          <div className="text-xs text-muted-foreground mt-1">
                            Chia đều cho 3 người
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Input Area */}
              <div className="flex flex-row items-center h-16 rounded-xl bg-background w-full px-4">
                <div>
                  <button className="flex items-center justify-center text-muted-foreground hover:text-foreground">
                    <Paperclip className="w-5 h-5" />
                  </button>
                </div>
                <div className="flex-grow ml-4">
                  <div className="relative w-full">
                    <input
                      type="text"
                      value={message}
                      onChange={(e) => setMessage(e.target.value)}
                      placeholder="Nhập tin nhắn..."
                      className="flex w-full border rounded-xl focus:outline-none focus:border-primary pl-4 pr-12 h-10 bg-background"
                      onKeyDown={(e) => {
                        if (e.key === "Enter" && !e.shiftKey) {
                          e.preventDefault();
                          // Send message
                          setMessage("");
                        }
                      }}
                    />
                    <button className="absolute flex items-center justify-center h-full w-12 right-0 top-0 text-muted-foreground hover:text-foreground">
                      <Smile className="w-6 h-6" />
                    </button>
                  </div>
                </div>
                <div className="ml-4">
                  <Button
                    className="flex items-center justify-center rounded-xl px-4 py-1 flex-shrink-0"
                    disabled={!message.trim()}
                  >
                    <span>Gửi</span>
                    <Send className="w-4 h-4 ml-2 transform rotate-45 -mt-px" />
                  </Button>
                </div>
              </div>
            </div>
          ) : (
            <div className="flex flex-col items-center justify-center h-full">
              <Users className="w-16 h-16 text-muted-foreground mb-4" />
              <h3 className="text-lg font-medium mb-2">
                Chọn một nhóm để bắt đầu
              </h3>
              <p className="text-muted-foreground text-sm">
                Xem chi tiêu và trò chuyện với các thành viên
              </p>
            </div>
          )}
        </div>
      </div>

      <CreateGroupModal
        isOpen={showCreateModal}
        onClose={() => setShowCreateModal(false)}
      />
      <JoinGroupModal
        isOpen={showJoinModal}
        onClose={() => setShowJoinModal(false)}
      />
      {selectedGroup && (
        <CreateExpenseModal
          isOpen={showExpenseModal}
          onClose={() => setShowExpenseModal(false)}
          groupId={selectedGroup.id}
        />
      )}
    </div>
  );
}
