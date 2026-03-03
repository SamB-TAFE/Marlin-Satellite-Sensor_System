using System.Diagnostics;
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

        bool isSortedA = false;
        bool isSortedB = false;

        Dictionary<string, double> analyticsA = new Dictionary<string, double>();
        Dictionary<string, double> analyticsB = new Dictionary<string, double>();
        public MainWindow()
        {
            InitializeComponent();
            SearchButtonA.IsEnabled = false;
            SearchButtonB.IsEnabled = false;
        }

        public void LoadData(double mu, double sigma)
        {
            sensorA.Clear();
            sensorB.Clear();

            analyticsA.Clear();
            analyticsB.Clear();

            Galileo6.ReadData dataMaker = new Galileo6.ReadData();
            sensorA.AddFirst(dataMaker.SensorA(mu, sigma));
            sensorB.AddFirst(dataMaker.SensorB(mu, sigma));

            for (int i = 1; i < 400; i++)
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

        public static int NumberOfNodes(LinkedList<double> linkList)
        {
            int count = 0;
            var node = linkList.First;
            while (node != null)
            {
                count++;
                node = node.Next;
            }
            return count;
        }

        public static void DisplayListboxData(LinkedList<double> linkList, ListBox listBox)
        {
            listBox.Items.Clear();

            var node = linkList.First;
            while (node != null)
            {
                listBox.Items.Add(node.Value);
                node = node.Next;
            }
        }

        public static bool SelectionSort(LinkedList<double> linkList)
        {
            int min = 0;
            int max = NumberOfNodes(linkList);

            for (int i = 0; i < (max - 1); i++)
            {
                min = i;

                for (int j = i + 1; j < (max - 1); j++)
                {
                    if (linkList.ElementAt(j) <  linkList.ElementAt(min))
                    {
                        min = j;
                    }
                }

                LinkedListNode<double> currentMin = linkList.Find(linkList.ElementAt(min));
                LinkedListNode<double> currentI = linkList.Find(linkList.ElementAt(i));

                var temp = currentMin.Value;
                currentMin.Value = currentI.Value;
                currentI.Value = temp;
            }

            return true;
        }
        
        public static bool InsertionSort(LinkedList<double> linkList)
        {
            int max = NumberOfNodes(linkList);

            for (int i = 0; i < (max - 1); i++)
            {
                for (int j = i + 1; j > 0; j--)
                {
                    if (linkList.ElementAt(j - 1) > linkList.ElementAt(j))
                    {
                        LinkedListNode<double> current = linkList.Find(linkList.ElementAt(j));
                        LinkedListNode<double> previous = linkList.Find(linkList.ElementAt(j - 1));

                        var temp = current.Value;
                        current.Value = previous.Value;
                        previous.Value = temp;
                    }
                }
            }

            return true;
        }

        public static int BinarySearchIterative(LinkedList<double> linkList, double searchValue, int listMin, int listMax)
        {
            while (listMin <= listMax - 1)
            {
                int middle = (listMin + listMax) / 2;

                var middleElement = linkList.ElementAt(middle);

                if (searchValue == middleElement)
                {
                    return middle;
                }
                else if (searchValue < middleElement)
                {
                    listMax = middle - 1;
                }
                else
                {
                    listMin = middle + 1;
                }
            }

            return listMin;
        }

        public static int BinarySearchRecursive(LinkedList<double> linkList, double searchValue, int listMin, int listMax)
        {
            if (listMin <= listMax - 1)
            {
                int middle = (listMin + listMax) / 2;

                var middleElement = linkList.ElementAt(middle);

                if (searchValue == middleElement)
                {
                    return middle;
                }
                else if (searchValue < middleElement)
                {
                    return BinarySearchRecursive(linkList, searchValue, listMin, middle - 1);
                }
                else
                {
                    return BinarySearchRecursive(linkList, searchValue, middle + 1, listMax);
                }
            }
            return listMin;
        }

        public static string GatherAnalytics(Stopwatch timer, Dictionary<string, double> analytics, string functionTimed)
        {
            double.TryParse(timer.ElapsedMilliseconds.ToString(), out double elapsedTime);
            analytics.Add(functionTimed, elapsedTime);
            return timer.ElapsedMilliseconds.ToString();
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MeanInput.Text.ToLower(), out var mean) && double.TryParse(DevInput.Text.ToLower(), out var standardDev))
            {
                LoadData(mean, standardDev);
                ShowAllSensorData();
                DisplayListboxData(sensorA, SensorAListBox);
                DisplayListboxData(sensorB, SensorBListBox);
            }
            else
            {
                MessageBox.Show("Please ensure that the Mean and Standard Dev inputs are valid");
            }
        }

        private void SearchButtonA_Click(object sender, RoutedEventArgs e)
        {
            if (isSortedA)
            {
                if (double.TryParse(SearchAInput.Text.ToLower(), out double searchTerm))
                {
                    if (RecursiveSearchA.IsChecked == true)
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int listBoxIndex = BinarySearchRecursive(sensorA, searchTerm, 0, NumberOfNodes(sensorA));
                        stopwatch.Stop();
                        string elapsed = GatherAnalytics(stopwatch, analyticsA, "recursive");
                        RecursiveSpeedADisplay.Text = elapsed + " ms";
                        SensorAListBox.SelectedIndex = listBoxIndex;
                    }
                    else
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int listBoxIndex = BinarySearchIterative(sensorA, searchTerm, 0, NumberOfNodes(sensorA));
                        stopwatch.Stop();
                        string elapsed = GatherAnalytics(stopwatch, analyticsA, "iterative");
                        IterativeSpeedADisplay.Text = elapsed + " ms";
                        SensorAListBox.SelectedIndex = listBoxIndex;
                    }
                }
                else
                {
                    MessageBox.Show("Please input a valid number.");
                }

            }
            else
            {
                MessageBox.Show("Please Sort before trying to Search.");
            }
        }

        private void SortButtonA_Click(object sender, RoutedEventArgs e)
        {
            if (SelectSortA.IsChecked == true)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                SelectionSort(sensorA);
                stopwatch.Stop();
                string elapsed = GatherAnalytics(stopwatch, analyticsA, "selection");
                SelectionSpeedADisplay.Text = elapsed + " ms";
                ShowAllSensorData();
                DisplayListboxData(sensorA, SensorAListBox);
            }
            else
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                InsertionSort(sensorA);
                stopwatch.Stop();
                string elapsed = GatherAnalytics(stopwatch, analyticsA, "insertion");
                InsertionSpeedADisplay.Text = elapsed + " ms";
                ShowAllSensorData();
                DisplayListboxData(sensorA, SensorAListBox);
            }
            if (isSortedA == false)
            {
                isSortedA = true;
            }
        }

        private void SearchButtonB_Click(object sender, RoutedEventArgs e)
        {
            if (isSortedB)
            {
                if (double.TryParse(SearchBInput.Text.ToLower(), out double searchTerm))
                {
                    if (RecursiveSearchB.IsChecked == true)
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int listBoxIndex = BinarySearchRecursive(sensorB, searchTerm, 0, NumberOfNodes(sensorB));
                        stopwatch.Stop();
                        string elapsed = GatherAnalytics(stopwatch, analyticsB, "recursive");
                        RecursiveSpeedBDisplay.Text = elapsed + " ms";
                        SensorBListBox.SelectedIndex = listBoxIndex;
                    }
                    else
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int listBoxIndex = BinarySearchIterative(sensorB, searchTerm, 0, NumberOfNodes(sensorB));
                        stopwatch.Stop();
                        string elapsed = GatherAnalytics(stopwatch, analyticsB, "iterative");
                        IterativeSpeedBDisplay.Text = elapsed + " ms";
                        SensorBListBox.SelectedIndex = listBoxIndex;
                    }
                }
                else
                {
                    MessageBox.Show("Please input a valid number.");
                }

            }
            else
            {
                MessageBox.Show("Please Sort before trying to Search.");
            }
        }

        private void SortButtonB_Click(object sender, RoutedEventArgs e)
        {
            if (SelectSortB.IsChecked == true)
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                SelectionSort(sensorB);
                stopwatch.Stop();
                string elapsed = GatherAnalytics(stopwatch, analyticsB, "selection");
                SelectionSpeedBDisplay.Text = elapsed + " ms";
                ShowAllSensorData();
                DisplayListboxData(sensorB, SensorBListBox);
            }
            else
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                InsertionSort(sensorB);
                stopwatch.Stop();
                string elapsed = GatherAnalytics(stopwatch, analyticsB, "insertion");
                InsertionSpeedBDisplay.Text = elapsed + " ms";
                ShowAllSensorData();
                DisplayListboxData(sensorB, SensorBListBox);
            }
            if (isSortedB == false)
            {
                isSortedB = true;
            }
        }
    }
}