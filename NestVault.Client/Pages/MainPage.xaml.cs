using NestVault.Client.Models;
using NestVault.Client.PageModels;

namespace NestVault.Client.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}