/* eslint-disable @typescript-eslint/no-explicit-any */
import { ApiResponse } from "@/src/entities/dto";
import axios from "../axios";
import {
  PagedList,
  GroupListDto,
  GroupDetailDto,
  CreateGroupRequest,
  UpdateGroupRequest,
  InviteMemberRequest,
} from "@/src/entities/group/group.dto";

export const groupService = {
  //NOTE - Lấy tất cả my group
  async getMyGroups(page = 1, pageSize = 10): Promise<PagedList<GroupListDto>> {
    try {
      const response = await axios.get<ApiResponse<GroupListDto[]>>(
        "/api/groups/mygroups",
        {
          params: {
            pageNumber: page,
            pageSize: pageSize,
          },
        }
      );
      if (!response.data.success) {
        throw new Error(response.data.message || "Failed to fetch groups");
      }

      if (!response.data.data) {
        return {
          items: [],
          currentPage: page,
          totalPages: 0,
          pageSize: pageSize,
          totalCount: 0,
          hasPrevious: false,
          hasNext: false,
        };
      }

      const groups = response.data.data || [];

      return {
        items: groups, // Đây là điểm quan trọng
        currentPage: page,
        totalPages: 1,
        pageSize: pageSize,
        totalCount: groups.length,
        hasPrevious: false,
        hasNext: false,
      };
    } catch (error: any) {
      throw error;
    }
  },

  //NOTE - Lấy group theo id
  async getGroup(id: string): Promise<GroupDetailDto> {
    const response = await axios.get<ApiResponse<GroupDetailDto>>(
      `/api/groups/${id}`
    );
    if (!response.data.success) {
      throw new Error(response.data.message);
    }

    if (!response.data.data) {
      throw new Error(response.data.message);
    }
    return response.data.data;
  },

  //NOTE - Tạo group
  async createGroup(request: CreateGroupRequest): Promise<GroupDetailDto> {
    const response = await axios.post<ApiResponse<GroupDetailDto>>(
      "/api/groups",
      request
    );
    if (!response.data.success) {
      throw new Error(response.data.message);
    }

    if (!response.data.data) {
      throw new Error(response.data.message);
    }
    return response.data.data;
  },

  //NOTE - Cập nhật group
  async updateGroup(
    id: string,
    request: UpdateGroupRequest
  ): Promise<GroupDetailDto> {
    const response = await axios.put<ApiResponse<GroupDetailDto>>(
      `/api/groups/${id}`,
      request
    );
    if (!response.data.success) {
      throw new Error(response.data.message);
    }

    if (!response.data.data) {
      throw new Error(response.data.message);
    }
    return response.data.data;
  },

  //NOTE - Rời group
  async leaveGroup(id: string): Promise<void> {
    await axios.post(`/api/groups/${id}/leave`);
  },

  //NOTE - Thêm thành viên
  async inviteMember(
    groupId: string,
    request: InviteMemberRequest
  ): Promise<void> {
    await axios.post(`/api/groups/${groupId}/members`, request);
  },

  //NOTE - Tham gia group bằng InviteCode
  async joinGroup(inviteCode: string): Promise<GroupDetailDto> {
    const response = await axios.post<ApiResponse<GroupDetailDto>>(
      "/api/groups/join",
      { inviteCode }
    );
    if (!response.data.success) {
      throw new Error(response.data.message);
    }

    if (!response.data.data) {
      throw new Error(response.data.message);
    }
    return response.data.data;
  },

  //NOTE - Kick member khỏi group (Chỉ admin)
  async removeMember(groupId: string, userId: string): Promise<void> {
    await axios.delete(`/api/groups/${groupId}/members/${userId}`);
  },

  //NOTE - Tạo lại Invite Code
  async regenerateInviteCode(groupId: string): Promise<string> {
    const response = await axios.post<ApiResponse<{ inviteCode: string }>>(
      `/api/groups/${groupId}/invite-code`
    );
    if (!response.data.success) {
      throw new Error(response.data.message);
    }

    if (!response.data.data) {
      throw new Error(response.data.message);
    }
    return response.data.data.inviteCode;
  },

  //NOTE - Archive group
  async archiveGroup(id: string): Promise<void> {
    await axios.post(`/api/groups/${id}/archive`);
  },

  //NOTE - Get invite QR code
  async getInviteQRCode(
    id: string
  ): Promise<{ inviteCode: string; inviteUrl: string; qrData: string }> {
    const response = await axios.get<
      ApiResponse<{ inviteCode: string; inviteUrl: string; qrData: string }>
    >(`/api/groups/${id}/invite-qr`);
    if (!response.data.success) {
      throw new Error(response.data.message);
    }

    if (!response.data.data) {
      throw new Error(response.data.message);
    }
    return response.data.data;
  },
};
