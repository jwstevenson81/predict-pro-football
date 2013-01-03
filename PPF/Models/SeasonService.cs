using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using AutoMapper;
using PPF.Models.ViewModels;


namespace PPF.Models
{
    public class SeasonService
    {
        private readonly ppfEntities _ctx;

        public SeasonService()
        {
            _ctx = new ppfEntities();
        }


        public LeaderboardViewModel GetWeeklyLeaderboard(int week)
        {
            var vm = new LeaderboardViewModel();
            // get the summed scores for this season
            var scores = (from p in _ctx.Picks
                          where p.Game.Season.IsCurrent && p.IsWinner.HasValue && p.IsWinner.Value && p.Game.Week == week
                          orderby p.Game.Week
                          group p by p.UserId into up
                          select new LeaderViewModel() { Week = week, UserId = up.Key, Points = up.Sum(p => p.PointTotal) }).ToList();
            //
            var psScores = (from p in _ctx.PlayoffSuperbowlPicks
                            where p.Season.IsCurrent && p.IsWinner && p.Week == week
                            orderby p.Week
                            group p by p.UserId into up
                            select new LeaderViewModel() { UserId = up.Key, Week = week, Points = up.Sum(p => p.PointTotal) }).ToList();

            foreach (var leaderViewModel in scores)
            {
                LeaderViewModel model = leaderViewModel;
                var iPsScores = psScores.SingleOrDefault(p => p.UserId == model.UserId && p.Week == model.Week);
                if (iPsScores != null)
                    leaderViewModel.Points += iPsScores.Points;
            }


            vm.Leaders = scores.OrderByDescending(p => p.Points).ToList();
            return vm;
            
        }

        public LeaderboardViewModel GetLeaderboard()
        {
            var vm = new LeaderboardViewModel();
            // get the summed scores for this season
            var scores = (from p in _ctx.Picks
                         where p.Game.Season.IsCurrent && p.IsWinner.HasValue && p.IsWinner.Value
                         orderby p.Game.Week
                         group p by p.UserId into up
                         select new LeaderViewModel() {UserId = up.Key, Points = up.Sum(p => p.PointTotal)}).ToList();
            //
            var psScores = (from p in _ctx.PlayoffSuperbowlPicks
                           where p.Season.IsCurrent && p.IsWinner
                           orderby p.Week 
                           group p by p.UserId into up
                           select new LeaderViewModel() {UserId = up.Key, Points = up.Sum(p => p.PointTotal)}).ToList();

            foreach (var leaderViewModel in scores)
            {
                LeaderViewModel model = leaderViewModel;
                var iPsScores = psScores.SingleOrDefault(p => p.UserId == model.UserId && p.Week == model.Week);
                if (iPsScores != null)
                    leaderViewModel.Points += iPsScores.Points;
            }


            vm.Leaders = scores.OrderByDescending(p => p.Points).ToList();
            return vm;
        }

        public SeasonViewModel GetCurrentSeason(string userId)
        {
            // setup auto-mapper
            AutoMapperConfiguration.Setup(_ctx);
            var season = new Season();
            if (_ctx.Seasons.Count(s => s.IsCurrent) == 1)
                season = _ctx.Seasons.Where(s => s.IsCurrent).Single();
            // map to a view model
            var svm = Mapper.Map<Season, SeasonViewModel>(season);
            svm.Leaderboard = GetLeaderboard();
            // get the season's score thus far
            var winningPicks =
                _ctx.Picks.Where(p => p.Game.Season.IsCurrent && p.IsWinner.HasValue && p.IsWinner.Value && p.UserId == userId).ToList();
            var winningPsPicks = _ctx.PlayoffSuperbowlPicks.Where(p => p.IsWinner && p.Season.IsCurrent && p.UserId == userId).ToList();
            // we need to make sure there are winning picks before we try to sum them
            svm.SeasonPointTotal = winningPicks.Count > 0 ? winningPicks.Sum(p => p.PointTotal) : 0;
            svm.SeasonPointTotal += winningPsPicks.Count > 0 ? winningPsPicks.Sum(p => p.PointTotal) : 0;
            //
            return svm;
        }

