using System.IO;
using System.Text.RegularExpressions;
using Emmet.Engine;
using Emmet.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Emmet.Tests.Engine
{
    /// <summary>
    /// Base class for Emmet engine tests.
    /// </summary>
    public class EngineTestsBase
    {
        protected EngineWrapper _engine;

        private readonly string _extensionsDir;

        public TestContext TestContext { get; set; }

        public EngineTestsBase(string extensionsDir = null)
        {
            _extensionsDir = extensionsDir;
        }

        [TestInitialize]
        public void SetUp()
        {
            if (!string.IsNullOrEmpty(_extensionsDir))
                _engine = new EngineWrapper(Path.Combine(TestContext.DeploymentDirectory, _extensionsDir));
            else
                _engine = new EngineWrapper(null);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _engine.Dispose();
            _engine = null;
        }

        protected static string GetSourceFromResource(string resourceName)
        {
            string retVal = DataHelper.GetEmbeddedResource(resourceName);

            return NormalizeWhiteSpace(retVal);
        }

        protected static string NormalizeWhiteSpace(string source)
        {
            return Regex.Replace(
                source.Replace("\r", string.Empty),
                @"^\s+",
                string.Empty,
                RegexOptions.Multiline);
        }
    }
}