using DataAccessLayer;
using ModelLayer;
using ModelLayer.ViewModels;


namespace SanityCheque.Common
{
    public static class ProfileDtoConverter
    {
        public static IProfile ToDto(Profile profile)
        {
            IProfile dto = new ProfileViewModel()
            {
                Id = profile.Id,
                Name = profile.Name,
                Bio = profile.Bio,
                DateOfBirth = profile.DateOfBirth,
                GenderId = profile.GenderId
            };

            return dto;
        }
    }
}