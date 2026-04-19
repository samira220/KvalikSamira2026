using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KvalikSamira.Models;
using KvalikSamira.Services;

namespace KvalikSamira.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        public IRelayCommand RegisterCommand { get; }
        public IRelayCommand GoToLoginCommand { get; }

        [ObservableProperty]
        private string lastName = "";

        [ObservableProperty]
        private string firstName = "";

        [ObservableProperty]
        private string patronymic = "";

        [ObservableProperty]
        private string login = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private string confirmPassword = "";

        [ObservableProperty]
        private string errorMessage = "";

        public event System.Action? RegisterSuccess;
        public event System.Action? GoToLogin;

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(registerAction);
            GoToLoginCommand = new RelayCommand(goToLoginAction);
        }

        private void registerAction()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все обязательные поля";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();

                if (db.users.Any(u => u.login == Login))
                {
                    ErrorMessage = "Пользователь с таким логином уже существует";
                    return;
                }

                var userRole = db.roles.First(r => r.name == "Пользователь");

                var newUser = new User
                {
                    lastName = LastName,
                    firstName = FirstName,
                    patronymic = string.IsNullOrWhiteSpace(Patronymic) ? null : Patronymic,
                    login = Login,
                    password = Password,
                    balance = 0,
                    roleId = userRole.id
                };

                db.users.Add(newUser);
                db.SaveChanges();

                RegisterSuccess?.Invoke();
            }
            catch
            {
                ErrorMessage = "Ошибка регистрации";
            }
        }

        private void goToLoginAction()
        {
            GoToLogin?.Invoke();
        }
    }
}
