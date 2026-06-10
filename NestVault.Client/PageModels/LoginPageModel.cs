using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NestVault.Client.PageModels
{
    public partial class LoginPageModel : ObservableObject
    {
        private readonly AuthApiService _authApi;
        private bool _sessionRestoreAttempted;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private bool _isBusy;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage) && !IsBusy;

        partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

        public LoginPageModel(AuthApiService authApi)
        {
            _authApi = authApi;
        }

        [RelayCommand]
        private async Task Appearing()
        {
            Password = string.Empty;
            ErrorMessage = string.Empty;

            // Try a silent sign-in with the persisted refresh token, once per app run.
            if (_sessionRestoreAttempted)
                return;
            _sessionRestoreAttempted = true;

            IsBusy = true;
            try
            {
                if (await _authApi.TryRestoreSessionAsync())
                    await Shell.Current.GoToAsync("//vault");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter your email and password.";
                return;
            }

            IsBusy = true;
            try
            {
                var result = await _authApi.LoginAsync(Email.Trim(), Password);
                if (!result.Success)
                {
                    ErrorMessage = result.ErrorMessage ?? "Login failed.";
                    return;
                }

                Password = string.Empty;
                await Shell.Current.GoToAsync("//vault");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task GoToRegister() => Shell.Current.GoToAsync("//register");
    }
}
