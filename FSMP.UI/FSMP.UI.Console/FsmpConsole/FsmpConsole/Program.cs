
using FsmpConsole;
using  FsmpLibrary;
using System.Linq;

//string testFileRoot = AppDomain.CurrentDomain.BaseDirectory;
//testFileRoot = testFileRoot.Replace("\\FsmpConsole\\bin\\Debug\\net10.0\\", "\\res\\sampleMusic");

//string foo = System.IO.Directory.GetCurrentDirectory();
//foo = foo.Substring(0, (foo.IndexOf("FsmpProject" + 12)));
//foo += @"res\sampleMusic";

//string testFileRoot = @".\FsmpProject\res\sampleMusic";
//string testFileRoot = @"~\FsmpProject\res\sampleMusic";

// Temporarily commented out until proper configuration service is implemented
string testFileRoot = @"C:\Users\Admin\source\repos\FsmpProject\res\sampleMusic";

while (true)
{

	Print.NewDisplay();

	Fsmp.CheckFileLocation(testFileRoot);

	var input = Console.ReadLine();

}



