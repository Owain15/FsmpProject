using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

namespace FSMP.MAUI.WinUI;

public static class Program
{
	[STAThread]
	static void Main(string[] args)
	{
		NativeSplash.Show();
		ComWrappersSupport.InitializeComWrappers();
		Microsoft.UI.Xaml.Application.Start(p =>
		{
			var context = new DispatcherQueueSynchronizationContext(
				DispatcherQueue.GetForCurrentThread());
			SynchronizationContext.SetSynchronizationContext(context);
			new App();
		});
	}
}
