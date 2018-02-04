using System.Collections.Generic;


namespace HearthStone.Mirror.Objects
{
	public class DungeonInfo
	{
		protected List<int> _bossesDefeated;
		protected int _bossesLostTo;
		protected int _nextBossHealth;
		protected int _heroHealth;
		protected List<int> _dbfIds;
		protected List<int> _cardsAddedToDeck;
		protected int _heroCardClass;
		protected List<int> _passiveBuffs;
		protected int _nextBossDbfId;
		protected List<int> _lootA;
		protected List<int> _lootB;
		protected List<int> _lootC;
		protected List<int> _treasure;
		protected List<int> _lootHistory;
		protected int _playerChosenLoot;
		protected int _playerChosenTreasure;
		protected bool _runActive;

		public List<int> BossesDefeated => _bossesDefeated;
		public int BossesLostTo => _bossesLostTo;
		public int NextBossHealth => _nextBossHealth;
		public int HeroHealth => _heroHealth;
		public List<int> DbfIds => _dbfIds;
		public List<int> CardsAddedToDeck => _cardsAddedToDeck;
		public int HeroCardClass => _heroCardClass;
		public List<int> PassiveBuffs => _passiveBuffs;
		public int NextBossDbfId => _nextBossDbfId;
		public List<int> LootA => _lootA;
		public List<int> LootB => _lootB;
		public List<int> LootC => _lootC;
		public List<int> Treasure => _treasure;
		public List<int> LootHistory => _lootHistory;
		public int PlayerChosenLoot => _playerChosenLoot;
		public int PlayerChosenTreasure => _playerChosenTreasure;
		public bool RunActive => _runActive;

		//protected Dictionary<int, int> _classBossWinos;
		//protected Dictionary<int, int> _classRunWins;
		//public Dictionary<int, int> ClassBossWinos => _classBossWinos;
		//public Dictionary<int, int> ClassRunWins => _classRunWins;
	}
}
