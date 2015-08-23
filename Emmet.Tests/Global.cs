using Emmet.Diagnostics;
using NSubstitute;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Shell.Interop;

namespace Emmet.Tests
{
    /// <summary>
    /// Performs global application initialization and cleanup procedures.
    /// </summary>
    [TestClass]
    public class Global
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var pane = Substitute.For<IVsOutputWindowPane>();
            Tracer.Initialize(pane);
        }
    }
}