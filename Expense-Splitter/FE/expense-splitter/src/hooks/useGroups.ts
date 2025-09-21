import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { groupService } from "@/src/lib/services/groupService";
import {
  CreateGroupRequest,
  PagedList,
  GroupListDto,
  GroupDetailDto,
} from "@/src/entities/group/group.dto";
import { toast } from "react-hot-toast";

// Get my groups
export const useMyGroups = (page = 1, pageSize = 12) => {
  return useQuery<PagedList<GroupListDto>>({
    queryKey: ["groups", "my-groups", page, pageSize],
    queryFn: () => groupService.getMyGroups(page, pageSize),
  });
};

// Get single group
export const useGroup = (id: string) => {
  return useQuery<GroupDetailDto>({
    queryKey: ["groups", id],
    queryFn: () => groupService.getGroup(id),
    enabled: !!id,
  });
};

// Create group
export const useCreateGroup = () => {
  const queryClient = useQueryClient();
  const router = useRouter();

  return useMutation({
    mutationFn: (data: CreateGroupRequest) => groupService.createGroup(data),
    onSuccess: (newGroup) => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      toast.success("Tạo nhóm thành công!");
      router.push(`/api/groups/${newGroup.id}`);
    },
  });
};

// Leave group
export const useLeaveGroup = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => groupService.leaveGroup(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      toast.success("Đã rời khỏi nhóm");
    },
  });
};

// Join group
export const useJoinGroup = () => {
  const queryClient = useQueryClient();
  const router = useRouter();

  return useMutation({
    mutationFn: (inviteCode: string) => groupService.joinGroup(inviteCode),
    onSuccess: (joinedGroup) => {
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      toast.success("Tham gia nhóm thành công!");
      router.push(`/api/groups/${joinedGroup.id}`);
    },
  });
};
