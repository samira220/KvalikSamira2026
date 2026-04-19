using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KvalikSamira.Models;
using KvalikSamira.Services;
using Microsoft.EntityFrameworkCore;

namespace KvalikSamira.ViewModels
{
    public partial class UserProfileViewModel : ViewModelBase
    {
        public IRelayCommand TopUpBalanceCommand { get; }
        public IRelayCommand BookAppointmentCommand { get; }
        public IRelayCommand WriteReviewCommand { get; }

        private User currentUser;

        [ObservableProperty]
        private string userInfo = "";

        [ObservableProperty]
        private string balanceText = "";

        [ObservableProperty]
        private string topUpAmount = "";

        [ObservableProperty]
        private string cardNumber = "";

        [ObservableProperty]
        private string statusMessage = "";

        [ObservableProperty]
        private ObservableCollection<Service> availableServices = new();

        [ObservableProperty]
        private Service? selectedServiceForAppointment;

        [ObservableProperty]
        private ObservableCollection<User> availableMasters = new();

        [ObservableProperty]
        private User? selectedMaster;

        [ObservableProperty]
        private string queueInfo = "";

        [ObservableProperty]
        private string reviewText = "";

        [ObservableProperty]
        private int reviewRating = 5;

        [ObservableProperty]
        private ObservableCollection<Appointment> myAppointments = new();

        [ObservableProperty]
        private bool hasAppointments;

        public UserProfileViewModel(User user)
        {
            TopUpBalanceCommand = new RelayCommand(topUpBalance);
            BookAppointmentCommand = new RelayCommand(bookAppointment);
            WriteReviewCommand = new RelayCommand(writeReview);
            currentUser = user;
            loadUserData();
        }

        private void loadUserData()
        {
            try
            {
                using var db = new MatyeDbContext();
                var user = db.users.Include(u => u.role).First(u => u.id == currentUser.id);
                currentUser = user;
                UserInfo = $"{user.lastName} {user.firstName} {user.patronymic ?? ""}\nРоль: {user.role.name}";
                BalanceText = $"{user.balance:F2} руб.";

                var services = db.services
                    .Include(s => s.serviceType)
                    .Include(s => s.masterServices).ThenInclude(ms => ms.user)
                    .OrderBy(s => s.name).ToList();
                AvailableServices = new ObservableCollection<Service>(services);

                var appointments = db.appointments
                    .Include(a => a.service)
                    .Include(a => a.master)
                    .Where(a => a.userId == currentUser.id)
                    .OrderByDescending(a => a.createdAt)
                    .ToList();
                MyAppointments = new ObservableCollection<Appointment>(appointments);
                HasAppointments = MyAppointments.Count > 0;
            }
            catch
            {
                HasAppointments = false;
            }
        }

        partial void OnSelectedServiceForAppointmentChanged(Service? value)
        {
            if (value != null)
            {
                var masters = value.masterServices?.Select(ms => ms.user).ToList() ?? new List<User>();
                AvailableMasters = new ObservableCollection<User>(masters);
                SelectedMaster = masters.FirstOrDefault();
            }
            else
            {
                AvailableMasters = new ObservableCollection<User>();
            }
        }

        private static string digitsOnly(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return new string(s.Where(char.IsDigit).ToArray());
        }

        private void topUpBalance()
        {
            StatusMessage = "";

            var cardDigits = digitsOnly(CardNumber);
            if (cardDigits.Length != 16)
            {
                StatusMessage = "Введите номер карты: ровно 16 цифр (пробелы допускаются)";
                return;
            }

            if (!decimal.TryParse(TopUpAmount?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
                amount <= 0)
            {
                StatusMessage = "Введите корректную сумму";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var user = db.users.Find(currentUser.id);
                if (user != null)
                {
                    user.balance += amount;
                    db.SaveChanges();
                    TopUpAmount = "";
                    CardNumber = "";
                    StatusMessage = $"Баланс пополнен на {amount:F2} руб.";
                    loadUserData();
                }
            }
            catch
            {
                StatusMessage = "Ошибка пополнения";
            }
        }

        private void bookAppointment()
        {
            StatusMessage = "";

            if (SelectedServiceForAppointment == null || SelectedMaster == null)
            {
                StatusMessage = "Выберите услугу и мастера";
                return;
            }

            if (AvailableMasters.Count == 0)
            {
                StatusMessage = "У выбранной услуги нет привязанных мастеров";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                var service = db.services.Find(SelectedServiceForAppointment.id);
                if (service == null)
                {
                    StatusMessage = "Услуга не найдена";
                    return;
                }

                var user = db.users.Find(currentUser.id);
                if (user == null)
                {
                    StatusMessage = "Пользователь не найден";
                    return;
                }

                if (user.balance < service.price)
                {
                    StatusMessage = "Недостаточно средств на балансе";
                    return;
                }

                int lastQueue = db.appointments
                    .Where(a => a.serviceId == SelectedServiceForAppointment.id && a.masterId == SelectedMaster.id)
                    .Select(a => a.queueNumber)
                    .DefaultIfEmpty(0)
                    .Max();

                var appointment = new Appointment
                {
                    userId = currentUser.id,
                    serviceId = SelectedServiceForAppointment.id,
                    masterId = SelectedMaster.id,
                    queueNumber = lastQueue + 1,
                    appointmentDate = DateTime.UtcNow,
                    createdAt = DateTime.UtcNow
                };

                db.appointments.Add(appointment);

                user.balance -= service.price;

                db.SaveChanges();

                QueueInfo = $"Ваш номер в очереди: {appointment.queueNumber}";
                StatusMessage = $"Вы записаны! Номер в очереди: {appointment.queueNumber}";
                loadUserData();
            }
            catch
            {
                StatusMessage = "Ошибка записи";
            }
        }

        private void writeReview()
        {
            StatusMessage = "";

            if (string.IsNullOrWhiteSpace(ReviewText))
            {
                StatusMessage = "Напишите текст отзыва";
                return;
            }

            if (SelectedServiceForAppointment == null)
            {
                StatusMessage = "Выберите услугу для отзыва (в блоке «Запись на услугу»)";
                return;
            }

            if (ReviewRating is < 1 or > 5)
            {
                StatusMessage = "Оценка должна быть от 1 до 5";
                return;
            }

            try
            {
                using var db = new MatyeDbContext();
                int? masterId = SelectedMaster?.id;
                if (masterId != null)
                {
                    bool masterOk = db.masterServices.Any(ms =>
                        ms.serviceId == SelectedServiceForAppointment.id && ms.userId == masterId);
                    if (!masterOk)
                    {
                        StatusMessage = "Выбранный мастер не оказывает эту услугу — выберите мастера из списка";
                        return;
                    }
                }

                var review = new Review
                {
                    userId = currentUser.id,
                    serviceId = SelectedServiceForAppointment.id,
                    masterId = masterId,
                    text = ReviewText.Trim(),
                    rating = ReviewRating,
                    createdAt = DateTime.UtcNow
                };
                db.reviews.Add(review);
                db.SaveChanges();

                ReviewText = "";
                StatusMessage = "Отзыв отправлен!";
            }
            catch
            {
                StatusMessage = "Ошибка отправки отзыва";
            }
        }
    }
}
