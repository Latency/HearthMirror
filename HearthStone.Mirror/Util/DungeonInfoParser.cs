using System;
using System.Collections.Generic;
using HearthStone.Mirror.Enums;
using HearthStone.Mirror.Objects;


namespace HearthStone.Mirror.Util
{
	internal class DungeonInfoParser : DungeonInfo
	{
		private readonly dynamic _map;

		public DungeonInfoParser(dynamic map)
		{
			_map = map;
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSSES_DEFEATED, out _bossesDefeated);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CARD_LIST, out _dbfIds);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_DECK_CLASS, out _heroCardClass);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_PASSIVE_BUFF_LIST, out _passiveBuffs);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_FIGHT, out _nextBossDbfId);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_A, out _lootA);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_B, out _lootB);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_OPTION_C, out _lootC);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_TREASURE_OPTION, out _treasure);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_LOOT_HISTORY, out _lootHistory);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_IS_RUN_ACTIVE, out int active);
			_runActive = active > 0;
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_BOSS_LOST_TO, out _bossesLostTo);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_LOOT, out _playerChosenLoot);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_PLAYER_CHOSEN_TREASURE, out _playerChosenTreasure);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_NEXT_BOSS_HEALTH, out _nextBossHealth);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_HERO_HEALTH, out _heroHealth);
			GetValue(GameSaveKeySubkeyId.DUNGEON_CRAWL_CARDS_ADDED_TO_DECK_MAP, out _cardsAddedToDeck);
		}

		private bool GetValue(GameSaveKeySubkeyId key, out int value)
		{
			value = 0;
			if (!GetValue(key, out List<int> values)) return false;
			value = values[0];
			return true;
		}

		private bool GetValue<T>(GameSaveKeySubkeyId key, out List<T> values)
		{
			values = null;
			try
			{
				var subIndex = Reflection.GetKeyIndex(_map, (int)key);
				if(subIndex == -1)
					return false;
				var list = _map["valueSlots"][subIndex]?[GetTypeKey(typeof(T))];
				if(list == null)
					return false;
				var size = (int)list["_size"];
				if(size <= 0)
					return false;
				values = new List<T>();
				var items = list["_items"];
				for(var i = 0; i < size; i++)
					values.Add((T)items[i]);
				return true;
			}
			catch(Exception)
			{
				return false;
			}
		}

		private static string GetTypeKey(Type t)
		{
			return t == typeof(int) ? "_IntValue"
				: (t == typeof(double) ? "_FloatValue"
				: (t == typeof(string) ? "_StringValue"
				: null));
		}

	}
}
