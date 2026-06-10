using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NestVault.Client.Models;

namespace NestVault.Client.PageModels
{
    public partial class VaultPageModel : ObservableObject
    {
        private readonly VaultItemRepository _repository;
        private readonly ModalErrorHandler _errorHandler;
        private List<VaultItem> _allItems = [];

        [ObservableProperty]
        private ObservableCollection<VaultItem> _items = [];

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isBusy;

        public VaultPageModel(VaultItemRepository repository, ModalErrorHandler errorHandler)
        {
            _repository = repository;
            _errorHandler = errorHandler;
        }

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        [RelayCommand]
        private async Task Appearing()
        {
            try
            {
                IsBusy = true;
                _allItems = await _repository.ListAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilter()
        {
            var query = SearchText.Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? _allItems
                : _allItems.Where(i =>
                    i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    i.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    i.Url.Contains(query, StringComparison.OrdinalIgnoreCase));

            Items = new ObservableCollection<VaultItem>(filtered);
        }

        [RelayCommand]
        private Task AddItem() => Shell.Current.GoToAsync("item?id=0");

        [RelayCommand]
        private Task OpenItem(VaultItem item) => Shell.Current.GoToAsync($"item?id={item.ID}");

        [RelayCommand]
        private async Task CopyUsername(VaultItem item)
        {
            await Clipboard.Default.SetTextAsync(item.Username);
            await AppShell.DisplaySnackbarAsync("Username copied to clipboard");
        }

        [RelayCommand]
        private async Task CopyPassword(VaultItem item)
        {
            await Clipboard.Default.SetTextAsync(item.Password);
            await AppShell.DisplaySnackbarAsync("Password copied to clipboard");
        }

        [RelayCommand]
        private async Task ToggleFavorite(VaultItem item)
        {
            try
            {
                item.IsFavorite = !item.IsFavorite;
                await _repository.SaveItemAsync(item);
                _allItems = await _repository.ListAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
        }
    }
}
