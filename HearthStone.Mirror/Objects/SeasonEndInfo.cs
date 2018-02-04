using System.Collections.Generic;


namespace HearthStone.Mirror.Objects
{
	public class SeasonEndInfo
	{
		public int BonusStars { get; }
		public int BoostedRank { get; }
		public int ChestRank { get; }
		public bool IsWild { get; }
		public int LegendRank { get; }
		public int Rank { get; }
		public int SeasonId { get; }
		public List<RewardData> RankedRewards { get; }

		public SeasonEndInfo(int bonusStars, int boostedRank, int chestRank, bool isWild, int legendRank, int rank, int seasonId, List<RewardData> rankedRewards)
		{
			BonusStars = bonusStars;
			BoostedRank = boostedRank;
			ChestRank = chestRank;
			IsWild = isWild;
			LegendRank = legendRank;
			Rank = rank;
			SeasonId = seasonId;
			RankedRewards = rankedRewards;
		}
	}
}