        public CurrentSeasonJsonModel GetCurrentSeason()
        {
            // setup auto-mapper
            AutoMapperConfiguration.Setup(_ctx);
            return
                _ctx.Seasons.GetCurrentSeason().Select(
                    s =>
                    new CurrentSeasonJsonModel() {SeasonYear = s.StartDate.Year, Id = s.Id}).SingleOrDefault();
        }

        public MyPredictViewModel SetupWeek(int week, string userId)
        {

            // setup auto-mapper
            AutoMapperConfiguration.Setup(_ctx);
            // create the season view model
            var season = Mapper.Map<Season, SeasonViewModel>(_ctx.Seasons.GetCurrentSeason().Single());
            // create the games view model
            var games = Mapper.Map<List<Game>, List<GameViewModel>>(_ctx.Games.GetCurrentWeekGames(week).ToList());
            // do we need to completly disable this week?
            var disabled = true;
            if (season.StartDate <= DateTime.Now && games.Count > 0)
            {
                // select the latest game for the week
                var maxGameDate = games.Select(g => g.GameDateTime).Max();
                disabled = DateTime.Now > maxGameDate;
            }
            // Get the picks for the current week for the current user
            var picks = Mapper.Map<List<Pick>, List<PickViewModel>>(_ctx.Picks.GetCurrentWeekPicks(week, userId).ToList());
            // get the games with no picks
            var noPicks = games.GetGamesWithNoPicks(picks).ToList();
            // we need a pick for each game that has no pick for the current user
            picks.AddRange(noPicks.Select(gvm => new PickViewModel { Game = gvm, UserId = userId }));
            // get the user's playoff and superbowl picks
            var psPicks = Mapper.Map<List<PlayoffSuperbowlPick>, List<SuperbowlPlayoffPickViewModel>>(_ctx.PlayoffSuperbowlPicks.GetCurrentWeekPlayoffSuperbowlPicks(week, userId).ToList());
            // get all the regular standard picks
            //var standardPicks = Mapper.Map<List<StandardPick>, List<PickViewModel>>(_ctx.StandardPicks.GetCurrentWeekStandardPicks(week).ToList());
            // get all the standard playoff picks
            //var standardPlayoffPicks = Mapper.Map<List<StandardPlayoffSuperbowlPick>, List<SuperbowlPlayoffPickViewModel>>(_ctx.StandardPlayoffSuperbowlPicks.GetCurrentWeekPlayoffStandardPicks(week).ToList());
            // get the standard superbowl picks
            //var standardSuperbowlPicks = Mapper.Map<List<StandardPlayoffSuperbowlPick>, List<SuperbowlPlayoffPickViewModel>>(_ctx.StandardPlayoffSuperbowlPicks.GetCurrentWeekSuperbowlStandardPicks(week).ToList());
            // get the teams
            var teams = Mapper.Map<List<Team>, List<TeamViewModel>>(_ctx.Teams.ToList());
            // get the week points
            var pointTotal = picks.Where(p => p.IsWinner).Sum(p => p.PointTotal);
            // do we have any playoff/superbowl points?
            var psPointTotal = psPicks.Where(p => p.IsWinner).Sum(p => p.PointTotal);
            // set the points list
            var pointsList = new List<int>();
            var pfGames = games.Where(g => g.IsPlayoff).Count();
            var sGames = games.Where(g => g.IsSuperbowl).Count();
            var showPlayoffSuperbowlPicks = false;
            var showSuperbowlPicksOnly = false;
            var maxRWeek = 0;
            if (games.Count > 0)
                maxRWeek = _ctx.Games.GetMaxRegularSeasonWeekCurrentSeason();
            showPlayoffSuperbowlPicks = (pfGames == 0 && sGames == 0);
            showSuperbowlPicksOnly = (pfGames == games.Count);
            if (pfGames == games.Count && games.Count == 4)
            {
                // this is the first playoff week
                pointsList.Add(0);
                pointsList.Add(5);
                pointsList.Add(10);
                pointsList.Add(15);
                pointsList.Add(20);
            }
            else if (pfGames == games.Count && games.Count == 2)
            {
                pointsList.Add(0);
                pointsList.Add(20);
                pointsList.Add(30);
            }
            else if (sGames == games.Count)
            {
                pointsList.Add(50);
            }
            else
            {
                // this is a normal week
                for (int i = 0; i <= games.Count; i++)
                {
                    pointsList.Add(i);
                }
            }

            if (picks.Count > 0)
            {
                foreach (var pick in _ctx.Picks.GetCurrentWeekPicks(week, userId).Where(p=>p.Game.GameDateTime < DateTime.Now).ToList())
                {
                    var pick1 = pick;
                    if (pick1.PointTotal > 0)
                        pointsList.RemoveAll(p => p == pick1.PointTotal);
                }
            }
            
            // create the view model
            var vm = new MyPredictViewModel()
            {
                TheSeason = season,
                //StandardPlayoffPicks = standardPlayoffPicks,
                //StandardSuperbowlPicks = standardSuperbowlPicks,
                //StandardWeeklyPicks = standardPicks,
                MyPicks = picks,
                MyPlayoffPicks = psPicks.Where(p => p.IsPlayoff).ToList(),
                MySuperbowlPicks = psPicks.Where(p => p.IsSuperbowl).ToList(),
                CurrentWeek = week,
                AfcTeams = teams.Where(t => t.Conference.ToUpper().Equals("AFC")).ToList(),
                NfcTeams = teams.Where(t => t.Conference.ToUpper().Equals("NFC")).ToList(),
                Disabled = disabled,
                WeekPointTotal = pointTotal,
                PointList = pointsList,
                PossiblePlayoffPointTotal = maxRWeek > 0 ? (maxRWeek - week) + 1 : 0,
                ShouldHavePlayoffSuperbowlPicks = showPlayoffSuperbowlPicks,
                PlayoffSuperbowlPointTotal = psPointTotal,
                ShouldHaveSuperbowlPicksOnly = showSuperbowlPicksOnly
            };
            // set the superbowl possible points
            if (maxRWeek > 0 && !vm.ShouldHaveSuperbowlPicksOnly)
                vm.PlayoffSuperbowlPointTotal = ((maxRWeek - week) + 1) * 2;
            else if (vm.ShouldHaveSuperbowlPicksOnly && week == maxRWeek + 1)
                vm.PossibleSuperbowlPointTotal = 30;
            else if (vm.ShouldHavePlayoffSuperbowlPicks && week == maxRWeek + 2)
                vm.PossibleSuperbowlPointTotal = 20;
            else if (vm.ShouldHavePlayoffSuperbowlPicks && week == maxRWeek + 3)
                vm.PossibleSuperbowlPointTotal = 10;
            else
                vm.PossibleSuperbowlPointTotal = 0;

            // return the newely created view model
            return vm;
        }

