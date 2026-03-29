using DataAccessLayer;
using ModelLayer;


namespace SanityCheque.Common
{
    public static class ProfileDtoConverter
    {
        public static IProfile ToDto(Profile profile)
        {
            var dto = new ProfileViewModel()
            {
                Id = profile.Id,
                Name = profile.Name,
                Bio = profile.Bio,
                DateOfBirth = profile.DateOfBirth
            };

            return dto;
        }
    }
}