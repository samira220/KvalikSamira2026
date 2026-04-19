using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KvalikSamira.Models;
using KvalikSamira.Services;
using Microsoft.EntityFrameworkCore;

namespace KvalikSamira.ViewModels
{
    public partial class MasterViewModel : ViewModelBase
    {
        public IRelayCommand RequestQualificationUpgradeCommand { get; }

        private User currentUser;

        [ObservableProperty]
        private string masterInfo = "";

        [ObservableProperty]
        private string qualificationInfo = "";

        [ObservableProperty]
        private ObservableCollection<Appointment> myClients = new();

        [ObservableProperty]
        private ObservableCollection<Service> myServices = new();

        [ObservableProperty]
        private string requestedLevel = "";

        [ObservableProperty]
        private string statusMessage = "";

        [ObservableProperty]
        private ObservableCollection<string> qualificationLevels = new(
            new[] { "Начинающий", "Опытный", "Эксперт", "Мастер" });

        [ObservableProperty]
        private string? selectedQualLevel;

        public MasterViewModel(User user)
        {
            RequestQualificationUpgradeCommand = new RelayCommand(requestQualificationUpgrade);
            currentUser = user;
            loadData();
        }

        private void loadData()
        {
            try
            {
                using var db = new MatyeDbContext();
                var user = db.users.Include(u => u.role).First(u => u.id == currentUser.id);
                MasterInfo = $"{user.lastName} {user.firstName} {user.patronymic ?? ""}";

                var qualification = db.qualifications.FirstOrDefault(q => q.userId == currentUser.id);
                QualificationInfo = qualification != null ? $"Квалификация: {qualification.level}" : "Квалификация не определена";

                var services = db.masterServices
                    .Include(ms => ms.service).ThenInclude(s => s.serviceType)
                    .Where(ms => ms.userId == currentUser.id)
                    .Select(ms => ms.service)
                    .OrderBy(s => s.name)
                    .ToList();
                MyServices = new ObservableCollection<Service>(services);

                var appointments = db.appointments
                    .Include(a => a.user)
                    .Include(a => a.service)
                    .Where(a => a.masterId == currentUser.id)
                    .OrderByDescending(a => a.appointmentDate)
                    .ToList();
                MyClients = new ObservableCollection<Appointment>(appointments);
            }
            catch { }
        }

        private void requestQualificationUpgrade()
        {
            StatusMessage = "";

            if (string.IsNullOrWhiteSpace(SelectedQualLevel))
            {
                StatusMessage = "Выберите желаемый уровень";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var qualification = db.qualifications.FirstOrDefault(q => q.userId == currentUser.id);
                string currentLevel = qualification?.level ?? "Начинающий";

                if (currentLevel == SelectedQualLevel)
                {
                    StatusMessage = "Вы уже имеете этот уровень";
                    return;
                }

                bool hasPending = db.qualificationRequests
                    .Any(q => q.userId == currentUser.id && q.status == "На рассмотрении");

                if (hasPending)
                {
                    StatusMessage = "У вас уже есть заявка на рассмотрении";
                    return;
                }

                db.qualificationRequests.Add(new QualificationRequest
                {
                    userId = currentUser.id,
                    currentLevel = currentLevel,
                    requestedLevel = SelectedQualLevel,
                    status = "На рассмотрении",
                    createdAt = DateTime.UtcNow
                });
                db.SaveChanges();

                StatusMessage = "Заявка на повышение отправлена!";
                loadData();
            }
            catch
            {
                StatusMessage = "Ошибка отправки заявки";
            }
        }
    }
}
