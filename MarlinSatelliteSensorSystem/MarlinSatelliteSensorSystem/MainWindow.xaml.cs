using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MarlinSatelliteSensorSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LinkedList<double> sensorA = new LinkedList<double>();
        LinkedList<double> sensorB = new LinkedList<double>();

        Dictionary<string, double> analyticsA = new Dictionary<string, double>();
        Dictionary<string, double> analyticsB = new Dictionary<string, double>();
        public MainWindow()
        {
            InitializeComponent();
        }

        public void LoadData(double mu, double sigma)
        {
            sensorA.Clear();
            sensorB.Clear();

            Galileo6.ReadData dataMaker = new Galileo6.ReadData();
            sensorA.AddFirst(dataMaker.SensorA(mu, sigma));
            sensorB.AddFirst(dataMaker.SensorB(mu, sigma));

            for (int i = 0; i < 399; i++)
            {
                sensorA.AddLast(dataMaker.SensorA(mu, sigma));
                sensorB.AddLast(dataMaker.SensorB(mu, sigma));
            }
        }

        public void ShowAllSensorData()
        {
            DoubleDataView.Items.Clear();

            var nodeA = sensorA.First;
            var nodeB = sensorB.First;

            while (nodeA != null && nodeB != null)
            {
                DoubleDataView.Items.Add(
                    new ListViewRow
                    {
                        ValueA = nodeA.Value,
                        ValueB = nodeB.Value
                    });

                nodeA = nodeA.Next;
                nodeB = nodeB.Next;
            }
        }



        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MeanInput.Text.ToLower(), out var mean) && double.TryParse(DevInput.Text.ToLower(), out var standardDev))
            {
                LoadData(mean, standardDev);
                ShowAllSensorData();
            }
            else
            {
                MessageBox.Show("Please ensure that the Mean and Standard Dev inputs are valid");
            }
        }
    }
}