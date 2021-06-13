using System;
using System.Collections.Generic;
using System.Linq;
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

        public static readonly DependencyProperty GridCodeProperty =
            DependencyProperty.Register("GridCode", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        internal TileGrid grid;
        private ContentControl[,] buttons;
        private Solver solver;
        private readonly Random random = new Random();

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

        public int GridCode
        {
            get { return (int)GetValue(GridCodeProperty); }
            set { SetValue(GridCodeProperty, value); }
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
            return (ControlTemplate)(state ? Application.Current.FindResource("BlackTile") : Application.Current.FindResource("WhiteTile"));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == GridCodeProperty)
            {
                grid.LoadInt32((int)e.NewValue);
                UpdateGridUI();
            }
        }

        private void InitializeTileGrid()
        {
            grid = new TileGrid(GridSize);
            uniformGrid.Children.Clear();
            buttons = new ContentControl[GridSize, GridSize];
            for (int y = GridSize - 1; y >= 0; y--)
            {
                for (int x = 0; x < GridSize; x++)
                {
                    var button = new ContentControl
                    {
                        Template = (ControlTemplate)System.Windows.Application.Current.FindResource("WhiteTile")
                    };
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
                buttons[x, y].Content = grid.ImbueState[x, y] ? "1" : string.Empty;
                SetValue(GridCodeProperty, grid.ToInt32());
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
                for (int x = 0; x < GridSize; x++)
                    FlipSingle(x, y);
        }

        private void ResetGrid()
        {
            grid.Reset();
            UpdateGridUI();
        }

        private void Log(string text)
        {
            statusTextBox.AppendText(text + Environment.NewLine);
            statusTextBox.ScrollToEnd();
        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InitializeTileGrid();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetGrid();
        }

        private void InvertColorsButton_Click(object sender, RoutedEventArgs e)
        {
            InvertColors();
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            solveButton.IsEnabled = false;
            var gridSize = GridSize;
            var gridCode = grid.ToInt32();
            if (solver == null || solver.Size != gridSize)
                solver = new Solver(gridSize);
            var (codes, score) = solver.Solve(grid);
            Log($"[{gridCode}, {gridSize}x{gridSize}] Steps = {score}, Solutions = {string.Join(", ", from c in codes select solver.ToChessString(c))}");
            solveButton.IsEnabled = true;
            UpdateGridUI();

            //var histogram = new int[10];
            //for (int i = 0; ; i++)
            //{
            //    var gridCode = random.Next(1 << (gridSize * gridSize));
            //    var (_, score) = await Task.Run(() => solver.Solve(gridCode));
            //    ++histogram[score];
            //    if (score >= 8)
            //        Log($"{(gridCode, score)}");
            //    if (i % 10 == 0)
            //        Log(string.Join(", ", from j in Enumerable.Range(0, 10) where histogram[j] != 0 select $"({j}, {histogram[j]})"));
            //}
            //var scores = await Task.Run(() => solver.GetAllScores());
            //using var file = File.OpenWrite("scores.bin");
            //file.Write(scores);
            //solveButton.IsEnabled = true;
            //var scores = File.ReadAllBytes("scores.bin");
            //var codes9 = from i in Enumerable.Range(0, scores.Length)
            //             where scores[i] == 9
            //             where solver.FlipActions.Contains(i)
            //             select i;
            //Log(string.Join(", ", codes9));
        }

        private void UnsolveButton_Click(object sender, RoutedEventArgs e)
        {
            for (int y = 0; y < GridSize; y++)
                for (int x = 0; x < GridSize; x++)
                    if (grid.ImbueState[x, y])
                        Flip(x, y);
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
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