        public void UpdateWeeklyScores(MyPredictViewModel vm)
        {
            // which games have we updated
            var scoredGames = from p in vm.MyPicks
                              where p.Game.HomeTeamScore > 0 || p.Game.AwayTeamScore > 0
                              select p;
            // have we updated any games?
            foreach (var sg in scoredGames.ToList())
            {
                var scoredGame = sg;
                // get the game from the db
                var dbGame = _ctx.Games.Where(g => g.Id == scoredGame.Game.Id).Single();
                // set the scores
                dbGame.HomeTeamScore = sg.Game.HomeTeamScore;
                dbGame.AwayTeamScore = sg.Game.AwayTeamScore;
                // get the winner from the game
                var winner = 0;
                winner = dbGame.HomeTeamScore > dbGame.AwayTeamScore ? dbGame.HomeTeam_Id : dbGame.AwayTeam_Id;
                // get the picks for the game
                var picks = _ctx.Picks.GetCurrentWeekPicks(vm.CurrentWeek).Where(p => p.Game.Id == scoredGame.Game.Id).ToList();
                // did we win?
                foreach (var pick in picks)
                    pick.IsWinner = pick.Team_Id == winner;
                // get the standard picks for the game
                var sPicks = _ctx.StandardPicks.GetCurrentWeekStandardPicks(vm.CurrentWeek).Where(p => p.Game.Id == scoredGame.Game.Id).ToList();
                // did we win?
                foreach (var sp in sPicks)
                    sp.IsWinner = sp.Team_Id == winner;
            }
            // persist the changes to the database
            _ctx.SaveChanges();
        }

