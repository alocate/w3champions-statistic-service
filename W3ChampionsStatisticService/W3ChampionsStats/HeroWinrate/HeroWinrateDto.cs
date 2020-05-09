using System.Collections.Generic;
using System.Linq;
using W3ChampionsStatisticService.PlayerStats.RaceOnMapVersusRaceStats;

namespace W3ChampionsStatisticService.W3ChampionsStats.HeroWinrate
{
    public class HeroWinrateDto
    {
        public HeroWinrateDto(List<HeroWinRatePerHero> stats, string opFirst, string opSecond, string opThird)
        {
            if (opSecond == "all")
            {
                Winrate = CombineWinrates(stats, $"{opFirst}", $"{opFirst}_all_all");
            }
            else if (opThird == "all")
            {
                Winrate = CombineWinrates(stats, $"{opFirst}_{opSecond}", $"{opFirst}_{opSecond}_all");
            }
            else
            {
                Winrate = stats.SingleOrDefault()?.WinRates.SingleOrDefault(s => s.HeroCombo == $"{opFirst}_{opSecond}_{opThird}")
                          ?? new HeroWinRate { HeroCombo = $"{opFirst}_{opSecond}_{opThird}" };
            }
        }

        private HeroWinRate CombineWinrates(List<HeroWinRatePerHero> stats, string startsWithString, string comboString)
        {
            var winrates = stats.SelectMany(s => s.WinRates).Where(s => s.HeroCombo.StartsWith(startsWithString)).ToList();
            var wins = winrates.Sum(w => w.WinLoss.Wins);
            var losses = winrates.Sum(w => w.WinLoss.Losses);
            return new HeroWinRate
            {
                HeroCombo = comboString,
                WinLoss = new WinLoss
                {
                    Wins = wins,
                    Losses = losses
                }
            };
        }

        public HeroWinRate Winrate { get; set; }
    }
}