using Microsoft.VisualStudio.TestTools.UnitTesting;
using cli_life;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestBoardCreation()
        {
            Board board = new Board(200, 200, 5, 0.5);
            Assert.AreEqual(200 / 5, board.Columns);
            Assert.AreEqual(200 / 5, board.Rows);
            Assert.AreEqual(200, board.Width);
            Assert.AreEqual(200, board.Height);
            Assert.IsFalse(board.IsStable);
        }

        [TestMethod]
        public void TestBoardAdvancement()
        {
            Board board = new Board(200, 200, 5, 0.5);
            int liveCellsBefore = board.Cells.Cast<Cell>().Count(cell => cell.IsAlive);

            board.Advance();
            int liveCellsAfter = board.Cells.Cast<Cell>().Count(cell => cell.IsAlive);

            Assert.AreNotEqual(liveCellsBefore, liveCellsAfter);
        }

        [TestMethod]
        public void TestBoardLoad()
        {
            Board board = new Board(200, 200, 5, 0.5);
            board.SaveB("test_board.txt");
            Board newBoard = new Board(200, 200, 5);
            newBoard.LoadB("test_board.txt");

            bool equal = true;
            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    if (board.Cells[x, y].IsAlive != newBoard.Cells[x, y].IsAlive)
                    {
                        equal = false;
                        break;
                    }
                }
                if (!equal)
                    break;
            }
            Assert.IsTrue(equal);
        }


        [TestMethod]
        public void TestBoardSave()
        {
            Board board = new Board(200, 200, 5, 0.5);
            board.SaveB("test_board.txt");
            Board newBoard = new Board(200, 200, 5);
            newBoard.LoadB("test_board.txt");

            bool equal = true;
            for (int x = 0; x < board.Columns; x++)
            {
                for (int y = 0; y < board.Rows; y++)
                {
                    if (board.Cells[x, y].IsAlive != newBoard.Cells[x, y].IsAlive)
                    {
                        equal = false;
                        break;
                    }
                }
                if (!equal)
                    break;
            }
            Assert.IsTrue(equal);
        }

        [TestMethod]
        public void TestCountLiveCellsAndCombinations()
        {
            Board board = new Board(200, 200, 5, 0.5);
            board.Advance();
            board.Advance();
            board.Advance();
            var result = Board.CountLiveCellsAndCombinations(board);
            Assert.IsTrue(result.Item1 > 0);
            Assert.IsTrue(result.Item2 > 0);
        }
    }
}