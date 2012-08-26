using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PPF.Models;
using PPF.Models.ViewModels;

namespace PPF.Tests.Services
{
    [TestClass]
    public class SeasonService
    {
        [TestMethod]
        public void Are_Auto_Mappings_Correct()
        {
            var ctx = new ppfEntities();
            PPF.Models.AutoMapperConfiguration.Setup(ctx);
            var games = ctx.Games.Where(g => g.Season.IsCurrent).ToList();
            var picks = ctx.Picks.Where(p => p.Game.Season.IsCurrent).ToList();
            var gvm = Mapper.Map<List<Game>, List<GameViewModel>>(games);
            Assert.IsNotNull(gvm[0].AwayTeam);
        }
    }
}
