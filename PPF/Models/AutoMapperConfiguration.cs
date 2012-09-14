using System.Linq;
using AutoMapper;
using PPF.Models.ViewModels;

namespace PPF.Models
{
    public class AutoMapperConfiguration
    {
        public static void Setup(ppfEntities ctx)
        {
            Mapper.Reset();

            Mapper.CreateMap<Game, GameViewModel>()
                .ForMember(m => m.HomeTeam,
                           opt =>
                           opt.MapFrom(
                               src => ctx.Teams.Where(t => t.Id == src.HomeTeam_Id).Single()))                               
                .ForMember(m => m.AwayTeam,
                           opt =>
                           opt.MapFrom(
                               src => ctx.Teams.Where(t => t.Id == src.AwayTeam_Id).Single()));

            Mapper.CreateMap<Pick, PickViewModel>()
                .ForMember(m => m.Game, opt => opt.MapFrom(src => src.Game))
                .ForMember(m => m.Team, opt => opt.MapFrom(src => ctx.Teams.Where(t => t.Id == src.Team_Id).Single()))
                .ForMember(m => m.IsStandard, opt => opt.UseValue(false))
                .ForMember(m => m.PossiblePoints, opt => opt.Ignore());

            Mapper.CreateMap<PlayoffSuperbowlPick, SuperbowlPlayoffPickViewModel>()
                .ForMember(m => m.IsStandard, opt => opt.UseValue(false))
                .ForMember(m => m.Team, opt => opt.MapFrom(src => ctx.Teams.Where(t => t.Id == src.Team_Id).Single()));

            Mapper.CreateMap<StandardPick, PickViewModel>()
                .ForMember(m => m.Game, opt => opt.MapFrom(src => src.Game))
                .ForMember(m => m.Team, opt => opt.MapFrom(src => ctx.Teams.Where(t => t.Id == src.Team_Id).Single()))
                .ForMember(m => m.IsStandard, opt => opt.UseValue(true))
                .ForMember(m => m.UserId, opt => opt.Ignore())
                .ForMember(m => m.PossiblePoints, opt => opt.Ignore());
                

            Mapper.CreateMap<StandardPlayoffSuperbowlPick, SuperbowlPlayoffPickViewModel>()
                .ForMember(m => m.IsStandard, opt => opt.UseValue(true))
                .ForMember(m => m.IsWinner, opt => opt.Ignore())
                .ForMember(m => m.UserId, opt=>opt.Ignore())
                .ForMember(m => m.Team, opt => opt.MapFrom(src => ctx.Teams.Where(t => t.Id == src.Team_Id).Single()));

            Mapper.CreateMap<Team, TeamViewModel>();

            Mapper.CreateMap<Season, SeasonViewModel>()
                .ForMember(m => m.Leaderboard, opt => opt.Ignore())
                .ForMember(m => m.SeasonPointTotal, opt => opt.Ignore())
                .ForMember(m => m.WeeklyLeaderboard, opt => opt.Ignore());


            Mapper.CreateMap<Season, SeasonSearchViewModel>()
                .ForMember(m => m.Season,
                            opt => opt.MapFrom(src => string.Concat(src.StartDate.Year.ToString(), " Season")))
                .ForMember(m => m.Weeks, 
                            opt => opt.MapFrom(src => src.NumberOfWeeks));

            Mapper.AssertConfigurationIsValid();
        }


    }
}