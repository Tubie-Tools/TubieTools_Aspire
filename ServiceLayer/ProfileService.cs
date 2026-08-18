using DataAccessLayer;
using DataAccessLayer.Data.Contexts;
using ModelLayer;
using SanityCheque.Common;

namespace ServiceLayer
{
    public class ProfileService : IProfileService
    {
        private readonly KitContext _context;
        private readonly ApplicationDbContext _dbContext;
        public ProfileService(KitContext context, ApplicationDbContext dbContext)
        {
            _context = context;
            _dbContext = dbContext;
        }

        public async Task<IProfile> Get(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(userName))
                {
                    return null;
                }
                else
                {
                    var id = _dbContext.Users.Where(f => f.UserName == userName).FirstOrDefault().Id;

                    var profile = (from r in _context.ProfileAspNetUsers.Where(f => f.FK_AspNetUser_Id == id)
                                   join p in _context.Profile on r.FK_Profile_Id equals p.Id
                                   select p).FirstOrDefault();

                    if (profile != null)
                    {
                        var toDto = ProfileDtoConverter.ToDto(profile);
                        return toDto;
                    }
                    else
                    {
                        return new ProfileViewModel();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Upsert(IProfile model, string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(userName))
                {
                    // do nothing
                    System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    var profile = _context.Profile.Where(f => f.Id == model.Id).FirstOrDefault();
                    if (profile != null)
                    {
                        // we updated the profile info, nothing to do with what profile is connected to what user
                        profile.Bio = model.Bio;
                        //profile.DateOfBirth = model.DateOfBirth;
                        profile.Name = model.Name;                        
                        
                        _context.Profile.Update(profile);
                        _context.SaveChanges();
                    }
                    else
                    {
                        var id = _dbContext.Users.Where(f => f.UserName == userName).FirstOrDefault().Id;
                        var newProfile = new Profile()
                        {
                            Bio = model.Bio,
                            Id = model.Id,
                            //DateOfBirth = model.DateOfBirth,
                            Name = model.Name
                        };

                        _context.Profile.Add(newProfile);
                        _context.SaveChanges();

                        // once this is saved, we will add this to ProfileAspNetUsers
                        var profileAspNetUser = new ProfileAspNetUsers { FK_AspNetUser_Id = id, FK_Profile_Id = newProfile.Id, Description = "SYSTEM" };
                        _context.ProfileAspNetUsers.Add(profileAspNetUser);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// used to grant access to an email
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        public void Grant(SimpleGrant model, string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(userName) || userName.Length > 50)
                {
                    // do nothing for awhile
                    System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    // get list of profile for current user
                    var user = _dbContext.Users.Where(f => f.UserName == userName).FirstOrDefault();
                    if (user != null)
                    {
                        var profile = _context.ProfileAspNetUsers.Where(f => f.FK_AspNetUser_Id == user.Id).FirstOrDefault();

                        if (profile == null)
                            throw new ApplicationException("Must create a profile before granting access.");
                        // check for the account
                        var accountFound = _dbContext.Users.Where(f => f.UserName == model.Email).FirstOrDefault();

                        if (accountFound != null)
                        {
                            // if we found an account, bind it
                            var profileAspNetUser = new ProfileAspNetUsers
                            {
                                FK_AspNetUser_Id = accountFound.Id,
                                FK_Profile_Id = profile.FK_Profile_Id,
                                Description = "SYSTEM"
                            };
                            _context.ProfileAspNetUsers.Add(profileAspNetUser);
                            _context.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
