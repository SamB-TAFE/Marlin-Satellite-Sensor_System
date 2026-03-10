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
            SortButtonA.IsEnabled = false;
            SortButtonB.IsEnabled = false;
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

            for (int i = 0; i < max; i++)
            {
                min = i;

                for (int j = i + 1; j < max; j++)
                {
                    if (linkList.ElementAt(j) <  linkList.ElementAt(min))
                    {
                        min = j;
                    }
                }

                LinkedListNode<double> currentMin = GetNodeAt(min, linkList);
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
                        LinkedListNode<double> current = GetNodeAt(j, linkList);
                        LinkedListNode<double> previous = GetNodeAt(j - 1, linkList);

                        var temp = current.Value;
                        current.Value = previous.Value;
                        previous.Value = temp;
                    }
                }
            }

            return true;
        }

        private bool IsSorted(LinkedList<double> sensorData)
        {
            int numberOfNodes = NumberOfNodes(sensorData);

            for (int i = 0; i < numberOfNodes - 1; i++)
            {
                if (sensorData.ElementAt(i) > sensorData.ElementAt(i + 1))
                {
                    return false;
                }
            }
            return true;
        }

        private static LinkedListNode<double> GetNodeAt(int index, LinkedList<double> linkList)
        {
            int count = 0;
            var node = linkList.First;
            while (count != index)
            {
                count++;
                node = node.Next;
            }
            return node;
        }

        public static int BinarySearchIterative(LinkedList<double> linkList, double searchValue, int listMin, int listMax)
        {
            while (listMin <= listMax)
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
            if (listMin <= listMax)
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
            double.TryParse(timer.Elapsed.TotalMilliseconds.ToString(), out double elapsedTime);
            analytics.Remove(functionTimed);
            analytics.Add(functionTimed, elapsedTime);
            return timer.Elapsed.TotalMilliseconds.ToString();
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(MeanInput.Text.ToLower(), out var mean) && double.TryParse(DevInput.Text.ToLower(), out var standardDev))
            {
                LoadData(mean, standardDev);
                ShowAllSensorData();
                DisplayListboxData(sensorA, SensorAListBox);
                DisplayListboxData(sensorB, SensorBListBox);
                NewDataReset();
            }
            else
            {
                MessageBox.Show("Please ensure that the Mean and Standard Dev inputs are valid");
            }
        }

        private void NewDataReset()
        {
            SortButtonA.IsEnabled = true;
            SortButtonB.IsEnabled = true;
            SearchButtonA.IsEnabled = false;
            SearchButtonB.IsEnabled = false;
            SearchAInput.Clear();
            SearchBInput.Clear();
            RecursiveSpeedADisplay.Clear();
            RecursiveSpeedBDisplay.Clear();
            IterativeSpeedADisplay.Clear();
            IterativeSpeedBDisplay.Clear();
            SelectionSpeedADisplay.Clear();
            SelectionSpeedBDisplay.Clear();
            InsertionSpeedADisplay.Clear();
            InsertionSpeedBDisplay.Clear();
        }

        private void SearchButtonA_Click(object sender, RoutedEventArgs e)
        {
            if (IsSorted(sensorA))
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
                        HighlightAllMatches(searchTerm, listBoxIndex, SensorAListBox, sensorA);
                    }
                    else
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int listBoxIndex = BinarySearchIterative(sensorA, searchTerm, 0, NumberOfNodes(sensorA));
                        stopwatch.Stop();
                        string elapsed = GatherAnalytics(stopwatch, analyticsA, "iterative");
                        IterativeSpeedADisplay.Text = elapsed + " ms";
                        HighlightAllMatches(searchTerm, listBoxIndex, SensorAListBox, sensorA);
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
                SearchButtonA.IsEnabled = true;
            }
        }

        private void SearchButtonB_Click(object sender, RoutedEventArgs e)
        {
            if (IsSorted(sensorB))
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
                        HighlightAllMatches(searchTerm, listBoxIndex, SensorBListBox, sensorB);
                    }
                    else
                    {
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        int listBoxIndex = BinarySearchIterative(sensorB, searchTerm, 0, NumberOfNodes(sensorB));
                        stopwatch.Stop();
                        string elapsed = GatherAnalytics(stopwatch, analyticsB, "iterative");
                        IterativeSpeedBDisplay.Text = elapsed + " ms";
                        HighlightAllMatches(searchTerm, listBoxIndex, SensorBListBox, sensorB);
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
                SearchButtonB.IsEnabled = true;
            }
        }

        private void HighlightAllMatches(double search, int resultIndex, ListBox listBox, LinkedList<double> linkList)
        {

            LinkedListNode<double> resultNode = GetNodeAt(resultIndex, linkList);
            double result = resultNode.Value;

            double floorSearch = Math.Floor(search);
            double floorResult = Math.Floor(result);

            if (search == result)
            {
                listBox.SelectedItem = linkList.ElementAt(resultIndex);
                
            }
            else if (floorSearch == floorResult)
            {
                listBox.SelectionMode = SelectionMode.Multiple;

                foreach (var item in listBox.Items)
                {
                    double listValue = (double)item;
                    if (Math.Floor(listValue) == floorSearch)
                    {
                        listBox.SelectedItems.Add(item);
                    }
                }
            }
            else
            {
                listBox.SelectedItem = linkList.ElementAt(resultIndex);
            }
        }

        private void SearchAInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SearchAInput.Text, out int parsedNumber))
            {
                MessageBox.Show("Please enter a valid number");
            }
        }

        private void SearchAInput_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchAInput.Clear();
        }

        private void SearchBInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(SearchBInput.Text, out int parsedNumber))
            {
                MessageBox.Show("Please enter a valid number");
            }
        }

        private void SearchBInput_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchBInput.Clear();
        }
    }
}