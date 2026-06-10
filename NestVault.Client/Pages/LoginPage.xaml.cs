namespace NestVault.Client.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginPageModel _model;

        public LoginPage(LoginPageModel model)
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
