using System.Collections.Generic;
using System.Linq;
using PPF.Models.ViewModels;

namespace PPF.Models
{
    public static class LinqQueries
    {
        public static IQueryable<Season> GetCurrentSeason(this IQueryable<Season> seasons)
        {
            return seasons.Where(season => season.IsCurrent);
        }

        public static IQueryable<Game> GetCurrentWeekGames(this IQueryable<Game> games, int week)
        {
            return games.Where(game => game.Season.IsCurrent && game.Week == week);
        }

        public static IQueryable<Pick> GetCurrentWeekPicks(this IQueryable<Pick> picks, int week)
        {
            return picks.Where(p => p.Game.Season.IsCurrent && p.Game.Week == week);
        }

        public static int GetMaxRegularSeasonWeekCurrentSeason(this IQueryable<Game> games)
        {
            return (from g in games
                    where g.IsPlayoff == false && g.IsSuperbowl == false && g.Season.IsCurrent
                    select g.Week).Max();
        }

        public static IQueryable<PlayoffSuperbowlPick> GetWinningHomePlayoffPicks(this IQueryable<PlayoffSuperbowlPick> picks, List<Game> games)
        {
            return from p in picks
                   where (from g in games where g.IsPlayoff select g.HomeTeam_Id).ToList().Contains(p.Team_Id)
                   && p.IsPlayoff
                   select p;
        }

        public static IQueryable<PlayoffSuperbowlPick> GetWinningAwayPlayoffPicks(this IQueryable<PlayoffSuperbowlPick> picks, List<Game> games)
        {
            return from p in picks
                   where p.IsPlayoff && (from g in games where g.IsPlayoff select g.AwayTeam_Id).ToList().Contains(p.Team_Id)
                   select p;
        }

        public static IQueryable<PlayoffSuperbowlPick> GetWinningHomeSuperbowlPicks(this IQueryable<PlayoffSuperbowlPick> picks, List<Game> games)
        {
            return from p in picks
                   where p.IsPlayoff && (from g in games where g.IsSuperbowl select g.HomeTeam_Id).ToList().Contains(p.Team_Id)
                   select p;
        }

        public static IQueryable<PlayoffSuperbowlPick> GetWinningAwaySuperbowlPicks(this IQueryable<PlayoffSuperbowlPick> picks, List<Game> games)
        {
            return from p in picks
                   where p.IsPlayoff && (from g in games where g.IsPlayoff select g.AwayTeam_Id).ToList().Contains(p.Team_Id)
                   select p;
        }

        public static IQueryable<Pick> GetCurrentWeekPicks(this IQueryable<Pick> picks, int week, string userId)
        {
            return picks.Where(p => p.Game.Season.IsCurrent && p.Game.Week == week && p.UserId == userId);
        }

        public static IQueryable<PlayoffSuperbowlPick> GetCurrentWeekPlayoffSuperbowlPicks(this IQueryable<PlayoffSuperbowlPick> psPicks, int week)
        {
            return psPicks.Where(ps => ps.Season.IsCurrent && ps.Week == week);
        }

        public static IQueryable<PlayoffSuperbowlPick> GetCurrentWeekPlayoffSuperbowlPicks(this IQueryable<PlayoffSuperbowlPick> psPicks, int week, string userId)
        {
            return psPicks.Where(ps => ps.Season.IsCurrent && ps.Week == week && ps.UserId == userId);
        }

        public static IQueryable<PlayoffSuperbowlPick> GetCurrentWeekPlayoffPicks(this IQueryable<PlayoffSuperbowlPick> psPicks, int week)
        {
            return GetCurrentWeekPlayoffSuperbowlPicks(psPicks, week).Where(ps => ps.IsPlayoff);
        }

        public static IQueryable<PlayoffSuperbowlPick> GetCurrentWeekPlayoffPicks(this IQueryable<PlayoffSuperbowlPick> psPicks, int week, string userId)
        {
            return GetCurrentWeekPlayoffSuperbowlPicks(psPicks, week).Where(ps => ps.IsPlayoff && ps.UserId == userId);
        }

        public static IQueryable<PlayoffSuperbowlPick> GetCurrentWeekSuperbowlPicks(this IQueryable<PlayoffSuperbowlPick> psPicks, int week)
        {
            return GetCurrentWeekPlayoffSuperbowlPicks(psPicks, week).Where(ps => ps.IsSuperbowl);
        }

        public static IQueryable<PlayoffSuperbowlPick> GetCurrentWeekSuperbowlPicks(this IQueryable<PlayoffSuperbowlPick> psPicks, int week, string userId)
        {
            return GetCurrentWeekPlayoffSuperbowlPicks(psPicks, week).Where(ps => ps.IsSuperbowl && ps.UserId == userId);
        }

        public static IQueryable<StandardPick> GetCurrentWeekStandardPicks(this IQueryable<StandardPick> standardPicks, int week)
        {
            return standardPicks.Where(sp => sp.Game.Season.IsCurrent && sp.Game.Week == week && sp.Active.HasValue && sp.Active.Value);
        }

        public static IQueryable<StandardPlayoffSuperbowlPick> GetCurrentWeekSuperbowlStandardPicks(this IQueryable<StandardPlayoffSuperbowlPick> standardPicks, int week)
        {
            return standardPicks.Where(sp => sp.IsSuperbowl && sp.Season.IsCurrent && sp.Week == week && sp.Active.HasValue && sp.Active.Value);
        }

        public static IQueryable<StandardPlayoffSuperbowlPick> GetCurrentWeekPlayoffStandardPicks(this IQueryable<StandardPlayoffSuperbowlPick> standardPicks, int week)
        {
            return standardPicks.Where(sp => sp.Season.IsCurrent && sp.IsPlayoff && sp.Week == week && sp.Active.HasValue && sp.Active.Value);
        }

        public static IQueryable<Team> GetTeamsByConference(this IQueryable<Team> teams, string conference)
        {
            return teams.Where(t => t.Conference.ToUpper() == conference.ToUpper());
        }

        public static IQueryable<Game> GetGamesWithNoPicks(this IQueryable<Game> games, IQueryable<Pick> picks)
        {
            return from g in games
                   where !(from p in picks select p.Game.Id).Contains(g.Id)
                   select g;
        }

        public static List<GameViewModel> GetGamesWithNoPicks(this List<GameViewModel> games, List<PickViewModel> picks)
        {
            return (from g in games
                    where !(from p in picks select p.Game.Id).Contains(g.Id)
                    select g).ToList();
        }
    }
}