        public void SaveStandardPicks(MyPredictViewModel vm, string userId)
        {
            var cwp = _ctx.StandardPicks.GetCurrentWeekStandardPicks(vm.CurrentWeek).ToList();
            //
            if (cwp.Count > 0)
            {
                foreach (var standardPick in cwp)
                {
                    standardPick.Active = false;
                    standardPick.UpdateBy = userId;
                    standardPick.UpdateDate = DateTime.Now;
                }
                //
            }
            //
            foreach (var pick in vm.MyPicks)
            {
                if (pick.Game != null && pick.Game.Id > 0 && pick.Team != null && pick.Team.Id > 0)
                {
                    var p = new StandardPick()
                                {
                                    Game_Id = pick.Game.Id,
                                    PointTotal = pick.PointTotal,
                                    Team_Id = pick.Team.Id,
                                    UpdateBy = userId,
                                    UpdateDate = DateTime.Now,
                                    Active = true
                                };
                    //
                    _ctx.AddToStandardPicks(p);
                }
            }
            //
            var cwpp = _ctx.StandardPlayoffSuperbowlPicks.GetCurrentWeekPlayoffStandardPicks(vm.CurrentWeek).ToList();
            if (cwpp.Count > 0)
            {
                foreach (var standardPick in cwpp)
                {
                    standardPick.Active = false;
                    standardPick.UpdateBy = userId;
                    standardPick.UpdateDate = DateTime.Now;
                } 
                //
            }
            //
            foreach (var pick in vm.MyPlayoffPicks)
            {
                var p = new StandardPlayoffSuperbowlPick()
                {
                    IsPlayoff = true,
                    Active = true,
                    Week = vm.CurrentWeek,
                    PointTotal = (_ctx.Games.GetMaxRegularSeasonWeekCurrentSeason() - vm.CurrentWeek) + 1,
                    Season_Id = vm.TheSeason.Id,
                    Team_Id = pick.Team.Id,
                    UpdateBy = userId,
                    UpdateDate = DateTime.Now
                };
                //
                _ctx.AddToStandardPlayoffSuperbowlPicks(p);
            }
            //
            var cwsp = _ctx.StandardPlayoffSuperbowlPicks.GetCurrentWeekSuperbowlStandardPicks(vm.CurrentWeek);
            if (cwpp.Count > 0)
            {
                foreach (var standardPick in cwsp)
                {
                    standardPick.Active = false;
                    standardPick.UpdateBy = userId;
                    standardPick.UpdateDate = DateTime.Now;
                }
                //
            }
            //
            foreach (var pick in vm.MySuperbowlPicks)
            {
                var p = new StandardPlayoffSuperbowlPick()
                {
                    IsSuperbowl = true,
                    Active = true,
                    Week = vm.CurrentWeek,
                    PointTotal = ((_ctx.Games.GetMaxRegularSeasonWeekCurrentSeason() - vm.CurrentWeek) + 1)*2,
                    Season_Id = vm.TheSeason.Id,
                    Team_Id = pick.Team.Id,
                    UpdateBy = userId,
                    UpdateDate = DateTime.Now
                };
                //
                _ctx.AddToStandardPlayoffSuperbowlPicks(p);
            }
            //
            _ctx.SaveChanges();
            //
        }

        public void SaveGame(GameViewModel gvm, string userId)
        {
            var game = _ctx.Games.Where(g => g.Id == gvm.Id).SingleOrDefault();
            game.GameDateTime = gvm.GameDateTime;
            game.UpdateBy = userId;
            game.UpdateDate = DateTime.Now;
            //
            _ctx.SaveChanges();
        }

