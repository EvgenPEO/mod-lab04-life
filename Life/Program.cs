using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace cli_life
{
    public class Cell
    {
        public bool IsAlive;
        public readonly List<Cell> neighbors = new List<Cell>();
        public bool IsAliveNext;
        public void DetermineNextLiveState()
        {
            int liveNeighbors = neighbors.Where(x => x.IsAlive).Count();
            if (IsAlive)
                IsAliveNext = liveNeighbors == 2 || liveNeighbors == 3;
            else
                IsAliveNext = liveNeighbors == 3;
        }
        public void Advance()
        {
            IsAlive = IsAliveNext;
        }
    }
    public class Board
    {
        public readonly Cell[,] Cells;
        public readonly int CellSize;

        private bool isStable;
        public bool IsStable { get { return isStable; } }
        private int generationsSinceStable = 0;
        public int gen = 0;

        public int Columns { get { return Cells.GetLength(0); } }
        public int Rows { get { return Cells.GetLength(1); } }
        public int Width { get { return Columns * CellSize; } }
        public int Height { get { return Rows * CellSize; } }

        public Board(int width, int height, int cellSize, double liveDensity = .1)
        {
            CellSize = cellSize;

            Cells = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Cells[x, y] = new Cell();

            ConnectNeighbors();
            Randomize(liveDensity);
        }

        readonly Random rand = new Random();
        public void Randomize(double liveDensity)
        {
            foreach (var cell in Cells)
                cell.IsAlive = rand.NextDouble() < liveDensity;
        }

        public void Advance()
        {
            gen++;
            bool wasStable = isStable;
            isStable = true;
            foreach (var cell in Cells)
            {
                cell.DetermineNextLiveState();
                if (cell.IsAlive != cell.IsAliveNext)
                {
                    isStable = false;
                }
                cell.Advance();
            }
            if (isStable && !wasStable)
            {
                Console.WriteLine($"стабильная фаза достигнута на {gen} поколении");
                generationsSinceStable = 0;
            }
            else if (isStable && wasStable)
            {
                generationsSinceStable++;
                if (generationsSinceStable == 10)
                {
                    Console.WriteLine("стабильная фаза продолжается...");
                    generationsSinceStable = 0;
                }
            }
            else
            {
                generationsSinceStable = 0;
            }
        }
        private void ConnectNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int xL = (x > 0) ? x - 1 : Columns - 1;
                    int xR = (x < Columns - 1) ? x + 1 : 0;

                    int yT = (y > 0) ? y - 1 : Rows - 1;
                    int yB = (y < Rows - 1) ? y + 1 : 0;

                    Cells[x, y].neighbors.Add(Cells[xL, yT]);
                    Cells[x, y].neighbors.Add(Cells[x, yT]);
                    Cells[x, y].neighbors.Add(Cells[xR, yT]);
                    Cells[x, y].neighbors.Add(Cells[xL, y]);
                    Cells[x, y].neighbors.Add(Cells[xR, y]);
                    Cells[x, y].neighbors.Add(Cells[xL, yB]);
                    Cells[x, y].neighbors.Add(Cells[x, yB]);
                    Cells[x, y].neighbors.Add(Cells[xR, yB]);
                }
            }
        }
        public void LoadB(string file)
        {
            string[] lines = File.ReadAllLines(file);
            string[] size = lines[0].Split(',');
            int columns = int.Parse(size[0]);
            int rows = int.Parse(size[1]);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    Cells[col, row].IsAlive = lines[row + 1][col] == '1';
                }
            }
        }
        public void SaveB(string file)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Columns},{Rows}");

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    sb.Append(Cells[col, row].IsAlive ? '1' : '0');
                }
                sb.AppendLine();
            }

            File.WriteAllText(file, sb.ToString());
        }
        public static (int, int) CountLiveCellsAndCombinations(Board board)
        {
            int liveCells = 0;
            HashSet<int> combinations = new HashSet<int>();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        liveCells++;

                        int combination = 0;
                        foreach (var neighbor in cell.neighbors)
                        {
                            if (neighbor.IsAlive)
                            {
                                combination |= 1 << neighbor.neighbors.IndexOf(cell);
                            }
                        }
                        combinations.Add(combination);
                    }
                }
            }

            return (liveCells, combinations.Count);
        }
        public bool CheckPattern(int startX, int startY, bool[,] pattern)
        {
            int patternWidth = pattern.GetLength(0);
            int patternHeight = pattern.GetLength(1);

            for (int x = 0; x < patternWidth; x++)
            {
                for (int y = 0; y < patternHeight; y++)
                {
                    int boardX = startX + x;
                    int boardY = startY + y;

                    if (boardX < 0 || boardX >= Columns || boardY < 0 || boardY >= Rows)
                    {
                        return false;
                    }

                    bool cellAlive = Cells[boardX, boardY].IsAlive;
                    bool patternAlive = pattern[x, y];

                    if (cellAlive != patternAlive)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public int CountSymmetricalCells(out int combinations)
        {
            int count = 0;
            combinations = 0;

            for (int x = 0; x < Columns / 2; x++)
            {
                for (int y = 0; y < Rows / 2; y++)
                {
                    var cell = Cells[x, y];
                    if (cell.IsAlive && cell.IsAlive == Cells[Columns - 1 - x, Rows - 1 - y].IsAlive)
                    {
                        count++;
                    }
                    if (cell.IsAlive && Cells[Columns - 1 - x, Rows - 1 - y].IsAlive)
                    {
                        combinations++;
                    }
                }
            }

            if (Columns % 2 == 1)
            {
                int middleX = Columns / 2;
                for (int y = 0; y < Rows / 2; y++)
                {
                    var cell = Cells[middleX, y];
                    if (cell.IsAlive && cell.IsAlive == Cells[middleX, Rows - 1 - y].IsAlive)
                    {
                        count++;
                    }
                    if (cell.IsAlive && Cells[middleX, Rows - 1 - y].IsAlive)
                    {
                        combinations++;
                    }
                }
            }

            if (Rows % 2 == 1)
            {
                int middleY = Rows / 2;
                for (int x = 0; x < Columns / 2; x++)
                {
                    var cell = Cells[x, middleY];
                    if (cell.IsAlive && cell.IsAlive == Cells[Columns - 1 - x, middleY].IsAlive)
                    {
                        count++;
                    }
                    if (cell.IsAlive && Cells[Columns - 1 - x, middleY].IsAlive)
                    {
                        combinations++;
                    }
                }
            }

            if (Columns % 2 == 1 && Rows % 2 == 1)
            {
                if (Cells[Columns / 2, Rows / 2].IsAlive)
                {
                    count++;
                    combinations++;
                }
            }

            return count;
        }
    }
    class Program
    {
        static Board board;

        static private void Reset()
        {
            var settingsJson = File.ReadAllText("SettingsBoard.json");
            var settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(settingsJson);

            var width = Convert.ToInt32((long)settings["width"]);
            var height = Convert.ToInt32((long)settings["height"]);
            var cellSize = Convert.ToInt32((long)settings["cellSize"]);
            var liveDensity = (double)settings["liveDensity"];

            board = new Board(width, height, cellSize, liveDensity);
        }
        static void Render()
        {
            int count = 0;
            HashSet<string> combinations = new HashSet<string>();

            for (int row = 0; row < board.Rows; row++)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    var cell = board.Cells[col, row];
                    if (cell.IsAlive)
                    {
                        Console.Write('*');
                        count++;

                        if (board.Columns % 2 == 0 && board.Rows % 2 == 0)
                        {
                            int x = col < board.Columns / 2 ? board.Columns - 1 - col : col;
                            int y = row < board.Rows / 2 ? board.Rows - 1 - row : row;
                            var symmetricalCell = board.Cells[x, y];
                            if (symmetricalCell.IsAlive)
                            {
                                string combination = $"{col},{row};{x},{y}";
                                combinations.Add(combination);
                            }
                        }
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.Write('\n');
            }
            Console.WriteLine($"количество симметричных живых клеток - {count}");
            Console.WriteLine($"количество симметричных комбинаций - {combinations.Count}");
        }
        static void Main(string[] args)
        {
            Reset();
            bool[,] blinkerPattern = new bool[,]
            {
                { false, true, false },
                { false, true, false },
                { false, true, false }
            };
            while (true)
            {
                Console.Clear();
                Render();
                board.Advance();
                var result = Board.CountLiveCellsAndCombinations(board);
                Console.WriteLine("NumPad1 - фигура 1");
                Console.WriteLine("NumPad2 - фигура 2");
                Console.WriteLine("NumPad3 - фигура 3");
                Console.WriteLine("s - сохранить, l - загрузить.");
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.NumPad1)
                    {
                        board.LoadB("State1.txt");
                    }
                    else if (key == ConsoleKey.NumPad2)
                    {
                        board.LoadB("State2.txt");
                    }
                    else if (key == ConsoleKey.NumPad3)
                    {
                        board.LoadB("State3.txt");
                    }
                    else if (key == ConsoleKey.S)
                    {
                        board.SaveB("StateB.txt");
                        Console.WriteLine("сохранили");
                    }
                    else if (key == ConsoleKey.L)
                    {
                        board.LoadB("StateB.txt");
                        Console.WriteLine("загрузили");
                    }
                }
                Console.WriteLine($"живые клетки - {result.Item1}");
                Console.WriteLine($"комбинации - {result.Item2}");
                if (board.CheckPattern(0, 0, blinkerPattern))
                {
                    Console.WriteLine("мигалка обнаружена");
                    //Thread.Sleep(1000);
                }
                if (board.IsStable)
                    break;
                Thread.Sleep(1);
            }
        }
    }
}