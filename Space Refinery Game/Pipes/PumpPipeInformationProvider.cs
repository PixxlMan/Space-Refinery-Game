using Space_Refinery_Engine;

namespace Space_Refinery_Game
{
	public sealed class PumpPipeInformationProvider : PipeInformationProvider
	{
		public PumpPipeInformationProvider(PumpPipe pipe) : base(pipe)
		{
		}

		public override void InformationUI()
		{
			base.InformationUI();

			var pumpPipe = (PumpPipe)Pipe;

			pumpPipe.ContainerA.DoUIInspectorReadonly();

			pumpPipe.ContainerB.DoUIInspectorReadonly();
		}
	}
}