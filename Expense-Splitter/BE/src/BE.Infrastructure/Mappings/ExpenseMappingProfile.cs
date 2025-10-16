using AutoMapper;
using BE.Application.DTOs.Expense.Expense;
using BE.Domain.Entities;

namespace BE.Infrastructure.Mappings;

public class ExpenseMappingProfile : Profile
{
    public ExpenseMappingProfile()
    {
        CreateMap<CreateExpenseDto, Expense>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Splits, opt => opt.Ignore());

        CreateMap<Expense, ExpenseDto>()
            .ForMember(dest => dest.PaidByName, opt => opt.MapFrom(src => src.PaidBy.Name))
            .ForMember(dest => dest.PaidByAvatar, opt => opt.MapFrom(src => src.PaidBy.AvatarUrl))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.Name))
            .ForMember(dest => dest.Splits, opt => opt.MapFrom(src => src.Splits));

        CreateMap<ExpenseSplit, ExpenseSplitDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User.AvatarUrl));

        CreateMap<CreateExpenseSplitDto, ExpenseSplit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Expense, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
    }
}