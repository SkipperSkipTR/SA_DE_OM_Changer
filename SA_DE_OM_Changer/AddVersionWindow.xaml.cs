using System.Windows;

namespace SA_DE_OM_Changer
{
    public partial class AddVersionWindow : Window
    {
        public string GameVersion { get; private set; } = string.Empty;
        public string AddressHex { get; private set; } = string.Empty;

        public AddVersionWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VersionBox.Text) || string.IsNullOrWhiteSpace(AddressBox.Text))
            {
                MessageBox.Show("Please enter both version and address.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GameVersion = VersionBox.Text.Trim();
            AddressHex = AddressBox.Text.Trim(); // always hex, no 0x required

            DialogResult = true;
            Close();
        }
    }
}