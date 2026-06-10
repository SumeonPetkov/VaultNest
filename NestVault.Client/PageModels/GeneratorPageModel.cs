using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NestVault.Client.PageModels
{
    public partial class GeneratorPageModel : ObservableObject
    {
        private readonly PasswordGeneratorService _passwordGenerator;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(LengthLabel))]
        private double _length = 16;

        [ObservableProperty]
        private bool _includeLowercase = true;

        [ObservableProperty]
        private bool _includeUppercase = true;

        [ObservableProperty]
        private bool _includeDigits = true;

        [ObservableProperty]
        private bool _includeSymbols = true;

        [ObservableProperty]
        private string _generatedPassword = string.Empty;

        public string LengthLabel => $"Length: {(int)Length}";

        public GeneratorPageModel(PasswordGeneratorService passwordGenerator)
        {
            _passwordGenerator = passwordGenerator;
            Regenerate();
        }

        partial void OnLengthChanged(double value) => Regenerate();
        partial void OnIncludeLowercaseChanged(bool value) => Regenerate();
        partial void OnIncludeUppercaseChanged(bool value) => Regenerate();
        partial void OnIncludeDigitsChanged(bool value) => Regenerate();
        partial void OnIncludeSymbolsChanged(bool value) => Regenerate();

        [RelayCommand]
        private void Regenerate()
        {
            GeneratedPassword = _passwordGenerator.Generate(
                (int)Length, IncludeLowercase, IncludeUppercase, IncludeDigits, IncludeSymbols);
        }

        [RelayCommand]
        private async Task Copy()
        {
            await Clipboard.Default.SetTextAsync(GeneratedPassword);
            await AppShell.DisplaySnackbarAsync("Password copied to clipboard");
        }
    }
}
