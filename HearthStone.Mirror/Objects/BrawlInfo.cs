namespace HearthStone.Mirror.Objects
{
	public class BrawlInfo
	{
		public int MaxWins { get; set; }
		public int MaxLosses { get; set; }
		public bool IsSessionBased => MaxWins > 0 || MaxLosses > 0;
		public int Wins { get; set; }
		public int Losses { get; set; }
		public int GamesPlayed { get; set; }
		public int WinStreak { get; set; }
	}
}
