import { useQuery } from "@tanstack/react-query"
import { groupService } from "../lib/services/groupService";

export const useMyGroups = (page = 1, pageSize = 10) => {
  return useQuery({
    queryKey: ['groups', 'my-groups', page, pageSize],
    queryFn: () => groupService.getMyGroups(page, pageSize),
  });
};