        public void DeleteGame(int gameId)
        {
            // delete the game
            var game = _ctx.Games.Where(g => g.Id == gameId).Single();
            _ctx.Games.DeleteObject(game);
            _ctx.SaveChanges();
            // find all the picks related to this game and invalidate them
            var picks = _ctx.Picks.Where(p => p.Game.Id == gameId).ToList();
            var psPicks = _ctx.PlayoffSuperbowlPicks.Where(p => p.Team_Id == game.HomeTeam_Id || p.Team_Id == game.AwayTeam_Id).ToList();

            foreach (var pick in picks)
            {
                _ctx.Picks.DeleteObject(pick);
            }
            _ctx.SaveChanges();

            foreach (var playoffSuperbowlPick in psPicks)
            {
                if ((game.IsPlayoff && playoffSuperbowlPick.IsPlayoff) || (game.IsSuperbowl && playoffSuperbowlPick.IsSuperbowl))
                {
                    playoffSuperbowlPick.IsWinner = false;
                    playoffSuperbowlPick.PointTotal = 0;
                }
            }
            _ctx.SaveChanges();
            // recalculate the playoff and superbowl picks
            if (game.IsPlayoff || game.IsSuperbowl)
            {
                CalcSuperbowlPlayoffPicks(null);
            }
        }


        public void SavePick(MyPredictViewModel vm, string userId)
        {
            // do we have any picks for this week?
            foreach (var pvm in vm.MyPicks)
            {
                var p = _ctx.Picks.Where(pick => pick.Game.Id == pvm.Game.Id && pick.UserId == userId).SingleOrDefault();
                if (p != null && p.Team_Id > 0 && p.Game != null && p.Game.GameDateTime >= DateTime.Now)
                {
                    p.Game.Id = pvm.Game.Id;
                    p.PointTotal = pvm.PointTotal;
                    p.UserId = userId;
                    p.Team_Id = pvm.Team.Id;
                    p.UserId = userId;
                }
                else if (p == null)
                {
                    var game = _ctx.Games.Where(g => g.Id == pvm.Game.Id).SingleOrDefault();
                    if (game != null && pvm.Team != null && game.GameDateTime >= DateTime.Now)
                    {
                        var newPick = new Pick()
                                          {
                                              Game_Id = pvm.Game.Id,
                                              PointTotal = pvm.PointTotal,
                                              Team_Id = pvm.Team.Id,
                                              UserId = userId
                                          };
                        _ctx.AddToPicks(newPick);
                    }
                    else if (game != null && pvm.Team != null)
                    {
                        var newPick = new Pick()
                                          {
                                              Game_Id = pvm.Game.Id,
                                              PointTotal = 0,
                                              Team_Id = game.HomeTeam_Id,
                                              UserId = userId
                                          };
                        _ctx.AddToPicks(newPick);
                    }
                    //
                }
            }
            
            // save changes
            _ctx.SaveChanges();
            //
            if (vm.MyPlayoffPicks.Count > 0)
            {
                var pPicks = _ctx.PlayoffSuperbowlPicks.GetCurrentWeekPlayoffPicks(vm.CurrentWeek, userId).ToList();
                var delPicks = from p in pPicks
                               where !(from ps in vm.MyPlayoffPicks select ps.Team.Id).ToList().Contains(p.Team_Id) 
                               select p;

                foreach (var playoffSuperbowlPick in delPicks)
                {
                    _ctx.PlayoffSuperbowlPicks.DeleteObject(playoffSuperbowlPick);
                }

                var addPicks = from p in vm.MyPlayoffPicks
                               where !(from ps in pPicks select ps.Team_Id).ToList().Contains(p.Team.Id)
                               select p;
                foreach (var superbowlPlayoffPickViewModel in addPicks)
                {
                    _ctx.AddToPlayoffSuperbowlPicks(new PlayoffSuperbowlPick()
                    {
                        IsPlayoff = true,
                        IsSuperbowl = false,
                        Team_Id = superbowlPlayoffPickViewModel.Team.Id,
                        PointTotal =
                            (vm.TheSeason.NumberOfWeeks - vm.CurrentWeek) + 1,
                        Season_Id = vm.TheSeason.Id,
                        UserId = userId,
                        Week = vm.CurrentWeek,
                        IsWinner = false
                    });
                }

                //
                _ctx.SaveChanges();
            }

            if (vm.MySuperbowlPicks.Count > 0)
            {
                var sPicks = _ctx.PlayoffSuperbowlPicks.GetCurrentWeekSuperbowlPicks(vm.CurrentWeek, userId).ToList();
                var delPicks = from p in sPicks
                               where!(from ps in vm.MySuperbowlPicks select ps.Team.Id).Contains(p.Team_Id)
                               select p;
                foreach (var playoffSuperbowlPick in delPicks)
                {
                    _ctx.PlayoffSuperbowlPicks.DeleteObject(playoffSuperbowlPick);
                }

                var addPicks = from p in vm.MySuperbowlPicks
                               where !(from ps in sPicks select ps.Team_Id).Contains(p.Team.Id)
                               select p;

                foreach (var superbowlPlayoffPickViewModel in addPicks)
                {
                    _ctx.AddToPlayoffSuperbowlPicks(new PlayoffSuperbowlPick()
                    {
                        IsPlayoff = false,
                        IsSuperbowl = true,
                        Team_Id = superbowlPlayoffPickViewModel.Team.Id,
                        PointTotal =
                            ((vm.TheSeason.NumberOfWeeks - vm.CurrentWeek) + 1)*2,
                        Season_Id = vm.TheSeason.Id,
                        UserId = userId,
                        Week = vm.CurrentWeek,
                        IsWinner = false
                    });
                }
                // save changes
                _ctx.SaveChanges();
            }
        }

