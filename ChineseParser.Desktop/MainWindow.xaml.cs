using ChineseParser.Parser;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChineseParser.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ParseResult ParseResult { get; set; }
        private bool _isRunning;
        private readonly ParserWorker _parseWorker;
        public MainWindow()
        {
            InitializeComponent();
            _parseWorker = new ParserWorker();
            ParseResult = new ParseResult();
            DataContext = ParseResult;
            KeyDown += MainWindow_KeyDown;
        }

        private async void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await Start();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Start();
        }

        private async Task Start()
        {
            if (_isRunning) return;
            if (string.IsNullOrWhiteSpace(searchTextBox.Text)) return;
            _isRunning = true;
            mainGrid.IsEnabled = false;

            try
            {
                var result = await _parseWorker.Parse(searchTextBox.Text);                
                DataContext = result;
                if (string.IsNullOrWhiteSpace(result.AudioLocalUri))
                {
                    var player = new MediaPlayer();
                    player.Open(new System.Uri(result.AudioLocalUri));
                    player.Volume = 100.0;
                    player.Play();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _isRunning = false;
                mainGrid.IsEnabled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox)sender;
            textBox.SelectAll();
            Clipboard.SetText(textBox.Text);
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void gifListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gifListView.SelectedValue != null)
            {
                var uri = gifListView.SelectedValue.ToString();
                Clipboard.SetText(uri);
            }
        }
    }
}
