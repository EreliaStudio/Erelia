using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Tests.Persistence;
using UnityEngine;

namespace Tests.Data
{
	public sealed class SaveHelperTests
	{
		[Test]
		public void Vector3_ToJson_MatchesExpectedCoordinates()
		{
			Vector3 vector = new Vector3(1.25f, -2.5f, 3.75f);

			JObject json = SaveHelper.ToJson(vector);

			SaveTestDataFactory.AssertJsonEquals(
				new JObject
				{
					["x"] = 1.25f,
					["y"] = -2.5f,
					["z"] = 3.75f
				},
				json);
		}

		[Test]
		public void Vector3_FromJson_RestoresCoordinates()
		{
			JObject json = new JObject
			{
				["x"] = 1.25f,
				["y"] = -2.5f,
				["z"] = 3.75f
			};

			Vector3 vector = SaveHelper.ToVector3(json);

			Assert.That(vector, Is.EqualTo(new Vector3(1.25f, -2.5f, 3.75f)));
		}

		[Test]
		public void Vector3Int_ToJson_MatchesExpectedCoordinates()
		{
			Vector3Int vector = new Vector3Int(1, -2, 3);

			JObject json = SaveHelper.ToJson(vector);

			SaveTestDataFactory.AssertJsonEquals(
				new JObject
				{
					["x"] = 1,
					["y"] = -2,
					["z"] = 3
				},
				json);
		}

		[Test]
		public void Vector3Int_FromJson_RestoresCoordinates()
		{
			JObject json = new JObject
			{
				["x"] = 1,
				["y"] = -2,
				["z"] = 3
			};

			Vector3Int vector = SaveHelper.ToVector3Int(json);

			Assert.That(vector, Is.EqualTo(new Vector3Int(1, -2, 3)));
		}
	}
}
