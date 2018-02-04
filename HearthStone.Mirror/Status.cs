using System;
using HearthStone.Mirror.Enums;


namespace HearthStone.Mirror
{
	public class Status
	{
		private Status(MirrorStatus status)
		{
			MirrorStatus = status;
		}

		private Status(Exception ex)
		{
			MirrorStatus = MirrorStatus.Error;
			Exception = ex;
		}

		public MirrorStatus MirrorStatus { get; }
		public Exception Exception { get; }

		public static Status GetStatus()
		{
			try
			{
				return new Mirror {ImageName = "Hearthstone"}.View == null ? new Status(MirrorStatus.ProcNotFound) : new Status(MirrorStatus.Ok);
			}
			catch(Exception e)
			{
				return new Status(e);
			}
		}
	}
}