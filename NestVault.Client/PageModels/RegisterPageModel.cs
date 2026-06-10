using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NestVault.Client.PageModels
{
    public partial class RegisterPageModel : ObservableObject
    {
        private readonly AuthApiService _authApi;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        private bool _isBusy;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage) && !IsBusy;

        partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

        public RegisterPageModel(AuthApiService authApi)
        {
            _authApi = authApi;
        }

        [RelayCommand]
        private async Task Register()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please fill in all fields.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            IsBusy = true;
            try
            {
                var result = await _authApi.RegisterAsync(FirstName.Trim(), LastName.Trim(), Email.Trim(), Password, ConfirmPassword);
                if (!result.Success)
                {
                    ErrorMessage = result.ErrorMessage ?? "Registration failed.";
                    return;
                }

                // Account created — sign in right away.
                var login = await _authApi.LoginAsync(Email.Trim(), Password);

                Password = string.Empty;
                ConfirmPassword = string.Empty;

                if (login.Success)
                {
                    await Shell.Current.GoToAsync("//vault");
                }
                else
                {
                    await Shell.Current.GoToAsync("//login");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToLogin()
        {
            ErrorMessage = string.Empty;
            await Shell.Current.GoToAsync("//login");
        }
    }
}
