using FixedPrecision;
using FXRenderer;
using Space_Refinery_Engine;
using Space_Refinery_Game;
using Space_Refinery_Utilities;

namespace InfiltrationGame;

public static class Initialization
{
	public static void Initialize(GameData gameData)
	{
		Starfield.CreateAndAdd(gameData.GraphicsWorld);

		Pipe.Create(PipeType.PipeTypes["Straight Pipe"], new Transform(new(0, 0, 0), QuaternionFixedDecimalInt4.CreateFromYawPitchRoll(0, 0, 0)), gameData, gameData.Game.GameReferenceHandler);

		gameData.Settings.RegisterToSettingValue<SwitchSettingValue>("Use Celcius", (v) => FormatUnit.UseCelcius = v);
		gameData.Settings.RegisterToSettingValue<SwitchSettingValue>("Use Pascal", (v) => FormatUnit.UsePascal = v);

		Player.Create(gameData);
	}
}
