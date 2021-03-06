using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using W3ChampionsStatisticService.CommonValueObjects;
using W3ChampionsStatisticService.Matches;
using W3ChampionsStatisticService.PadEvents;

namespace WC3ChampionsStatisticService.UnitTests
{
    [TestFixture]
    public class MatchupRepoTests : IntegrationTestBase
    {
        [SetUp]
        public async Task SetupSut()
        {
            var matchRepository = new MatchRepository(MongoClient);
            await matchRepository.EnsureIndices();
        }

        [Test]
        public async Task LoadAndSave()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.Load();

            Assert.AreEqual(2, matches.Count);
        }

        [Test]
        public async Task LoadAndSearch()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();
            matchFinishedEvent1.match.players[1].battleTag = "KOMISCHER#123";
            matchFinishedEvent1.match.players[1].won = true;
            matchFinishedEvent1.match.players[0].won = false;
            matchFinishedEvent1.match.gateway = GateWay.America;
            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent2.match.gateway = GateWay.America;
            matchFinishedEvent2.match.season = 1;

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));

            var matches = await matchRepository.LoadFor("KOMISCHER#123");

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("KOMISCHER#123", matches[0].Teams[0].Players[0].BattleTag);
        }

        [Test]
        public async Task LoadAndSearch_InvalidString()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();
            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent1.match.players[1].battleTag = "peter#123";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));

            var matches = await matchRepository.LoadFor("asd#123");

            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public async Task Upsert()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent = TestDtoHelper.CreateFakeEvent();

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent));
            var matches = await matchRepository.Load();

            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public async Task CountFor()
        {
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"https://www.test.w3champions.com:{i}/login");
            }

            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent3 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[1].battleTag = "wolf#456";

            matchFinishedEvent2.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "wolf#456";
            matchFinishedEvent2.match.players[1].battleTag = "peter#123";

            matchFinishedEvent3.match.season = 1;
            matchFinishedEvent3.match.players[0].battleTag = "notFound";
            matchFinishedEvent3.match.players[1].battleTag = "notFound2";

            var matchup = Matchup.Create(matchFinishedEvent1);
            await matchRepository.Insert(matchup);
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var count = await matchRepository.CountFor(matchup.Teams[0].Players[0].BattleTag);

            Assert.AreEqual(2, count);
        }

        [Test]
        public async Task SearchForPlayerAndOpponent()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[1].battleTag = "wolf#456";
            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "peter#123";
            matchFinishedEvent2.match.players[1].battleTag = "ANDERER#456";
            matchFinishedEvent2.match.season = 1;

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.LoadFor("peter#123", "wolf#456");
            var count = await matchRepository.CountFor("peter#123", "wolf#456");

            Assert.AreEqual(1, count);
            Assert.AreEqual("peter#123", matches.Single().Teams.First().Players.Single().BattleTag);
            Assert.AreEqual("wolf#456", matches.Single().Teams.Last().Players.Single().BattleTag);
        }

        [Test]
        public async Task SearchForPlayerAndOpponent_FilterByGateway()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.gateway = GateWay.America;
            matchFinishedEvent2.match.gateway = GateWay.Europe;

            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[1].battleTag = "wolf#456";

            matchFinishedEvent2.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "peter#123";
            matchFinishedEvent2.match.players[1].battleTag = "ANDERER#456";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.LoadFor("peter#123", null, GateWay.America);
            var count = await matchRepository.CountFor("peter#123", null, GateWay.America);

            Assert.AreEqual(1, count);
            Assert.AreEqual("peter#123", matches.Single().Teams.First().Players.Single().BattleTag);
        }

        [TestCase(1, "ANDERER#456")]
        [TestCase(0, "wolf#456")]
        public async Task SearchForPlayerAndOpponent_FilterBySeason(int season, string playerTwo)
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.season = 0;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[1].battleTag = "wolf#456";

            matchFinishedEvent2.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "peter#123";
            matchFinishedEvent2.match.players[1].battleTag = "ANDERER#456";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.LoadFor("peter#123", null, season : season);
            var count = await matchRepository.CountFor("peter#123", null, season : season);

            Assert.AreEqual(1, count);
            Assert.AreEqual("peter#123", matches.Single().Teams.First().Players.Single().BattleTag);
            Assert.AreEqual(playerTwo, matches.Single().Teams.Last().Players.Single().BattleTag);  
        }

        [TestCase(1)]
        [TestCase(0)]
        public async Task SearchForPlayerAndOpponent_FilterBySeason_NoResults(int season)
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.season = season ^ 1;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[1].battleTag = "ANDERER#456";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            var matches = await matchRepository.LoadFor("peter#123", null, season : season);
            var count = await matchRepository.CountFor("peter#123", null, season : season);

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task SearchForPlayerAndOpponent_2v2_SameTeam()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFake2v2Event();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[1].battleTag = "wolf#456";
            matchFinishedEvent1.match.players[2].battleTag = "LostTeam1#456";
            matchFinishedEvent1.match.players[3].battleTag = "LostTeam2#456";

            matchFinishedEvent2.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "peter#123";
            matchFinishedEvent2.match.players[1].battleTag = "ANDERER#456";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.LoadFor("peter#123@10", "wolf#456");
            var count = await matchRepository.CountFor("peter#123@10", "wolf#456");

            Assert.AreEqual(0, count);
        }

        [Test]
        public async Task SearchForPlayerAndOpponent_2v2()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFake2v2Event();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();
            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[0].team = 0;
            matchFinishedEvent1.match.players[1].battleTag = "LostTeam1#456";
            matchFinishedEvent1.match.players[1].team = 0;

            matchFinishedEvent1.match.players[2].battleTag = "wolf#456";
            matchFinishedEvent1.match.players[2].team = 1;
            matchFinishedEvent1.match.players[3].battleTag = "LostTeam2#456";
            matchFinishedEvent1.match.players[3].team = 2;

            matchFinishedEvent2.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "peter#123";
            matchFinishedEvent2.match.players[1].battleTag = "ANDERER#456";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.LoadFor("peter#123", "wolf#456");
            var count = await matchRepository.CountFor("peter#123", "wolf#456");

            Assert.AreEqual(1, count);
            Assert.AreEqual("peter#123", matches.Single().Teams[0].Players[0].BattleTag);
            Assert.AreEqual("wolf#456", matches.Single().Teams[1].Players[0].BattleTag);
        }

        [Test]
        public async Task SearchForPlayerAndOpponent_2v2And1V1()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFake2v2Event();
            var matchFinishedEvent2 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.season = 1;
            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.match.players[0].team = 0;
            matchFinishedEvent1.match.players[1].battleTag = "LostTeam1#456";
            matchFinishedEvent1.match.players[1].team = 0;

            matchFinishedEvent1.match.players[2].battleTag = "wolf#456";
            matchFinishedEvent1.match.players[2].team = 1;
            matchFinishedEvent1.match.players[3].battleTag = "LostTeam2#456";
            matchFinishedEvent1.match.players[3].team = 1;

            matchFinishedEvent2.match.season = 1;
            matchFinishedEvent2.match.players[0].battleTag = "peter#123";
            matchFinishedEvent2.match.players[1].battleTag = "wolf#456";

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            await matchRepository.Insert(Matchup.Create(matchFinishedEvent2));
            var matches = await matchRepository.LoadFor("peter#123", "wolf#456");
            var count = await matchRepository.CountFor("peter#123", "wolf#456");

            Assert.AreEqual(2, count);
            Assert.AreEqual("peter#123", matches[0].Teams[0].Players[0].BattleTag);
            Assert.AreEqual("peter#123", matches[1].Teams[0].Players[0].BattleTag);
        }

        [Test]
        public async Task SearchForGameMode2v2_NotFound()
        {
            var matchRepository = new MatchRepository(MongoClient);
            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            matchFinishedEvent1.match.gameMode = GameMode.GM_1v1;

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            var matches = await matchRepository.Load(GateWay.Undefined, GameMode.GM_2v2_AT);

            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public async Task SearchForGameMode2v2_Found()
        {
            var matchRepository = new MatchRepository(MongoClient);
            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            matchFinishedEvent1.match.gameMode = GameMode.GM_2v2_AT;

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            var matches = await matchRepository.Load(GateWay.Undefined, GameMode.GM_2v2_AT);

            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public async Task SearchForGameMode2v2_LoadDefault()
        {
            var matchRepository = new MatchRepository(MongoClient);
            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();
            matchFinishedEvent1.match.gameMode = GameMode.GM_2v2_AT;

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));
            var matches = await matchRepository.Load();

            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public async Task ReforgedIconGetsReplaced()
        {
            var matchRepository = new MatchRepository(MongoClient);

            var matchFinishedEvent1 = TestDtoHelper.CreateFakeEvent();

            matchFinishedEvent1.match.players[0].battleTag = "peter#123";
            matchFinishedEvent1.result.players[0].battleTag = "peter#123";
            matchFinishedEvent1.result.players[0].heroes = new List<Hero> { new Hero { icon = "jainasea"}};

            await InsertMatchEvent(matchFinishedEvent1);

            await matchRepository.Insert(Matchup.Create(matchFinishedEvent1));

            var result = await matchRepository.LoadDetails(matchFinishedEvent1.Id);

            Assert.AreEqual("archmage", result.PlayerScores[0].Heroes[0].icon);
        }
    }
}