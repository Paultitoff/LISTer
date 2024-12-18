using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LISTer
{
    /// <summary>
    /// Логика взаимодействия для TimePickerWindow.xaml
    /// </summary>
    public partial class TimePickerWindow : Window
    {
        public TimeSpan? SelectedTime { get; private set; }
        public TimePickerWindow()
        {
            InitializeComponent();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (HoursComboBox.SelectedItem == null || MinutesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите часы и минуты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int hours = int.Parse((HoursComboBox.SelectedItem as ComboBoxItem).Content.ToString());
            int minutes = int.Parse((MinutesComboBox.SelectedItem as ComboBoxItem).Content.ToString());

            SelectedTime = new TimeSpan(hours, minutes, 0);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedTime = null;
            DialogResult = false;
            Close();
        }
    }
}
