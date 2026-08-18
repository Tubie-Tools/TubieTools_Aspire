using DataAccessLayer;
using DataAccessLayer.Data.Contexts;
using DTOLayer;
using ModelLayer;


namespace ServiceLayer
{
    /// <summary>
    /// for all events big and small
    /// </summary>
    public class EventService : IEventService
    {
        private readonly KitContext _context;
        private readonly ApplicationDbContext _dbContext;

        public EventService(KitContext context, ApplicationDbContext dbContext)
        {
            _context = context;
            _dbContext = dbContext;
        }

        public async Task<IEvent[]> Get(string userName)
        {
            try
            {
                if (string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(userName))
                {
                    return null;
                }
                else
                {
                    // always have to check if they are who they say they are ...
                    var id = _dbContext.Users.Where(f => f.UserName == userName).FirstOrDefault().Id;
                    var myEvents = (from r in _context.ProfileAspNetUsers.Where(f => f.FK_AspNetUser_Id == id)
                                    join e in _context.Event on r.FK_Profile_Id equals e.ProfileId
                                    select e).OrderByDescending(f => f.DateCreated).ToList();
                    if (myEvents != null && myEvents.Any())
                    {
                        var dto = EventDtoConverter.ToDto(myEvents);
                        if (dto != null && dto.Any())
                        {
                            return dto.ToArray();
                        }
                    }

                    return null; // no data found
                }
            }
            catch (Exception ex)
            {
                // log ex
                throw;
            }
        }

        public void Create(IEvent model, string userName)
        {
            try
            {
                var id = _dbContext.Users.Where(f => f.UserName == userName).FirstOrDefault().Id;
                var profileAspNetUserFound = _context.ProfileAspNetUsers.Where(f => f.FK_AspNetUser_Id == id).FirstOrDefault();
                if (profileAspNetUserFound != null)
                {
                    model.CreatedBy = profileAspNetUserFound.FK_Profile_Id.ToString();
                    // check if we have an event type
                    var eventTypeRecord = _context.EventType.Where(f => f.Name == model.EventName).FirstOrDefault();
                    if (eventTypeRecord == null)
                    {
                        // we don't have an event type, this means its a new value in the system so lets create a record for it
                        var eventType = new EventType { Name = model.EventName, DateCreated = DateTime.Now, CreatedBy = "System" };
                        _context.EventType.Add(eventType);
                        _context.SaveChanges();
                        if (eventType.Id != 0)
                        {
                            // we made a new eventType
                            var saveModel = EventDtoConverter.ToDto(model, profileAspNetUserFound.FK_Profile_Id, eventType.Id);
 
                            _context.Event.Add(saveModel);
                            _context.SaveChanges();
                        }
                        else { throw new ApplicationException("Issue with event types. We apologize for the inconvenience."); }
                    }
                    else
                    {
                        // use existing event type
                        var saveModel = EventDtoConverter.ToDto(model, profileAspNetUserFound.FK_Profile_Id, eventTypeRecord.Id);
 
                        _context.Event.Add(saveModel);
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public IList<IEvent> GroupedEvents(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedList<IEvent>> Get(ProductParameters param, string userName)
        {
            var events = await Get(userName);
            return events != null && events.Length > 0
                ? PagedList<IEvent>
                    .ToPagedList(events, param.PageNumber, param.PageSize)
                : null;
        }

        public async Task<PagedList<IEvent>> GetWeightsOverTime(ProductParameters param, string userName)
        {
            throw new NotImplementedException();
            //var events = await Get(userName);
            //var specificEvents = (from r in events
            //                      group r by new { r.WeightPounds, r.WeightOunces } into newGroup 
            //                      select newGroup).ToArray();

            //return PagedList<IEvent>
            //    .ToPagedList(specificEvents.AsEnumerable(), param.PageNumber, param.PageSize);
        }
        
    }
}
