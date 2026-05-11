using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Persistence;

namespace Tests.Feats
{
	public sealed class FeatBoardProgressSaveTests
	{
		[Test]
		public void FeatRequirementProgress_ToJson_MatchesExpectedJson()
		{
			FeatRequirementProgress progress = new FeatRequirementProgress
			{
				CurrentProgress = 65.5f,
				CompletedRepeatCount = 2
			};

			JObject json = progress.ToJson();

			SaveTestDataFactory.AssertJsonEquals(
				SaveTestDataFactory.ExpectedRequirementJson(65.5f, 2),
				json);
		}

		[Test]
		public void FeatRequirementProgress_LoadFromJson_RestoresProgressAndRepeats()
		{
			FeatRequirementProgress progress = new FeatRequirementProgress();
			JObject json = SaveTestDataFactory.ExpectedRequirementJson(25f, 3);

			progress.LoadFromJson(json);

			Assert.That(progress.CurrentProgress, Is.EqualTo(25f));
			Assert.That(progress.CompletedRepeatCount, Is.EqualTo(3));
		}

		[Test]
		public void FeatNodeProgress_ToJson_MatchesExpectedJson()
		{
			FeatNode node = SaveTestDataFactory.CreateDamageNode();
			FeatNodeProgress progress = new FeatNodeProgress(node)
			{
				CompletionCount = 2
			};
			progress.RequirementProgress[0].CurrentProgress = 40f;
			progress.RequirementProgress[0].CompletedRepeatCount = 1;

			JObject json = progress.ToJson();

			SaveTestDataFactory.AssertJsonEquals(
				SaveTestDataFactory.ExpectedNodeJson(
					SaveTestDataFactory.DamageNodeId,
					2,
					SaveTestDataFactory.ExpectedRequirementJson(40f, 1)),
				json);
		}

		[Test]
		public void FeatNodeProgress_FromJson_RestoresCompletionsAndRequirements()
		{
			FeatNode node = SaveTestDataFactory.CreateDamageNode();
			JObject json = SaveTestDataFactory.ExpectedNodeJson(
				SaveTestDataFactory.DamageNodeId,
				3,
				SaveTestDataFactory.ExpectedRequirementJson(75f, 2));

			FeatNodeProgress progress = FeatNodeProgress.FromJson(json, node);

			Assert.That(progress.NodeId, Is.EqualTo(SaveTestDataFactory.DamageNodeId));
			Assert.That(progress.CompletionCount, Is.EqualTo(3));
			Assert.That(progress.RequirementProgress, Has.Count.EqualTo(1));
			Assert.That(progress.RequirementProgress[0].CurrentProgress, Is.EqualTo(75f));
			Assert.That(progress.RequirementProgress[0].CompletedRepeatCount, Is.EqualTo(2));
		}

		[Test]
		public void FeatBoardProgress_ToJson_MatchesExpectedJson()
		{
			FeatBoard board = SaveTestDataFactory.CreateFeatBoard();
			FeatBoardProgress progress = new FeatBoardProgress();

			FeatNodeProgress rootProgress = progress.GetOrCreateProgress(board.GetNode(SaveTestDataFactory.RootNodeId));
			rootProgress.CompletionCount = 1;

			FeatNodeProgress damageProgress = progress.GetOrCreateProgress(board.GetNode(SaveTestDataFactory.DamageNodeId));
			damageProgress.CompletionCount = 2;
			damageProgress.RequirementProgress[0].CurrentProgress = 40f;
			damageProgress.RequirementProgress[0].CompletedRepeatCount = 1;

			JObject json = progress.ToJson();

			JObject expected = new JObject
			{
				["nodes"] = new JArray
				{
					SaveTestDataFactory.ExpectedNodeJson(SaveTestDataFactory.RootNodeId, 1),
					SaveTestDataFactory.ExpectedNodeJson(
						SaveTestDataFactory.DamageNodeId,
						2,
						SaveTestDataFactory.ExpectedRequirementJson(40f, 1))
				}
			};
			SaveTestDataFactory.AssertJsonEquals(expected, json);
		}

		[Test]
		public void FeatBoardProgress_FromJson_RestoresKnownNodesAndIgnoresUnknownNodes()
		{
			FeatBoard board = SaveTestDataFactory.CreateFeatBoard();
			JObject json = new JObject
			{
				["nodes"] = new JArray
				{
					SaveTestDataFactory.ExpectedNodeJson(SaveTestDataFactory.RootNodeId, 1),
					SaveTestDataFactory.ExpectedNodeJson(
						SaveTestDataFactory.DamageNodeId,
						2,
						SaveTestDataFactory.ExpectedRequirementJson(40f, 1)),
					SaveTestDataFactory.ExpectedNodeJson("removed_node", 5)
				}
			};

			FeatBoardProgress progress = FeatBoardProgress.FromJson(json, board);

			Assert.That(progress.NodeProgress, Has.Count.EqualTo(2));
			Assert.That(progress.FindProgress(SaveTestDataFactory.RootNodeId).CompletionCount, Is.EqualTo(1));

			FeatNodeProgress damageProgress = progress.FindProgress(SaveTestDataFactory.DamageNodeId);
			Assert.That(damageProgress.CompletionCount, Is.EqualTo(2));
			Assert.That(damageProgress.RequirementProgress[0].CurrentProgress, Is.EqualTo(40f));
			Assert.That(damageProgress.RequirementProgress[0].CompletedRepeatCount, Is.EqualTo(1));
			Assert.That(progress.FindProgress("removed_node"), Is.Null);
		}
	}
}
