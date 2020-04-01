﻿using System;
using System.Collections.Generic;
using System.Linq;
using W3ChampionsStatisticService.MatchEvents;

namespace W3ChampionsStatisticService.Matches
{
    public class Matchup
    {
        public string Map { get; set; }
        public string Id { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public GameMode GameMode { get; set; }
        public IList<Team> Teams { get; set; } = new List<Team>();

        public Matchup(MatchFinishedEvent matchFinishedEvent)
        {
            var data = matchFinishedEvent.data;
            Map = data.mapInfo.name;
            Id = data.gameId;

            StartTime = DateTimeOffset.Now;
            Duration = TimeSpan.FromSeconds(data.mapInfo.elapsedGameTimeTotalSeconds);

            var winners = data.players.Where(p => p.won);
            var loosers = data.players.Where(p => !p.won);

            Teams.Add(CreateTeam(loosers));
            Teams.Add(CreateTeam(winners));
        }

        private static Team CreateTeam(IEnumerable<PlayerRaw> loosers)
        {
            var team = new Team();
            team.Players.AddRange(CreatePlayerArray(loosers));
            return team;
        }

        private static IEnumerable<PlayerOverviewMatches> CreatePlayerArray(IEnumerable<PlayerRaw> players)
        {
            return players.Select(w => new PlayerOverviewMatches {
                Name = w.battleTag.Split("#")[0],
                BattleTag = w.battleTag.Split("#")[1]
            });
        }
    }
}