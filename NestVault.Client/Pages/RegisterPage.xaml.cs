namespace NestVault.Client.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage(RegisterPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
