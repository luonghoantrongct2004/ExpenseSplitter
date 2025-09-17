import { CreateGroupRequest, GroupListResponse, UpdateGroupRequest } from "@/src/entities/dto";
import { Group } from "@/src/entities/model";
import axios from "axios";

export const groupService = {
    //NOTE - ⁡⁢⁢Lấy tất cả group
    async getMyGroups(page = 1, pageSize = 10): Promise<GroupListResponse> {
        const { data } = await axios.get('/api/groups/my-groups', {
            params: { page, pageSize }
        });
        return data;
    },
    //NOTE - Lấy group theo id
    async getGroup(id: string): Promise<Group>{
        const {data} = await axios.post(`/api/groups/${id}`);
        return data
    },
    //NOTE - Tạo group 
    async createGroup(request: CreateGroupRequest): Promise<Group> {
        const { data } = await axios.post('/api/groups', request);
        return data;
    },

    //NOTE - Cập nhật group
    async updateGroup(id: string, request: UpdateGroupRequest): Promise<Group> {
        const { data } = await axios.put(`/api/groups/${id}`, request);
        return data;
    },

    //NOTE - Rời group
    async leaveGroup(id: string): Promise<void> {
        await axios.post(`/api/groups/${id}/leave`);
    },

    //NOTE -  Thêm thành viên
    async inviteMember(groupId: string, email: string): Promise<void> {
        await axios.post(`/api/groups/${groupId}/invite`, { email });
    },

    //NOTE - Tham gia group bằng InviteCode
    async joinGroup(inviteCode: string): Promise<Group> {
        const { data } = await axios.post('/api/groups/join', { inviteCode });
        return data;
    },

    //NOTE - Kick member khỏi group ⁡⁣⁢⁣(Chỉ admin)⁡
    async removeMember(groupId: string, userId: string): Promise<void> {
        await axios.delete(`/api/groups/${groupId}/members/${userId}`);
    },

    //NOTE - Tạo lại Invite Code
    async regenerateInviteCode(groupId: string): Promise<string> {
        const { data } = await axios.post(`/api/groups/${groupId}/regenerate-invite`);
        return data.inviteCode;
    }
}
