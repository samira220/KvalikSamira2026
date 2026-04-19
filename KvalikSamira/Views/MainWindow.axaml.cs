using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace KvalikSamira.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }

        private async void CloseClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Подтверждение",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                SystemDecorations = SystemDecorations.BorderOnly
            };

            var result = false;
            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15
            };
            panel.Children.Add(new TextBlock
            {
                Text = "Вы действительно хотите выйти?",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            });

            var buttonsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10
            };

            var yesBtn = new Button { Content = "Да", Width = 80 };
            yesBtn.Click += (_, _) => { result = true; dialog.Close(); };

            var noBtn = new Button { Content = "Нет", Width = 80 };
            noBtn.Click += (_, _) => { dialog.Close(); };

            buttonsPanel.Children.Add(yesBtn);
            buttonsPanel.Children.Add(noBtn);
            panel.Children.Add(buttonsPanel);

            dialog.Content = panel;
            await dialog.ShowDialog(this);

            if (result)
            {
                Close();
            }
        }

        private void MinimizeClick(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}