        public void SetupNewUserMidSeason(string userId)
        {
            // get the standard picks for the weeks prior
            var sPicks = _ctx.StandardPicks.Where(p => p.Game.GameDateTime < DateTime.Now && p.Active != null && p.Active.Value && p.Game.Season.IsCurrent).ToList();
            var sspPicks = _ctx.StandardPlayoffSuperbowlPicks.Where(p => p.Active != null && p.Active.Value && p.Season.IsCurrent).ToList();
            foreach (var sp in sPicks)
            {
                var pick = new Pick()
                               {
                                   Game = sp.Game,
                                   PointTotal = sp.PointTotal,
                                   Team_Id = sp.Team_Id,
                                   UpdateBy = "new_user_add",
                                   UpdateDate = DateTime.Now,
                                   UserId = userId,
                                   IsWinner = sp.IsWinner
                               };
                // add the new pick to the context
                _ctx.AddToPicks(pick);
            }
            // playoff
            var season = _ctx.Seasons.Where(s => s.IsCurrent).SingleOrDefault();
            foreach (var sp in sspPicks)
            {
                var pick = new PlayoffSuperbowlPick()
                               {
                                   IsPlayoff = sp.IsPlayoff,
                                   IsSuperbowl = sp.IsSuperbowl,
                                   Week = sp.Week,
                                   PointTotal = sp.PointTotal,
                                   UserId = userId,
                                   Season = season,
                                   Team_Id = sp.Team_Id
                               };
                // add to context
                _ctx.AddToPlayoffSuperbowlPicks(pick);
            }
            // save
            _ctx.SaveChanges();
        }

        public List<TeamViewModel> GetTeams()
        {
            // setup auto-mapper
            AutoMapperConfiguration.Setup(_ctx);
            return Mapper.Map<List<Team>, List<TeamViewModel>>(_ctx.Teams.ToList());
        }

        public List<TeamViewModel> SearchTeams(string term)
        {
            var teams = GetTeams();
            return teams.Where(t => t.TeamName.ToUpper().Contains(term.ToUpper())).ToList();
        }

        public List<SeasonSearchViewModel> SearchSeasons(string term)
        {
            AutoMapperConfiguration.Setup(_ctx);
            var season = _ctx.Seasons.Where(s => SqlFunctions.StringConvert((double) s.StartDate.Year).Contains(term)).ToList();
            var svm = Mapper.Map<List<Season>, List<SeasonSearchViewModel>>(season);
            return svm;
        }

