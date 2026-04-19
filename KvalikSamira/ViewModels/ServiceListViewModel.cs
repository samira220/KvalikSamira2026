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
    public partial class ServiceListViewModel : ViewModelBase
    {
        public IRelayCommand BackToLoginCommand { get; }
        public IRelayCommand NextPageCommand { get; }
        public IRelayCommand PreviousPageCommand { get; }
        public IRelayCommand ClearCollectionFilterCommand { get; }
        public IRelayCommand<Service?> EditServiceCommand { get; }
        public IRelayCommand AddServiceCommand { get; }
        public IRelayCommand SaveServiceCommand { get; }
        public IRelayCommand CancelEditCommand { get; }

        private const int pageSize = 3;
        private List<Service> allServices = new();
        private readonly Action? navigateBackFromGuest;

        [ObservableProperty]
        private ObservableCollection<Service> currentServices = new();

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        private string pageInfo = "";

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private int selectedTabIndex;

        [ObservableProperty]
        private ObservableCollection<Collection> collectionsList = new();

        [ObservableProperty]
        private Collection? selectedCollection;

        [ObservableProperty]
        private Service? selectedService;

        private bool isEditing;

        [ObservableProperty]
        private bool isEditPanelVisible;

        [ObservableProperty]
        private string editName = "";

        [ObservableProperty]
        private string editDescription = "";

        [ObservableProperty]
        private string editPrice = "";

        [ObservableProperty]
        private string editDuration = "";

        [ObservableProperty]
        private string editMasterInfo = "";

        [ObservableProperty]
        private bool canManageServices;

        [ObservableProperty]
        private bool showGuestBackButton;

        public ServiceListViewModel(bool canManageServices = false, Action? navigateBackFromGuest = null)
        {
            BackToLoginCommand = new RelayCommand(backToLogin);
            NextPageCommand = new RelayCommand(nextPage);
            PreviousPageCommand = new RelayCommand(previousPage);
            ClearCollectionFilterCommand = new RelayCommand(clearCollectionFilter);
            EditServiceCommand = new RelayCommand<Service?>(editService);
            AddServiceCommand = new RelayCommand(addService);
            SaveServiceCommand = new RelayCommand(saveService);
            CancelEditCommand = new RelayCommand(cancelEdit);

            CanManageServices = canManageServices;
            this.navigateBackFromGuest = navigateBackFromGuest;
            ShowGuestBackButton = navigateBackFromGuest != null;
            loadData();
        }

        private void backToLogin()
        {
            navigateBackFromGuest?.Invoke();
        }

        private void loadData()
        {
            try
            {
                using var db = new MatyeDbContext();
                allServices = db.services
                    .Include(s => s.serviceType)
                    .Include(s => s.collection)
                    .Include(s => s.masterServices)
                        .ThenInclude(ms => ms.user)
                    .OrderBy(s => s.name)
                    .ToList();

                var collections = db.collections.OrderBy(c => c.name).ToList();
                CollectionsList = new ObservableCollection<Collection>(collections);
            }
            catch
            {
                allServices = new List<Service>();
            }

            applyFilters();
        }

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            applyFilters();
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            CurrentPage = 1;
            applyFilters();
        }

        partial void OnSelectedCollectionChanged(Collection? value)
        {
            CurrentPage = 1;
            applyFilters();
        }

        private void applyFilters()
        {
            var filtered = allServices.AsEnumerable();

            string typeName = SelectedTabIndex == 0 ? "Кастом" : "Косплей";
            filtered = filtered.Where(s => s.serviceType?.name == typeName);

            if (SelectedCollection != null)
            {
                filtered = filtered.Where(s => s.collectionId == SelectedCollection.id);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(s =>
                    s.name.ToLower().Contains(search) ||
                    (s.description ?? "").ToLower().Contains(search));
            }

            var list = filtered.OrderBy(s => s.name).ToList();

            TotalPages = Math.Max(1, (int)Math.Ceiling(list.Count / (double)pageSize));
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pageItems = list.Skip((CurrentPage - 1) * pageSize).Take(pageSize).ToList();
            CurrentServices = new ObservableCollection<Service>(pageItems);

            int start = (CurrentPage - 1) * pageSize + 1;
            int end = Math.Min(CurrentPage * pageSize, list.Count);
            PageInfo = list.Count > 0 ? $"{start}-{end} из {list.Count}" : "0 записей";
        }

        private void nextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                applyFilters();
            }
        }

        private void previousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                applyFilters();
            }
        }

        private void clearCollectionFilter()
        {
            SelectedCollection = null;
        }

        private void editService(Service? service)
        {
            if (!CanManageServices)
                return;
            if (service == null) return;
            SelectedService = service;
            isEditing = true;
            EditName = service.name;
            EditDescription = service.description ?? "";
            EditPrice = service.price.ToString("F2");
            EditDuration = service.durationMinutes.ToString();

            var masters = service.masterServices?.Select(ms => $"{ms.user.lastName} {ms.user.firstName}") ?? Enumerable.Empty<string>();
            EditMasterInfo = string.Join(", ", masters);

            IsEditPanelVisible = true;
        }

        private void addService()
        {
            if (!CanManageServices)
                return;
            SelectedService = null;
            isEditing = false;
            EditName = "";
            EditDescription = "";
            EditPrice = "";
            EditDuration = "60";
            EditMasterInfo = "";
            IsEditPanelVisible = true;
        }

        private void saveService()
        {
            if (!CanManageServices)
                return;
            if (string.IsNullOrWhiteSpace(EditName)) return;
            if (!decimal.TryParse(EditPrice, out var price)) return;
            if (!int.TryParse(EditDuration, out var duration)) return;

            try
            {
                using var db = new MatyeDbContext();

                if (isEditing && SelectedService != null)
                {
                    var service = db.services.Find(SelectedService.id);
                    if (service != null)
                    {
                        service.name = EditName;
                        service.description = EditDescription;
                        service.price = price;
                        service.durationMinutes = duration;
                        ServiceModificationTime.ApplyLastModifiedUtc(service, DateTime.UtcNow);
                        db.SaveChanges();
                    }
                }
                else
                {
                    string typeName = SelectedTabIndex == 0 ? "Кастом" : "Косплей";
                    var serviceType = db.serviceTypes.First(st => st.name == typeName);

                    var newService = new Service
                    {
                        name = EditName,
                        description = EditDescription,
                        price = price,
                        durationMinutes = duration,
                        serviceTypeId = serviceType.id,
                        collectionId = SelectedCollection?.id
                    };
                    ServiceModificationTime.ApplyNewEntityTimestampsUtc(newService, DateTime.UtcNow);
                    db.services.Add(newService);
                    db.SaveChanges();
                }

                IsEditPanelVisible = false;
                loadData();
            }
            catch
            {
            }
        }

        private void cancelEdit()
        {
            IsEditPanelVisible = false;
        }
    }
}
