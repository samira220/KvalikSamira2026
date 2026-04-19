using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KvalikSamira.Models;

namespace KvalikSamira.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public IRelayCommand ShowServicesCommand { get; }
        public IRelayCommand ShowGuestCatalogCommand { get; }
        public IRelayCommand ShowProfileCommand { get; }
        public IRelayCommand ShowModeratorPanelCommand { get; }
        public IRelayCommand ShowAdminPanelCommand { get; }
        public IRelayCommand ShowMasterPanelCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        [ObservableProperty]
        private object? currentPage;

        [ObservableProperty]
        private bool isLoggedIn;

        [ObservableProperty]
        private string currentUserName = "";

        [ObservableProperty]
        private string currentRoleName = "";

        [ObservableProperty]
        private bool isUser;

        [ObservableProperty]
        private bool isModerator;

        [ObservableProperty]
        private bool isAdmin;

        [ObservableProperty]
        private bool isMaster;

        private User? currentUser;

        public MainWindowViewModel()
        {
            ShowServicesCommand = new RelayCommand(showServices);
            ShowGuestCatalogCommand = new RelayCommand(showGuestCatalog);
            ShowProfileCommand = new RelayCommand(showProfile);
            ShowModeratorPanelCommand = new RelayCommand(showModeratorPanel);
            ShowAdminPanelCommand = new RelayCommand(showAdminPanel);
            ShowMasterPanelCommand = new RelayCommand(showMasterPanel);
            LogoutCommand = new RelayCommand(logout);
            showLogin();
        }

        private void showLogin()
        {
            IsLoggedIn = false;
            var loginVm = new LoginViewModel();
            loginVm.LoginSuccess += onLoginSuccess;
            loginVm.GoToRegister += showRegister;
            CurrentPage = loginVm;
        }

        private void showRegister()
        {
            var registerVm = new RegisterViewModel();
            registerVm.RegisterSuccess += showLogin;
            registerVm.GoToLogin += showLogin;
            CurrentPage = registerVm;
        }

        private void onLoginSuccess(User user)
        {
            currentUser = user;
            IsLoggedIn = true;
            CurrentUserName = $"{user.lastName} {user.firstName}";
            CurrentRoleName = user.role?.name ?? "";

            IsUser = CurrentRoleName == "Пользователь";
            IsModerator = CurrentRoleName == "Модератор";
            IsAdmin = CurrentRoleName == "Администратор";
            IsMaster = CurrentRoleName == "Мастер";

            showServices();
        }

        private void showServices()
        {
            bool canManage = currentUser?.role?.name == "Модератор";
            CurrentPage = new ServiceListViewModel(canManageServices: canManage);
        }

        private void showGuestCatalog()
        {
            CurrentPage = new ServiceListViewModel(
                canManageServices: false,
                navigateBackFromGuest: showLogin);
        }

        private void showProfile()
        {
            if (currentUser != null)
                CurrentPage = new UserProfileViewModel(currentUser);
        }

        private void showModeratorPanel()
        {
            CurrentPage = new ModeratorViewModel();
        }

        private void showAdminPanel()
        {
            CurrentPage = new AdminViewModel();
        }

        private void showMasterPanel()
        {
            if (currentUser != null)
                CurrentPage = new MasterViewModel(currentUser);
        }

        private void logout()
        {
            currentUser = null;
            IsLoggedIn = false;
            IsUser = false;
            IsModerator = false;
            IsAdmin = false;
            IsMaster = false;
            CurrentUserName = "";
            CurrentRoleName = "";
            showLogin();
        }
    }
}
