namespace NestVault.Client.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsPageModel _model;

        public SettingsPage(SettingsPageModel model)
        {
            InitializeComponent();
            BindingContext = _model = model;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _model.AppearingCommand.Execute(null);
        }
    }
}
