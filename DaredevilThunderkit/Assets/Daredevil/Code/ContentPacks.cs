using System.Collections;
using R2API.ScriptableObjects;
using RoR2.ContentManagement;

namespace Daredevil
{
	internal class ContentPacks : IContentPackProvider
	{
		internal ContentPack contentPack;
		internal R2APISerializableContentPack serializableContentPack;

		public string identifier => "com.nayDPz.Daredevil";

		public void Initialize()
		{
			ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
			serializableContentPack = DaredevilMain.serializableContentPack;
		}

		private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
		{
			addContentPackProvider(this);
		}

		public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
		{
			contentPack = serializableContentPack.GetOrCreateContentPack();
			contentPack.identifier = identifier;
			args.ReportProgress(1f);
			yield break;
		}

		public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
		{
			ContentPack.Copy(contentPack, args.output);
			args.ReportProgress(1f);
			yield break;
		}

		public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
		{
			args.ReportProgress(1f);
			yield break;
		}
	}
}

