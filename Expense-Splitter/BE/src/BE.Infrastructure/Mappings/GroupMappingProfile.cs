using AutoMapper;
using BE.Application.DTOs.Group;
using BE.Domain.Entities;

namespace BE.Infrastructure.Mappings
{
    public class GroupMappingProfile : Profile
    {
        public GroupMappingProfile()
        {
            // Group -> GroupDto
            CreateMap<Group, GroupDto>()
                .ForMember(dest => dest.MemberCount,
                    opt => opt.MapFrom(src => src.Members.Count(m => m.IsActive)))
                .ForMember(dest => dest.TotalExpenses,
                    opt => opt.MapFrom(src => src.Expenses.Where(e => !e.IsDeleted).Sum(e => e.Amount)))
                // Ignore các field sẽ set manual trong service
                .ForMember(dest => dest.IsAdmin, opt => opt.Ignore())
                .ForMember(dest => dest.UserBalance, opt => opt.Ignore());

            // Group -> GroupDetailDto
            CreateMap<Group, GroupDetailDto>()
                .IncludeBase<Group, GroupDto>()
                .ForMember(dest => dest.Members,
                    opt => opt.MapFrom(src => src.Members.Where(m => m.IsActive)))
                .ForMember(dest => dest.Statistics, opt => opt.Ignore());

            // Group -> GroupListDto
            CreateMap<Group, GroupListDto>()
                .ForMember(dest => dest.MemberCount,
                    opt => opt.MapFrom(src => src.Members.Count(m => m.IsActive)))
                .ForMember(dest => dest.LastActivity,
                    opt => opt.MapFrom(src => src.UpdatedAt))
                // Ignore user-specific fields
                .ForMember(dest => dest.UserBalance, opt => opt.Ignore())
                .ForMember(dest => dest.IsAdmin, opt => opt.Ignore());

            // GroupMember -> GroupMemberDto
            CreateMap<GroupMember, GroupMemberDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Name : ""))
                .ForMember(dest => dest.UserEmail,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : ""))
                .ForMember(dest => dest.UserAvatar,
                    opt => opt.MapFrom(src => src.User != null ? src.User.AvatarUrl : null))
                // Balance sẽ được set trong service
                .ForMember(dest => dest.Balance, opt => opt.Ignore());

            // CreateGroupDto -> Group
            CreateMap<CreateGroupDto, Group>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.InviteCode, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            // InviteMemberDto -> GroupMember
            CreateMap<InviteMemberDto, GroupMember>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
        }
    }
}
