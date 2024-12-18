using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Toolkit.Uwp.Notifications;

namespace LISTer
{
    public class DailyNote
    {
        public string Text { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public string BackgroundColor { get; set; } = "#d9d9d9";
        public TextDecorationCollection TextDecoration { get; set; } = null;
        public string RemainingTime
        {
            get
            {
                TimeSpan timeLeft = DueDate - DateTime.Now;
                if (timeLeft.TotalSeconds <= 0)
                    return "Срок истёк";
                int days = timeLeft.Days;
                int hours = timeLeft.Hours;
                int minutes = timeLeft.Minutes;
                return $"{days}д {hours}ч {minutes}мин";
            }
        }
    }
    public partial class MainPage : Page
    {
        private bool isOpen = false;
        private const string NotesFilePath = "notes.json";
        private List<DailyNote> Notes = new List<DailyNote>();
        private DateTime? SelectedDate;
        private TimeSpan? SelectedTime;
        private DispatcherTimer refreshTimer;

        public MainPage()
        {
            InitializeComponent();
            // подгрузка текущей даты
            string currentDate = DateTime.Now.ToString("dddd, dd MMMM");
            DateTextBox.Content = currentDate;
            // подгрузка заметок
            LoadNotes();
            // вывод уведомлений
            NotifyOverdueNotes();
            ShowCreationTips();
            // таймер на обновление страницы
            InitializeRefreshTimer();
        }
        // метод обновления страницы
        private void InitializeRefreshTimer()
        {
            refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            refreshTimer.Tick += (s, e) => RefreshNotesList();
            refreshTimer.Start();
        }
        // метод отправки уведомлений
        private void ShowToastNotification(string title, string content)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(content)
                .Show();
        }

        // уведомления о просроченных заметках
        private void NotifyOverdueNotes()
        {
            var overdueNotes = Notes.Where(note => note.DueDate < DateTime.Now && !note.IsCompleted).ToList();
            if (overdueNotes.Any())
            {
                string message = $"У вас {overdueNotes.Count} просроченных заметок!";
                ShowToastNotification("Просроченные заметки", message);
            }
        }

        // Метод для отображения совета дня
        private void ShowCreationTips()
        {
            // Список советов
            List<string> tips = new List<string>
            {
                "Не забывайте добавлять новые заметки для продуктивного дня!",
                "Планируйте день заранее для максимальной эффективности.",
                "Каждый день — новая возможность для достижения ваших целей!",
                "Используйте напоминания, чтобы не забывать важные дела.",
                "Не откладывайте на завтра то, что можете сделать сегодня.",
            };

            // Случайный выбор совета
            Random random = new Random();
            string randomTip = tips[random.Next(tips.Count)];

            // Показ уведомления с советом
            ShowToastNotification("Совет дня", randomTip);
        }

        //анимация панели
        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            double panelFrom = isOpen ? 0 : -SlidingPanel.Width;
            double panelTo = isOpen ? -SlidingPanel.Width : 0;

            double buttonFrom = isOpen ? SlidingPanel.Width : 0;
            double buttonTo = isOpen ? 0 : SlidingPanel.Width;

            var panelAnimation = new DoubleAnimation
            {
                From = panelFrom,
                To = panelTo,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase()
            };
            var buttonAnimation = new DoubleAnimation
            {
                From = buttonFrom,
                To = buttonTo,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase()
            };
            var overlayAnimation = new DoubleAnimation
            {
                From = isOpen ? 1 : 0,
                To = isOpen ? 0 : 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase()
            };
            var panelTransform = new TranslateTransform();
            SlidingPanel.RenderTransform = panelTransform;
            panelTransform.BeginAnimation(TranslateTransform.XProperty, panelAnimation);
            ButtonTransform.BeginAnimation(TranslateTransform.XProperty, buttonAnimation);
            BackgroundOverlay.BeginAnimation(OpacityProperty, overlayAnimation);
            isOpen = !isOpen;
            BackgroundOverlay.Visibility = isOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        // кнопки фильтрации элементов
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем значение тега, чтобы понять, какой фильтр применить
            var button = sender as Button;
            if (button == null) return;

            string filter = button.Tag.ToString();
            List<DailyNote> filteredNotes = new List<DailyNote>(Notes);

            switch (filter)
            {
                case "All":
                    // Все заметки
                    break;
                case "Completed":
                    // Фильтрация выполненных
                    filteredNotes = filteredNotes.Where(note => note.BackgroundColor == "#8fffaf").ToList();
                    break;
                case "NotCompleted":
                    // Фильтрация невыполненных
                    filteredNotes = filteredNotes.Where(note => note.BackgroundColor == "#d9d9d9").ToList();
                    break;
                case "Overdue":
                    // Фильтрация просроченных
                    filteredNotes = filteredNotes.Where(note => note.DueDate < DateTime.Now && !note.IsCompleted).ToList();
                    break;
                default:
                    break;
            }
            NotesList.ItemsSource = filteredNotes;

        }

