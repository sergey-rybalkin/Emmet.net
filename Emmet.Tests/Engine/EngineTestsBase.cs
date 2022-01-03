using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Emmet.Engine;
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

        private readonly string _baseDirectory;

        public EngineTestsBase(string extensionsDir = null)
        {
            _extensionsDir = extensionsDir;
            _baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        [TestInitialize]
        public void SetUp()
        {
            if (!string.IsNullOrEmpty(_extensionsDir))
                _engine = new EngineWrapper(Path.Combine(_baseDirectory, _extensionsDir));
            else
                _engine = new EngineWrapper(null);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _engine.Dispose();
            _engine = null;
        }

        protected string GetSourceFromResource(string resourceName)
        {
            string filePath = Path.Combine(_baseDirectory, "Resources", resourceName);
            string retVal = File.ReadAllText(filePath);

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