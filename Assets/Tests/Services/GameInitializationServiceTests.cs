using NUnit.Framework;

namespace Tests.Services
{
    public sealed class GameInitializationServiceTests
    {
        [Test]
        public void TryInitializeNewGameSave_NullArguments_ReturnsFalse()
        {
            bool result = GameInitializationService.TryInitializeNewGameSave(null, null, null);

            Assert.That(result, Is.False);
        }
    }
}
