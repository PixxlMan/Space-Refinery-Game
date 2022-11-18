using System.Runtime.Serialization;

namespace Space_Refinery_Game
{
	public interface ISetting
	{
		public Guid Guid { get; init; }

		public bool Dirty { get; }

		ISettingOptions Options { get; set; }

		public void DoUI();

		public void Accept();

		public void Cancel();

		public void SetUp();

		public event Action<ISetting> AcceptedSettingChange;

		public event Action<ISetting> SettingChanged;
	}

	// ICreatableSetting is necessary to house the static abstract Create method because .Net 7 throws a fit if it's part of ISetting, and you then try to use ISetting as type argument...
	public interface ICreatableSetting : ISetting
	{
		public static abstract ISetting Create();
	}
}