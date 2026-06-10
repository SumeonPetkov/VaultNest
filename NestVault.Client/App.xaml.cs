namespace NestVault.Client
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell())
            {
                Title = "NestVault"
            };

            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
            {
                window.Width = 1100;
                window.Height = 760;
                window.MinimumWidth = 800;
                window.MinimumHeight = 600;
            }

            return window;
        }
    }
}
