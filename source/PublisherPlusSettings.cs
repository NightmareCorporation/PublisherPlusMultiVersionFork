using Verse;

namespace PublisherPlus
{
	public class PublisherPlusSettings : ModSettings
	{

		public static bool useGitIgnore;

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref useGitIgnore, "useGitIgnore", false);
		}
	}
}
