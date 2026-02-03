
using FsmpConsole;
using  FsmpLibrary;
using System.Linq;


// Temporarily commented out until proper configuration service is implemented
string testFileRoot = GetTestFileRoot();



while (true)
{

	Print.NewDisplay();

	Fsmp.CheckFileLocation(testFileRoot);

	var input = Console.ReadLine();

}

string GetTestFileRoot()
{
	string solutionName = "FsmpProject";
	string testDataDir = @"\res\sampleMusic\Music"; 

	var dir = new DirectoryInfo(AppContext.BaseDirectory);
	
	while (dir != null)
	{
		if (dir.Name.Equals(solutionName, StringComparison.OrdinalIgnoreCase))
			return dir.FullName + testDataDir;

		if (dir.GetFiles("*.sln").Any())
			return dir.FullName + testDataDir;

		dir = dir.Parent;
	}

	throw new DirectoryNotFoundException("Could not find the solution directory.");
}


