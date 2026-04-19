using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emmet;

[assembly: AssemblyTitle(Vsix.Name)]
[assembly: AssemblyDescription(Vsix.Description)]
[assembly: AssemblyCompany(Vsix.Author)]
[assembly: AssemblyProduct(Vsix.Name)]
[assembly: ComVisible(false)]
[assembly: AssemblyVersion(Vsix.Version)]
[assembly: AssemblyFileVersion(Vsix.Version)]
[assembly: InternalsVisibleTo("Emmet.Tests")]