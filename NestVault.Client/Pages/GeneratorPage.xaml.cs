namespace NestVault.Client.Pages
{
    public partial class GeneratorPage : ContentPage
    {
        public GeneratorPage(GeneratorPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
