using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestVault.Client.Models;

namespace NestVault.Client.PageModels
{
    public partial class VaultItemDetailPageModel : ObservableObject, IQueryAttributable
    {
        private readonly VaultItemRepository _repository;
        private readonly PasswordGeneratorService _passwordGenerator;
        private readonly ModalErrorHandler _errorHandler;
        private VaultItem _item = new();

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _url = string.Empty;

        [ObservableProperty]
        private string _notes = string.Empty;

        [ObservableProperty]
        private bool _isFavorite;

        [ObservableProperty]
        private string _pageTitle = "New Item";

        [ObservableProperty]
        private bool _canDelete;

        public VaultItemDetailPageModel(VaultItemRepository repository, PasswordGeneratorService passwordGenerator, ModalErrorHandler errorHandler)
        {
            _repository = repository;
            _passwordGenerator = passwordGenerator;
            _errorHandler = errorHandler;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id) && id > 0)
            {
                LoadItemAsync(id).FireAndForgetSafeAsync(_errorHandler);
            }
            else
            {
                _item = new VaultItem();
                Name = Username = Password = Url = Notes = string.Empty;
                IsFavorite = false;
                PageTitle = "New Item";
                CanDelete = false;
            }
        }

        private async Task LoadItemAsync(int id)
        {
            var item = await _repository.GetAsync(id) ?? throw new Exception($"Vault item {id} was not found.");

            _item = item;
            Name = item.Name;
            Username = item.Username;
            Password = item.Password;
            Url = item.Url;
            Notes = item.Notes;
            IsFavorite = item.IsFavorite;
            PageTitle = item.Name;
            CanDelete = true;
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await Shell.Current.DisplayAlertAsync("Missing name", "Please give this item a name.", "OK");
                return;
            }

            try
            {
                _item.Name = Name.Trim();
                _item.Username = Username;
                _item.Password = Password;
                _item.Url = Url.Trim();
                _item.Notes = Notes;
                _item.IsFavorite = IsFavorite;

                await _repository.SaveItemAsync(_item);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (_item.ID == 0)
                return;

            var confirmed = await Shell.Current.DisplayAlertAsync(
                "Delete item",
                $"Delete \"{_item.Name}\"? This cannot be undone.",
                "Delete", "Cancel");

            if (!confirmed)
                return;

            try
            {
                await _repository.DeleteItemAsync(_item);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }

        [RelayCommand]
        private void GeneratePassword()
        {
            Password = _passwordGenerator.Generate(16, useLowercase: true, useUppercase: true, useDigits: true, useSymbols: true);
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            if (string.IsNullOrEmpty(Password))
                return;

            await Clipboard.Default.SetTextAsync(Password);
            await AppShell.DisplaySnackbarAsync("Password copied to clipboard");
        }
    }
}
