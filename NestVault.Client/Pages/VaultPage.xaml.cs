namespace NestVault.Client.Pages
{
    public partial class VaultPage : ContentPage
    {
        private readonly VaultPageModel _model;

        public VaultPage(VaultPageModel model)
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