        // обновление списка заметок
        private void RefreshNotesList()
        {
            foreach (var note in Notes)
            {
                if (note.DueDate < DateTime.Now && !note.IsCompleted)
                {
                    note.BackgroundColor = "#ff9c9c";
                }
                else
                {
                    note.BackgroundColor = note.IsCompleted ? "#8fffaf" : "#d9d9d9";
                }

                note.TextDecoration = note.IsCompleted ? TextDecorations.Strikethrough : null;
            }

            Notes = Notes.OrderBy(note => note.DueDate).ToList();
            NotesList.ItemsSource = null;
            NotesList.ItemsSource = Notes;
        }

        // возможность клика на заметку для отметки готовности и наоборот
        private void NoteTextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is DailyNote note)
            {
                note.IsCompleted = !note.IsCompleted;
                SaveNotes();
                RefreshNotesList();
            }
        }

        // метод для выбора даты 
        private void OpenDatePicker_Click(object sender, RoutedEventArgs e)
        {
            // Создание всплывающего окна с DatePicker
            var datePickerPopup = new Popup
            {
                Placement = PlacementMode.MousePoint,
                StaysOpen = false
            };
            // Ограничение выбора даты начиная с текущей
            var datePicker = new DatePicker
            {
                SelectedDate = SelectedDate,
                DisplayDateStart = DateTime.Today
            };

            // Обработчик выбора даты
            datePicker.SelectedDateChanged += (s, args) =>
            {
                if (datePicker.SelectedDate.HasValue)
                {
                    DateTime selected = datePicker.SelectedDate.Value;

                    // Проверка на дату в прошлом
                    if (selected < DateTime.Today)
                    {
                        MessageBox.Show("Выбранная дата не может быть в прошлом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        datePicker.SelectedDate = null;
                    }
                    else
                    {
                        SelectedDate = selected;
                    }
                }

                // Закрытие всплывающего окна
                datePickerPopup.IsOpen = false;
            };

            // Добавляем DatePicker в всплывающее окно
            datePickerPopup.Child = datePicker;
            datePickerPopup.IsOpen = true;
        }

        // метод на переключение на окно выбора времени
        private void SelectTimeButton_Click(object sender, RoutedEventArgs e)
        {
            var timePicker = new TimePickerWindow();
            if (timePicker.ShowDialog() == true)
            {
                SelectedTime = timePicker.SelectedTime;
            }
        }

        // реализация создания заметки на кнопку и проверка на правильность свойств заметки
        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string noteText = NewNoteTextBox.Text.Trim();
            DateTime? selectedDate = SelectedDate;

            if (string.IsNullOrWhiteSpace(noteText))
            {
                MessageBox.Show("Введите текст заметки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!selectedDate.HasValue || !SelectedTime.HasValue)
            {
                MessageBox.Show("Укажите дату и время выполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime dueDateTime = selectedDate.Value.Date + SelectedTime.Value;

            if (dueDateTime < DateTime.Now)
            {
                MessageBox.Show("Дата и время выполнения не могут быть в прошлом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newNote = new DailyNote
            {
                Text = noteText,
                DueDate = dueDateTime,
                IsCompleted = false
            };

            Notes.Add(newNote);
            SaveNotes();
            RefreshNotesList();

            ShowNoteCreatedNotification(newNote);

            NewNoteTextBox.Clear();
            SelectedDate = null;
            SelectedTime = null;
        }

        // поддержка возможности создания заметки на Enter, вместо кнопки
        private void NewNoteTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                AddNoteButton_Click(sender, e);
            }
        }

        // Уведомление после создания заметки
        private void ShowNoteCreatedNotification(DailyNote newNote)
        {
            string timeRemaining = newNote.RemainingTime;

            string notificationContent = $"Время на выполнение: {timeRemaining}";
            ShowToastNotification("Заметка создана", notificationContent);
        }

        // Подгрузка заметок
        private void LoadNotes()
        {
            try
            {
                if (File.Exists(NotesFilePath))
                {
                    string json = File.ReadAllText(NotesFilePath);
                    Notes = JsonConvert.DeserializeObject<List<DailyNote>>(json) ?? new List<DailyNote>();
                    RefreshNotesList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заметок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Сохранение заметки
        private void SaveNotes()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Notes, Formatting.Indented);
                File.WriteAllText(NotesFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении заметок: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод по удалению заметки из json файла и списка в приложении на кнопку удаления
        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.DataContext is DailyNote noteToDelete)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить заметку \"{noteToDelete.Text}\"?",
                                              "Подтверждение удаления",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    Notes.Remove(noteToDelete);
                    SaveNotes();
                    RefreshNotesList();
                }
            }
        }
    }
    
}