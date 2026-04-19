using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KvalikSamira.Services;
using Microsoft.EntityFrameworkCore;

namespace KvalikSamira.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        public IRelayCommand LoginCommand { get; }
        public IRelayCommand GoToRegisterCommand { get; }

        [ObservableProperty]
        private string login = "";

        [ObservableProperty]
        private string password = "";

        [ObservableProperty]
        private string errorMessage = "";

        public event System.Action<Models.User>? LoginSuccess;
        public event System.Action? GoToRegister;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(loginAction);
            GoToRegisterCommand = new RelayCommand(goToRegisterAction);
        }

        private void loginAction()
        {
            ErrorMessage = "";
            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Заполните все поля";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var user = db.users
                    .Where(u => u.login == Login && u.password == Password)
                    .Select(u => new { u.id, u.login, u.password, u.firstName, u.lastName, u.patronymic, u.balance, u.roleId })
                    .FirstOrDefault();

                if (user == null)
                {
                    ErrorMessage = "Неверный логин или пароль";
                    return;
                }

                var fullUser = db.users
                    .Include(u => u.role)
                    .First(u => u.id == user.id);

                LoginSuccess?.Invoke(fullUser);
            }
            catch
            {
                ErrorMessage = "Ошибка подключения к БД";
            }
        }

        private void goToRegisterAction()
        {
            GoToRegister?.Invoke();
        }
    }
}
