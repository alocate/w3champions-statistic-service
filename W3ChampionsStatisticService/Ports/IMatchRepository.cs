﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using W3ChampionsStatisticService.Matches;

namespace W3ChampionsStatisticService.Ports
{
    public interface IMatchRepository
    {
        Task<List<Matchup>> Load(DateTimeOffset since = default, int pageSize = 100);
        Task Insert(Matchup matchup);
    }
}