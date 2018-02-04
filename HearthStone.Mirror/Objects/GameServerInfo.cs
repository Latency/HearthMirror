namespace HearthStone.Mirror.Objects
{
	public class GameServerInfo
	{
		public string Address { get; set; }
		public string AuroraPassword { get; set; }
		public long ClientHandle { get; set; }
		public int GameHandle { get; set; }
		public int Mission { get; set; }
		public int Port { get; set; }
		public bool Resumable { get; set; }
		public bool SpectatorMode { get; set; }
		public string SpectatorPassword { get; set; }
		public string Version { get; set; }
	}
}