namespace OkageLibraryMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
	}

    public void SwitchToTab(int tabIndex)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (tabIndex)
            {
                case 0: AppTabBar.CurrentItem = HomeTab; break;
                case 1: AppTabBar.CurrentItem = GamesTab; break;
                case 2: AppTabBar.CurrentItem = ELFTab; break;
            };
        });

    }
}