        public void SaveSeason(int id, int sWe, DateTime ssD, int dId)
        {
            Season season;
            //            
            if (dId > 0)
            {
                // get the current season, and set it inactive, and inactivate all the games associated with
                // the season
                var dDeason = _ctx.Seasons.Where(s => s.Id == dId).Single();
                dDeason.IsCurrent = false;
                _ctx.SaveChanges();                
            }
            //
            if (id > 0)
            {
                season = _ctx.Seasons.Where(s => s.Id == id).SingleOrDefault();
                season.IsCurrent = dId > 0 || (season.IsCurrent && dId == 0);
                season.NumberOfWeeks = sWe;
                season.StartDate = ssD;
                season.EndDate = ssD.AddDays(sWe * 7);                
            }
            else
            {
                season = new Season()
                {
                    EndDate = ssD.AddDays(sWe * 7),
                    StartDate = ssD,
                    NumberOfWeeks = sWe,
                    IsCurrent = true
                };
                _ctx.AddToSeasons(season);
            }
            //
            _ctx.SaveChanges();
            //
        }

        public void CalcSuperbowlPlayoffPicks(int? seasonId)
        {
            var inQuestionSeasonId = seasonId.HasValue ? seasonId.Value : _ctx.Seasons.Where(s => s.IsCurrent).Select(s => s.Id).SingleOrDefault();
            // get the playoff/superbowl picks
            var games = _ctx.Games.Where(g => (g.IsPlayoff || g.IsSuperbowl) && g.Season.Id == inQuestionSeasonId).ToList();
            var finalRegularSeasonGameWeek = _ctx.Games.GetMaxRegularSeasonWeekCurrentSeason();

            foreach (var game in games)
            {
                Game game1 = game;
                var picks =
                    _ctx.PlayoffSuperbowlPicks.Where(
                        p => p.IsPlayoff == game1.IsPlayoff && p.IsSuperbowl == game1.IsSuperbowl
                             && (p.Team_Id == game1.HomeTeam_Id || p.Team_Id == game1.AwayTeam_Id));
                //
                foreach (var playoffSuperbowlPick in picks)
                {
                    playoffSuperbowlPick.IsWinner = true;
                    if (playoffSuperbowlPick.IsPlayoff && !playoffSuperbowlPick.IsSuperbowl)
                    {
                        playoffSuperbowlPick.PointTotal = (finalRegularSeasonGameWeek - playoffSuperbowlPick.Week) + 1;
                    }
                    else if (playoffSuperbowlPick.IsSuperbowl && !playoffSuperbowlPick.IsPlayoff)
                    {
                        playoffSuperbowlPick.PointTotal = ((finalRegularSeasonGameWeek - playoffSuperbowlPick.Week) + 1)*2;                        
                    }
                }
            }
            // save the changes back to the database
            _ctx.SaveChanges();
        }

        public GameViewModel AddGame(int hId, int aId, DateTime gDt, bool isPlayoff, bool isSuperbowl, int week)
        {
            var game = new Game()
                           {
                               HomeTeam_Id = hId,
                               AwayTeam_Id = aId,
                               GameDateTime = gDt,
                               IsPlayoff = isPlayoff,
                               IsSuperbowl = isSuperbowl,
                               Week = week,
                               Season = _ctx.Seasons.GetCurrentSeason().SingleOrDefault(),
                               HomeTeamScore = 0,
                               AwayTeamScore = 0
                           };
            //
            _ctx.AddToGames(game);
            _ctx.SaveChanges();

            // setup auto-mapper
            AutoMapperConfiguration.Setup(_ctx);

            // calculate the playoff and superbowl picks only if you are adding a playoff or superbowl game
            if (isSuperbowl || isPlayoff)
            {
                // calculate the superbowl and playoff picks
                CalcSuperbowlPlayoffPicks(null);
            }
            // return the game we just added
            return Mapper.Map<Game, GameViewModel>(game);

        }
    }
}