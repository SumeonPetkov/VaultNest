using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NestVault.Client.PageModels
{
    public partial class SettingsPageModel : ObservableObject
    {
        private readonly AuthApiService _authApi;
        private readonly TokenStore _tokenStore;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public string AppVersion => $"NestVault {AppInfo.Current.VersionString}";

        public SettingsPageModel(AuthApiService authApi, TokenStore tokenStore)
        {
            _authApi = authApi;
            _tokenStore = tokenStore;
        }

        [RelayCommand]
        private void Appearing()
        {
            Email = _tokenStore.Email ?? string.Empty;
        }

        [RelayCommand]
        private async Task Logout()
        {
            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Log out",
                "Your vault stays encrypted on this device. Log out now?",
                "Log out", "Cancel");

            if (!confirmed)
                return;

            IsBusy = true;
            try
            {
                await _authApi.LogoutAsync();
                await Shell.Current.GoToAsync("//login");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
