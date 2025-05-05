using System.Windows;

namespace TP.ConcurrentProgramming.PresentationView
{
    public partial class BallCountWindow : Window
    {
        public int BallCount { get; private set; }

        public BallCountWindow()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(BallCountTextBox.Text, out int count) && count > 0)
            {
                BallCount = count;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Wprowadź poprawną liczbę kulek (> 0).");
            }
        }
    }
}
