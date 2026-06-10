namespace NestVault.Client.Pages
{
    public partial class VaultItemDetailPage : ContentPage
    {
        public VaultItemDetailPage(VaultItemDetailPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}
