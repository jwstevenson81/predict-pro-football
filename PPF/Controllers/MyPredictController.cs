using System.Collections.Generic;
using System.Data.Objects;
using System.Web.Mvc;
using PPF.Models;
using PPF.Models.ViewModels;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PPF.Controllers
{
    public class MyPredictController : Controller
    {
        private readonly SeasonService _svc;

        public MyPredictController()
        {
            _svc = new SeasonService();
        }
    
        [Authorize]
        public ActionResult Index()
        {
            return View(_svc.GetCurrentSeason(User.Identity.Name));
        }

        [Authorize]
        public ActionResult Tab(int week)
        {
            var vm = _svc.SetupWeek(week, User.Identity.Name);
            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public ActionResult AcceptPicks(MyPredictViewModel vm, FormCollection formValues)
        {
            var pPicks = (from formValue in formValues.AllKeys
                           where formValue.Contains("_team_playoff_") && bool.Parse(Request.Form.GetValues(formValue)[0])
                           let idx = formValue.LastIndexOf("_")
                           select new TeamViewModel() {Id = int.Parse(formValue.Substring(idx + 1, formValue.Length - (idx + 1)))}
                           into team select new SuperbowlPlayoffPickViewModel() { IsPlayoff = true, Team = team, UserId = User.Identity.Name }).ToList();

            var sPicks = (from formValue in formValues.AllKeys
                          where formValue.Contains("_team_superbowl_") && bool.Parse(Request.Form.GetValues(formValue)[0])
                            let idx = formValue.LastIndexOf("_")
                            select new TeamViewModel() { Id = int.Parse(formValue.Substring(idx + 1, formValue.Length - (idx + 1))) }
                            into team
                            select new SuperbowlPlayoffPickViewModel() { IsPlayoff = true, Team = team, UserId = User.Identity.Name}).ToList();


            vm.MyPlayoffPicks = pPicks;
            vm.MySuperbowlPicks = sPicks;

            if (vm.AreStandardPicks)
                _svc.SaveStandardPicks(vm, User.Identity.Name);
            else
            {
                _svc.SavePick(vm, User.Identity.Name);
                if (User.IsInRole("Admins"))
                    _svc.UpdateWeeklyScores(vm);
            }


            return Json("success");

        }

        [Authorize(Roles = "Admins")]
        [HttpPost]
        public JsonResult CalcPsPicks()
        {
            _svc.CalcSuperbowlPlayoffPicks(null);
            return Json("success");
        }


        [Authorize(Roles = "Admins")]
        [HttpPost]
        public JsonResult SaveGame(string gameId, string gameDateTime)
        {
            var gvm = new GameViewModel() {Id = int.Parse(gameId), GameDateTime = DateTime.Parse(gameDateTime)};
            _svc.SaveGame(gvm, User.Identity.Name);
            return Json("success");
        }

        [Authorize(Roles = "Admins")]
        [HttpPost]
        public JsonResult DeleteGame(string gameId)
        {
            var gid = int.Parse(gameId);
            _svc.DeleteGame(gid);
            return Json("success");
        }

        
        [Authorize(Roles = "Admins")]
        [HttpPost]
        public JsonResult SaveSeason(string startDate, string weeks, int id, int dId)
        {
            var ssD = DateTime.Parse(startDate);
            var sWe = int.Parse(weeks);
            _svc.SaveSeason(id, sWe, ssD, dId);
            return Json("success");
        }

        [Authorize(Roles = "Admins")]
        [HttpPost]
        public JsonResult AddGame(int homeTeamId, int awayTeamId, string gameDateTime, string gameType, int week)
        {
            var isPlayoff = false;
            var isSuperbowl = false;
            if (gameType.ToUpper().Equals("P"))
                isPlayoff = true;
            else if (gameType.ToUpper().Equals("S"))
                isSuperbowl = true;
            //
            var gDt = DateTime.Parse(gameDateTime);
            var team = _svc.AddGame(homeTeamId, awayTeamId, gDt, isPlayoff, isSuperbowl, week);
            return Json(team);
        }

        [Authorize(Roles = "Admins")]
        [HttpPost]
        public JsonResult SearchSeason(string term)
        {
            return Json(_svc.SearchSeasons(term));
        }

        [Authorize(Roles="Admins")]
        [HttpPost]
        public JsonResult SearchTeams(string term)
        {
            return Json(_svc.SearchTeams(term));
        }

        [Authorize(Roles="Admins")]
        [HttpPost]
        public JsonResult GetCurrentSeason()
        {
            return Json(_svc.GetCurrentSeason());

        }
    }
}
