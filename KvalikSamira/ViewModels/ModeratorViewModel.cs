using System;
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
    public partial class ModeratorViewModel : ViewModelBase
    {
        public IRelayCommand BindMasterCommand { get; }
        public IRelayCommand UnbindMasterCommand { get; }
        public IRelayCommand ApproveRequestCommand { get; }
        public IRelayCommand RejectRequestCommand { get; }

        [ObservableProperty]
        private ObservableCollection<Service> servicesList = new();

        [ObservableProperty]
        private Service? selectedServiceMod;

        [ObservableProperty]
        private ObservableCollection<User> mastersList = new();

        [ObservableProperty]
        private User? selectedMasterMod;

        [ObservableProperty]
        private ObservableCollection<string> currentMastersOfService = new();

        [ObservableProperty]
        private ObservableCollection<QualificationRequest> pendingRequests = new();

        [ObservableProperty]
        private QualificationRequest? selectedRequest;

        [ObservableProperty]
        private string statusMessage = "";

        [ObservableProperty]
        private bool hasPendingRequests;

        public ModeratorViewModel()
        {
            BindMasterCommand = new RelayCommand(bindMaster);
            UnbindMasterCommand = new RelayCommand(unbindMaster);
            ApproveRequestCommand = new RelayCommand(approveRequest);
            RejectRequestCommand = new RelayCommand(rejectRequest);
            loadData();
        }

        private void loadData()
        {
            try
            {
                using var db = new MatyeDbContext();
                var services = db.services.Include(s => s.serviceType).OrderBy(s => s.name).ToList();
                ServicesList = new ObservableCollection<Service>(services);

                var masters = db.users.Include(u => u.role)
                    .Where(u => u.role.name == "Мастер")
                    .OrderBy(u => u.lastName).ToList();
                MastersList = new ObservableCollection<User>(masters);

                var requests = db.qualificationRequests
                    .Include(q => q.user)
                    .Where(q => q.status == "На рассмотрении")
                    .OrderByDescending(q => q.createdAt).ToList();
                PendingRequests = new ObservableCollection<QualificationRequest>(requests);
                HasPendingRequests = PendingRequests.Count > 0;
            }
            catch
            {
                HasPendingRequests = false;
            }
        }

        partial void OnSelectedServiceModChanged(Service? value)
        {
            loadMastersOfService();
        }

        private void loadMastersOfService()
        {
            if (SelectedServiceMod == null)
            {
                CurrentMastersOfService = new ObservableCollection<string>();
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var masters = db.masterServices
                    .Include(ms => ms.user)
                    .Where(ms => ms.serviceId == SelectedServiceMod.id)
                    .Select(ms => $"{ms.user.lastName} {ms.user.firstName}")
                    .ToList();
                CurrentMastersOfService = new ObservableCollection<string>(masters);
            }
            catch { }
        }

        private void bindMaster()
        {
            StatusMessage = "";
            if (SelectedServiceMod == null || SelectedMasterMod == null)
            {
                StatusMessage = "Выберите услугу и мастера";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                bool exists = db.masterServices.Any(ms =>
                    ms.userId == SelectedMasterMod.id && ms.serviceId == SelectedServiceMod.id);

                if (exists)
                {
                    StatusMessage = "Мастер уже привязан к этой услуге";
                    return;
                }

                db.masterServices.Add(new MasterService
                {
                    userId = SelectedMasterMod.id,
                    serviceId = SelectedServiceMod.id
                });
                db.SaveChanges();

                StatusMessage = "Мастер привязан к услуге";
                loadMastersOfService();
            }
            catch
            {
                StatusMessage = "Ошибка привязки";
            }
        }

        private void unbindMaster()
        {
            StatusMessage = "";
            if (SelectedServiceMod == null || SelectedMasterMod == null)
            {
                StatusMessage = "Выберите услугу и мастера";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var link = db.masterServices.FirstOrDefault(ms =>
                    ms.userId == SelectedMasterMod.id && ms.serviceId == SelectedServiceMod.id);

                if (link == null)
                {
                    StatusMessage = "Мастер не привязан к этой услуге";
                    return;
                }

                db.masterServices.Remove(link);
                db.SaveChanges();

                StatusMessage = "Мастер отвязан от услуги";
                loadMastersOfService();
            }
            catch
            {
                StatusMessage = "Ошибка отвязки";
            }
        }

        private void approveRequest()
        {
            StatusMessage = "";
            if (SelectedRequest == null)
            {
                StatusMessage = "Выберите заявку в списке";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var request = db.qualificationRequests.Find(SelectedRequest.id);
                if (request != null)
                {
                    request.status = "Одобрено";

                    var qualification = db.qualifications.FirstOrDefault(q => q.userId == request.userId);
                    if (qualification != null)
                    {
                        qualification.level = request.requestedLevel;
                    }
                    else
                    {
                        db.qualifications.Add(new Qualification
                        {
                            userId = request.userId,
                            level = request.requestedLevel
                        });
                    }

                    db.SaveChanges();
                    StatusMessage = "Квалификация повышена";
                    loadData();
                }
            }
            catch
            {
                StatusMessage = "Ошибка одобрения";
            }
        }

        private void rejectRequest()
        {
            StatusMessage = "";
            if (SelectedRequest == null)
            {
                StatusMessage = "Выберите заявку в списке";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var request = db.qualificationRequests.Find(SelectedRequest.id);
                if (request != null)
                {
                    request.status = "Отклонено";
                    db.SaveChanges();
                    StatusMessage = "Заявка отклонена";
                    loadData();
                }
            }
            catch
            {
                StatusMessage = "Ошибка отклонения";
            }
        }
    }
}
