using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LightsOut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly IEnumerable<int> GridSizes = Enumerable.Range(1, 16);

        internal TileGrid grid;
        ContentControl[,] buttons;
        Solver solver;
        Random random = new Random();

        public MainWindow()
        {
            InitializeComponent();
            sizeComboBox.SelectedIndex = 4;
            InitializeTileGrid();
        }

        public int GridSize
        {
            get { return uniformGrid.Rows; }
        }

        public void Flip(int x, int y)
        {
            grid.Imbue(x, y);
            UpdateButton(x, y);
            UpdateButton(x - 1, y);
            UpdateButton(x, y - 1);
            UpdateButton(x + 1, y);
            UpdateButton(x, y + 1);
        }

        public void FlipSingle(int x, int y)
        {
            grid.Force(x, y);
            UpdateButton(x, y);
        }

        static ControlTemplate GetTemplate(bool state)
        {
            return (ControlTemplate)(state ? App.Current.FindResource("BlackTile") : App.Current.FindResource("WhiteTile"));
        }

        private void InitializeTileGrid()
        {
            grid = new TileGrid(GridSize);
            uniformGrid.Children.Clear();
            buttons = new ContentControl[GridSize, GridSize];
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    var button = new ContentControl();
                    button.Template = (ControlTemplate)App.Current.FindResource("WhiteTile");
                    int xTemp = x, yTemp = y;
                    button.MouseDown += (object sender, MouseButtonEventArgs e) =>
                    {
                        switch (e.ChangedButton)
                        {
                            case MouseButton.Left:
                                Flip(xTemp, yTemp);
                                break;
                            case MouseButton.Right:
                                FlipSingle(xTemp, yTemp);
                                break;
                            default:
                                break;
                        }
                    };
                    buttons[x, y] = button;
                    uniformGrid.Children.Add(button);
                }
            }
            solveButton.IsEnabled = GridSize <= 5;
        }

        private void UpdateButton(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < GridSize && y < GridSize)
            {
                buttons[x, y].Template = GetTemplate(grid[x, y]);
                buttons[x, y].Content = grid.PressState[x, y] == 1 ? "1" : string.Empty;
            }
        }

        private void UpdateGridUI()
        {
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    UpdateButton(x, y);
        }

        private void InvertColors()
        {
            for (int y = 0; y < GridSize; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    FlipSingle(x, y);
                }
            }
        }

        private void DoChase1()
        {
            var list = new List<Point>();
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    if (grid[x, y])
                        list.Add(new Point(x, y));
            foreach (var point in list)
                Flip((int)point.X, (int)point.Y);
        }

        private void DoChase2()
        {
            // Find first row with black tiles.
            bool foundBlackTileRow = false;
            for (int y = 0; y < GridSize - 1; y++)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    if (grid[x, y])
                    {
                        foundBlackTileRow = true;
                        Flip(x, y + 1);
                    }
                }
                if (foundBlackTileRow)
                    break;
            }
        }

        private void ResetGrid()
        {
            grid.Reset();
            UpdateGridUI();
        }

        private void sizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeTileGrid();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGrid();
        }

        private void chase1Button_Click(object sender, RoutedEventArgs e)
        {
            DoChase1();
        }

        private void chase2Button_Click(object sender, RoutedEventArgs e)
        {
            DoChase2();
        }

        private void invertColorsButton_Click(object sender, RoutedEventArgs e)
        {
            InvertColors();
        }

        private async void solveButton_Click(object sender, RoutedEventArgs e)
        {
            solveButton.IsEnabled = false;
            var gridSize = GridSize;
            var solution = await Task.Run(() =>
            {
                if (solver == null || solver.Size != gridSize)
                    solver = new Solver(gridSize);
                return solver.Solve(grid);
            });
            statusTextBlock.Text = $"Code: {solver.ToChessString(solution.Code)}, Number of steps: {solution.Score}";
            solveButton.IsEnabled = true;
            UpdateGridUI();
        }

        private void unsolveButton_Click(object sender, RoutedEventArgs e)
        {
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    if (grid.PressState[x, y] == 1)
                        Flip(x, y);
        }

        private void randomButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGrid();
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    if (random.Next(2) == 0)
                        grid.Imbue(x, y, false);
            UpdateGridUI();
        }
    }
}
