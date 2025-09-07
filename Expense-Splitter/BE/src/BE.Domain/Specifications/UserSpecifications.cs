using BE.Domain.Entities;

namespace BE.Domain.Specifications;

public class UserByGoogleIdSpec : BaseSpecification<User>
{
    public UserByGoogleIdSpec(string googleId)
        : base(u => u.GoogleId == googleId)
    {
        AddInclude(u => u.UserPreference!);
    }

    public class UserByEmailSpec : BaseSpecification<User>
    {
        public UserByEmailSpec(string email)
            : base(u => u.Email.ToLower() == email.ToLower())
        {
            AddInclude(u => u.UserPreference!);
        }
    }

    public class UserWithPreferencesSpec : BaseSpecification<User>
    {
        public UserWithPreferencesSpec(Guid userId)
            : base(u => u.Id == userId)
        {
            AddInclude(u => u.UserPreference!);
        }
    }

    public class ActiveUsersSpec : BaseSpecification<User>
    {
        public ActiveUsersSpec() : base(u => u.IsActive)
        {
            ApplyOrderByDescending(u => u.LastLoginAt ?? u.CreatedAt);
        }
    }
}