using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KvalikSamira.Models;
using KvalikSamira.Services;
using Microsoft.EntityFrameworkCore;

namespace KvalikSamira.ViewModels
{
    public partial class AdminViewModel : ViewModelBase
    {
        public IRelayCommand StartEditUserCommand { get; }
        public IRelayCommand SaveEditUserCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public IRelayCommand ShowAddEmployeeCommand { get; }
        public IRelayCommand AddEmployeeCommand { get; }
        public IRelayCommand CancelAddCommand { get; }

        [ObservableProperty]
        private ObservableCollection<User> usersList = new();

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private ObservableCollection<Role> rolesList = new();

        [ObservableProperty]
        private bool isEditVisible;

        [ObservableProperty]
        private string editLastName = "";

        [ObservableProperty]
        private string editFirstName = "";

        [ObservableProperty]
        private string editPatronymic = "";

        [ObservableProperty]
        private string editLogin = "";

        [ObservableProperty]
        private Role? editRole;

        [ObservableProperty]
        private bool isAddVisible;

        [ObservableProperty]
        private string newLastName = "";

        [ObservableProperty]
        private string newFirstName = "";

        [ObservableProperty]
        private string newPatronymic = "";

        [ObservableProperty]
        private string newLogin = "";

        [ObservableProperty]
        private string newPassword = "";

        [ObservableProperty]
        private Role? newRole;

        [ObservableProperty]
        private string statusMessage = "";

        public AdminViewModel()
        {
            StartEditUserCommand = new RelayCommand(startEditUser);
            SaveEditUserCommand = new RelayCommand(saveEditUser);
            CancelEditCommand = new RelayCommand(cancelEdit);
            ShowAddEmployeeCommand = new RelayCommand(showAddEmployee);
            AddEmployeeCommand = new RelayCommand(addEmployee);
            CancelAddCommand = new RelayCommand(cancelAdd);
            loadData();
        }

        private void loadData()
        {
            try
            {
                using var db = new MatyeDbContext();
                var users = db.users.Include(u => u.role).OrderBy(u => u.lastName).ToList();
                UsersList = new ObservableCollection<User>(users);

                var roles = db.roles.OrderBy(r => r.name).ToList();
                RolesList = new ObservableCollection<Role>(roles);
            }
            catch { }
        }

        private void startEditUser()
        {
            StatusMessage = "";
            if (SelectedUser == null)
            {
                StatusMessage = "Выберите пользователя в списке";
                return;
            }
            EditLastName = SelectedUser.lastName;
            EditFirstName = SelectedUser.firstName;
            EditPatronymic = SelectedUser.patronymic ?? "";
            EditLogin = SelectedUser.login;
            EditRole = RolesList.FirstOrDefault(r => r.id == SelectedUser.roleId);
            IsEditVisible = true;
            IsAddVisible = false;
        }

        private void saveEditUser()
        {
            StatusMessage = "";
            if (SelectedUser == null) return;

            if (string.IsNullOrWhiteSpace(EditLastName) || string.IsNullOrWhiteSpace(EditFirstName) ||
                string.IsNullOrWhiteSpace(EditLogin))
            {
                StatusMessage = "Заполните обязательные поля";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var user = db.users.Find(SelectedUser.id);
                if (user != null)
                {
                    user.lastName = EditLastName;
                    user.firstName = EditFirstName;
                    user.patronymic = string.IsNullOrWhiteSpace(EditPatronymic) ? null : EditPatronymic;
                    user.login = EditLogin;
                    if (EditRole != null) user.roleId = EditRole.id;
                    db.SaveChanges();

                    StatusMessage = "Пользователь обновлён";
                    IsEditVisible = false;
                    loadData();
                }
            }
            catch
            {
                StatusMessage = "Ошибка сохранения";
            }
        }

        private void cancelEdit()
        {
            IsEditVisible = false;
        }

        private void showAddEmployee()
        {
            NewLastName = "";
            NewFirstName = "";
            NewPatronymic = "";
            NewLogin = "";
            NewPassword = "";
            NewRole = RolesList.FirstOrDefault(r => r.name == "Мастер");
            IsAddVisible = true;
            IsEditVisible = false;
        }

        private void addEmployee()
        {
            StatusMessage = "";

            if (string.IsNullOrWhiteSpace(NewLastName) || string.IsNullOrWhiteSpace(NewFirstName) ||
                string.IsNullOrWhiteSpace(NewLogin) || string.IsNullOrWhiteSpace(NewPassword))
            {
                StatusMessage = "Заполните все обязательные поля";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();

                if (db.users.Any(u => u.login == NewLogin))
                {
                    StatusMessage = "Логин уже занят";
                    return;
                }

                var employee = new User
                {
                    lastName = NewLastName,
                    firstName = NewFirstName,
                    patronymic = string.IsNullOrWhiteSpace(NewPatronymic) ? null : NewPatronymic,
                    login = NewLogin,
                    password = NewPassword,
                    balance = 0,
                    roleId = NewRole?.id ?? db.roles.First(r => r.name == "Мастер").id
                };
                db.users.Add(employee);
                db.SaveChanges();

                if (NewRole?.name == "Мастер")
                {
                    db.qualifications.Add(new Qualification
                    {
                        userId = employee.id,
                        level = "Начинающий"
                    });
                    db.SaveChanges();
                }

                StatusMessage = "Сотрудник добавлен";
                IsAddVisible = false;
                loadData();
            }
            catch
            {
                StatusMessage = "Ошибка добавления";
            }
        }

        private void cancelAdd()
        {
            IsAddVisible = false;
        }
    }
}
