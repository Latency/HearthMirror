using System.Collections.Generic;


namespace HearthStone.Mirror.Objects
{
	public class ArenaInfo
	{
		public Deck Deck { get; set; }
		public int Losses { get; set; }
		public int Wins { get; set; }
		public int CurrentSlot { get; set; }
		public List<RewardData> Rewards { get; set; }
		public int Season { get; set; }
	}
}