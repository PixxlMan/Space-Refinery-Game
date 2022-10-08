using System.Runtime.Serialization;

namespace Space_Refinery_Game
{
	public interface ISetting
	{
		public Guid Guid { get; }

		public bool Dirty { get; }

		ISettingOptions Options { get; set; }

		public static abstract ISetting Create();

		public void DoUI();

		public void Accept();

		public void Cancel();

		public void SetUp();

		public event Action<ISetting> AcceptedSettingChange;

		public event Action<ISetting> SettingChanged;
	}
}