using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleChess
{
    public partial class Form1 : Form
    {
        private struct BestMove { public int firstIndex;public int lastIndex; }
        Button oldChecker = new Button();
        Image handPiece = null;
        Image grayBackgound = Properties.Resources.gray_background;
        Image redBackgound = Properties.Resources.red_background;
        string turn = "white";
        Checker[] board1d = new Checker[64];
        Checker[,] board2d = new Checker[8, 8];
        Button[] buttons1d = new Button[64];
        List<Checker[]> boardPositions = new List<Checker[]>();
        List<int> whitePieces = new List<int>();
        List<int> blackPieces = new List<int>();
        int currentBoardIndex = -1;
        bool vsComputer = true;
        int compLvl = 2;
        bool sound = true;
        System.Media.SoundPlayer soundClick = new System.Media.SoundPlayer();
        System.Media.SoundPlayer soundFail = new System.Media.SoundPlayer();
        System.Media.SoundPlayer soundIllegal = new System.Media.SoundPlayer();
        System.Media.SoundPlayer soundCheck = new System.Media.SoundPlayer();
        System.Media.SoundPlayer soundWin = new System.Media.SoundPlayer();


        public Form1()
        {
            InitializeComponent();
            soundClick.SoundLocation = "click01.wav";
            soundFail.SoundLocation = "fail.wav";
            soundIllegal.SoundLocation = "illegal.wav";
            soundCheck.SoundLocation = "check.wav";
            soundWin.SoundLocation = "win.wav";
            StartPosition = FormStartPosition.Manual;
            Location = new Point(50, 30);
            Height = 650;
            label1.Text = "White Move First";
            oldChecker = empty;
            Button[] buttons1d2 = { button0, button1, button2, button3, button4, button5, button6, button7, button8, button9,
                                    button10, button11, button12, button13, button14, button15, button16, button17, button18, button19,
                                    button20, button21, button22, button23, button24, button25, button26, button27, button28, button29,
                                    button30, button31, button32, button33, button34, button35, button36, button37, button38, button39,
                                    button40, button41, button42, button43, button44, button45, button46, button47, button48, button49,
                                    button50, button51, button52, button53, button54, button55, button56, button57, button58, button59,
                                    button60, button61, button62, button63};
            buttons1d = buttons1d2;
            foreach (Button button in buttons1d)
            {
                board1d[button.TabIndex] = new Checker();
                if (button.Tag != null)
                {
                    board1d[button.TabIndex].color = button.Tag.ToString();
                    board1d[button.TabIndex].name = button.AccessibleName.ToString();
                }
                board1d[button.TabIndex].index = button.TabIndex;
                board1d[button.TabIndex].moved = false;
                board1d[button.TabIndex].enPassant = false;
            }
            int i = 0;
            NewBoardPosition();
            while (i < 64)
            {
                board2d[(i / 8), (i % 8)] = board1d[i];
                i++;
            }
        }


        private async void Checker_Click(Button checker)
        {
            if (vsComputer && turn == "black") { return; }
            if (checker == oldChecker)
            {
                handPiece = null;
                oldChecker = empty;
                foreach (Button button in buttons1d)
                {
                    button.BackgroundImage = null;
                }
            }
            else if ((handPiece == null && checker.Tag != null && checker.Tag.ToString() == turn) ||
                (oldChecker.Tag == checker.Tag && handPiece != null))
            {
                if (handPiece != null)
                {
                    foreach (Button button in buttons1d)
                    {
                        button.BackgroundImage = null;
                    }
                }
                oldChecker = checker;
                handPiece = checker.Image;
                List<int> range = PossibleMoves(board1d[checker.TabIndex]);
                foreach (int i in range)
                {
                    buttons1d[i].BackgroundImage = grayBackgound;
                }
                checker.BackgroundImage = grayBackgound;
            }
            else if (checker.Tag != oldChecker.Tag && handPiece != null && checker.BackgroundImage == grayBackgound)
            {
                MakeMove(oldChecker.TabIndex, checker.TabIndex);
                if (IsCheck(turn))
                {
                    if (sound) { soundIllegal.Play(); }
                    label1.Text = "Iligal Move!";
                    BackMove();
                    boardPositions.RemoveAt(boardPositions.Count - 1);
                }
                else
                {
                    buttonForward.Enabled = false;
                    if (currentBoardIndex < boardPositions.Count - 1)
                    {
                        boardPositions.RemoveRange(currentBoardIndex, (boardPositions.Count - 1) - (currentBoardIndex));
                    }
                    DrawBoard(board1d);
                    handPiece = null;
                    turn = SwitchColor(turn);
                    label1.Text = turn + " turn";
                    if (IsCheck(turn))
                    {
                        if (IsCheckMate(turn))
                        {
                            if (sound) { soundWin.Play(); }
                            label1.Text = "CheckMate!!!";
                            foreach (Button button in buttons1d)
                            {
                                button.Enabled = false;
                            }
                            return;
                        }
                        else
                        {
                            if (sound)
                            {
                                soundCheck.Play();
                            }
                            await Task.Delay(200);
                            label1.Text = "Check!";
                        }
                    }else if (IsDrow(turn))
                    {
                        if (sound) { soundFail.Play(); }
                        label1.Text = "Draw!!!";
                        foreach (Button button in buttons1d)
                        {
                            button.Enabled = false;
                        }
                        return;
                    }
                    else
                    {
                        if (sound)
                        {
                            soundClick.Play();
                        }
                    }
                    if (turn == "black" && vsComputer)
                    {
                        label1.Text = "Computer thinking..";
                        buttonNew.Enabled = false;
                        buttonForward.Enabled = false;
                        buttonBack.Enabled = false;
                        checkBoxVS.Enabled = false;
                        radioLevel1.Enabled = false;
                        radioLevel2.Enabled = false;
                        radioLevel3.Enabled = false;
                        await Task.Delay(50);
                        ComputerTurn();
                    }
                }
            }
        }

        private string SwitchColor(string color)
        {
            if (color == "white")
            {
                return "black";
            }
            else
            {
                return "white";
            }
        }

        private void CountPieces()
        {
            whitePieces.Clear();
            blackPieces.Clear();
            foreach (Checker checker in board1d)
            {
                if (checker.color == "white")
                {
                    whitePieces.Add(checker.index);
                }else if (checker.color == "black")
                {
                    blackPieces.Add(checker.index);
                }
            }
        }

        private void BackMove()
        {
            currentBoardIndex--;
            GetBoard(currentBoardIndex);
            if (currentBoardIndex == 0) { buttonBack.Enabled = false; }
            CountPieces();
        }

        private void MakeMove(int indexFirst, int indexLast)
        {
            board1d[indexLast].color = board1d[indexFirst].color;
            board1d[indexLast].name = board1d[indexFirst].name;
            board1d[indexLast].moved = true;

            board1d[indexFirst].color = null;
            board1d[indexFirst].name = null;
            if (board1d[indexLast].name == "p")
            {
                int column = board1d[indexLast].index % 8;
                if (board1d[indexLast].index / 8 == 3 && board1d[indexFirst].index / 8 == 1)
                {
                    board2d[2, column].enPassant = true;
                }
                else if (board1d[indexLast].index / 8 == 4 && board1d[indexFirst].index / 8 == 6)
                {
                    board2d[5, column].enPassant = true;
                }
                else if (board1d[indexLast].enPassant && board1d[indexLast].index / 8 == 2)
                {
                    board2d[3, column].color = null;
                    board2d[3, column].name = null;
                }
                else if (board1d[indexLast].enPassant && board1d[indexLast].index / 8 == 5)
                {
                    board2d[4, column].color = null;
                    board2d[4, column].name = null;
                }
                else if (board1d[indexLast].index / 8 == 0 || board1d[indexLast].index / 8 == 7)
                {
                    board1d[indexLast].name = "q";
                }
            }
            else if (board1d[indexLast].name == "K" && !board1d[indexFirst].moved)
            {
                if (board1d[indexLast].index % 8 == 6)
                {
                    board1d[board1d[indexLast].index - 1].color = board1d[board1d[indexLast].index + 1].color;
                    board1d[board1d[indexLast].index - 1].name = "r";
                    board1d[board1d[indexLast].index + 1].color = null;
                    board1d[board1d[indexLast].index + 1].name = null;
                }
                else if (board1d[indexLast].index % 8 == 2)
                {
                    board1d[board1d[indexLast].index + 1].color = board1d[board1d[indexLast].index - 2].color;
                    board1d[board1d[indexLast].index + 1].name = "r";
                    board1d[board1d[indexLast].index - 2].color = null;
                    board1d[board1d[indexLast].index - 2].name = null;
                }
            }
            board1d[indexFirst].moved = true;
            EnPassantReset(board1d[indexLast].color);
            NewBoardPosition();
            buttonBack.Enabled = true;
            CountPieces();
        }

        private void NewBoardPosition()
        {
            Checker[] newBoardPosition = new Checker[64];
            foreach (Checker checker in board1d)
            {
                newBoardPosition[checker.index] = new Checker();
                newBoardPosition[checker.index].index = checker.index;
                newBoardPosition[checker.index].color = checker.color;
                newBoardPosition[checker.index].name = checker.name;
                newBoardPosition[checker.index].enPassant = checker.enPassant;
                newBoardPosition[checker.index].moved = checker.moved;
            }
            boardPositions.Add(newBoardPosition);
            currentBoardIndex++;
        }

        private void GetBoard(int boardPositionIndex)
        {
            foreach (Checker checker in board1d)
            {
                checker.color = boardPositions[boardPositionIndex][checker.index].color;
                checker.name = boardPositions[boardPositionIndex][checker.index].name;
                checker.enPassant = boardPositions[boardPositionIndex][checker.index].enPassant;
                checker.moved = boardPositions[boardPositionIndex][checker.index].moved;
            }
        }

        private void EnPassantReset(string color)
        {
            if (color == "white")
            {
                for (int i = 16; i < 24; i++)
                {
                    board1d[i].enPassant = false;
                }
            }
            else
            {
                for (int i = 40; i < 48; i++)
                {
                    board1d[i].enPassant = false;
                }
            }
        }


        private void DrawBoard(Checker[] board)
        {
            int i = 0;
            foreach (Button button in buttons1d)
            {
                button.BackgroundImage = null;
            }
            foreach (Button button in buttons1d)
            {
                button.Tag = board[i].color;
                button.AccessibleName = board[i].name;
                if (board[i].color == "white")
                {
                    switch (board[i].name)
                    {
                        case "p":
                            button.Image = global::SimpleChess.Properties.Resources.pawn_white;
                            break;
                        case "b":
                            button.Image = global::SimpleChess.Properties.Resources.bishop_white;
                            break;
                        case "k":
                            button.Image = global::SimpleChess.Properties.Resources.knight_white;
                            break;
                        case "q":
                            button.Image = global::SimpleChess.Properties.Resources.queen_white;
                            break;
                        case "r":
                            button.Image = global::SimpleChess.Properties.Resources.rock_white;
                            break;
                        case "K":
                            if (IsCheck("white")) 
                            { 
                                button.BackgroundImage = redBackgound;
                                buttons1d[IsThretened(board1d[button.TabIndex], "black")].BackgroundImage = redBackgound;
                            }
                            button.Image = global::SimpleChess.Properties.Resources.king_white;
                            break;
                    }
                }
                else if (board[i].color == "black")
                {
                    switch (board[i].name)
                    {
                        case "p":
                            button.Image = global::SimpleChess.Properties.Resources.pawn_black;
                            break;
                        case "b":
                            button.Image = global::SimpleChess.Properties.Resources.bishop_black;
                            break;
                        case "k":
                            button.Image = global::SimpleChess.Properties.Resources.knight_black;
                            break;
                        case "q":
                            button.Image = global::SimpleChess.Properties.Resources.queen_black;
                            break;
                        case "r":
                            button.Image = global::SimpleChess.Properties.Resources.rock_black;
                            break;
                        case "K":
                            button.Image = global::SimpleChess.Properties.Resources.king_black;
                            if (IsCheck("black"))
                            {
                                button.BackgroundImage = redBackgound;
                                buttons1d[IsThretened(board1d[button.TabIndex], "white")].BackgroundImage = redBackgound;
                            }
                            break;
                    }
                }
                else
                {
                    button.Image = global::SimpleChess.Properties.Resources.empty;
                }
                i++;
            }
        }

        private void NewBoard()
        {
            label1.Text = "White Move First";
            handPiece = null;
            oldChecker = empty;
            turn = "white";
            GetBoard(0);
            DrawBoard(board1d);
            for (int i = 0; i < 64; i++)
            {
                buttons1d[i].BackgroundImage = null;
                board1d[i].color = boardPositions[0][i].color;
                board1d[i].name = boardPositions[0][i].name;
                board1d[i].enPassant = boardPositions[0][i].enPassant;
                board1d[i].moved = boardPositions[0][i].moved;
            }
            foreach (Button button in buttons1d)
            {
                button.Enabled = true;
            }
            boardPositions.Clear();
            NewBoardPosition();
            currentBoardIndex = 0;
            buttonPlay.Enabled = false;
            buttonForward.Enabled = false;
            buttonBack.Enabled = false;
        }

        private int IsThretened(Checker checker, string color)
        {
            foreach (Checker oldChecker in board1d)
            {
                if (oldChecker.color == color)
                {
                    if (isValidMove(oldChecker, checker))
                    {
                        return oldChecker.index;
                    }
                }
            }
            return -1;
        }

        private bool IsCheck(string color)
        {
            string oppositColor = SwitchColor(color);
            foreach (Checker checker in board1d)
            {
                if (checker.color == color && checker.name == "K")
                {
                    if (IsThretened(board1d[checker.index], oppositColor) != -1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        private bool IsCheckMate(string color)
        {
            string oppositColor = SwitchColor(color);
            foreach (Checker checker in board1d)
            {
                if (checker.color == color)
                {
                    foreach (int moveIndex in PossibleMoves(checker))
                    {
                        MakeMove(checker.index, moveIndex);
                        if (!IsCheck(color))
                        {
                            BackMove();
                            boardPositions.RemoveAt(boardPositions.Count - 1);
                            EnPassantReset(oppositColor);
                            return false;
                        }
                        BackMove();
                        boardPositions.RemoveAt(boardPositions.Count - 1);
                        EnPassantReset(oppositColor);
                    }
                }
            }
            return true;
        }

        private bool IsDrow(string color)
        {
            foreach (Checker item1 in board1d)
            {
                if (item1.color == color)
                {
                    foreach (int index2 in PossibleMoves(item1))
                    {
                        MakeMove(item1.index, index2);
                        if (!IsCheck(color))
                        {
                            BackMove();
                            boardPositions.RemoveAt(boardPositions.Count - 1);
                            return false;
                        }
                        BackMove();
                        boardPositions.RemoveAt(boardPositions.Count - 1);
                    }
                }
            }
            return true;
        }

        private List<int> PossibleMoves(Checker checker)
        {
            string color = checker.color;
            string opositeColor = SwitchColor(color);
            int rowOld = checker.index / 8;
            int columnOld = checker.index % 8;
            List<int> possibleMoves = new List<int>();

            if (checker.name == "k")
            {
                try
                {
                    if (board2d[rowOld + 2, columnOld + 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 2, columnOld + 1].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld + 2, columnOld - 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 2, columnOld - 1].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld - 2, columnOld + 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 2, columnOld + 1].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld - 2, columnOld - 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 2, columnOld - 1].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld + 1, columnOld + 2].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 1, columnOld + 2].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld + 1, columnOld - 2].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 1, columnOld - 2].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld - 1, columnOld + 2].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 1, columnOld + 2].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld - 1, columnOld - 2].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 1, columnOld - 2].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
            }

            else if (checker.name == "K")
            {
                if (rowOld - 1 > -1 && columnOld - 1 > -1)
                {
                    if (board2d[rowOld - 1, columnOld - 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 1, columnOld - 1].index);
                    }
                }
                if (rowOld - 1 > -1 && columnOld + 1 < 8)
                {
                    if (board2d[rowOld - 1, columnOld + 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 1, columnOld + 1].index);
                    }
                }
                if (rowOld + 1 < 8 && columnOld - 1 > -1)
                {
                    if (board2d[rowOld + 1, columnOld - 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 1, columnOld - 1].index);
                    }
                }
                if (rowOld + 1 < 8 && columnOld + 1 < 8)
                {
                    if (board2d[rowOld + 1, columnOld + 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 1, columnOld + 1].index);
                    }
                }
                if (rowOld - 1 > -1)
                {
                    if (board2d[rowOld - 1, columnOld].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld - 1, columnOld].index);
                    }
                }
                if (rowOld + 1 < 8)
                {
                    if (board2d[rowOld + 1, columnOld].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld + 1, columnOld].index);
                    }
                }
                if (columnOld - 1 > -1)
                {
                    if (board2d[rowOld, columnOld - 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld, columnOld - 1].index);
                    }
                }
                if (columnOld + 1 < 8)
                {
                    if (board2d[rowOld, columnOld + 1].color != color)
                    {
                        possibleMoves.Add(board2d[rowOld, columnOld + 1].index);
                    }
                }
                if (!checker.moved)
                {
                    if (board1d[checker.index + 3].name == "r" && !board1d[checker.index + 3].moved &&
                            board1d[checker.index + 2].name == null && board1d[checker.index + 1].name == null)
                    {
                        if (IsThretened(checker, opositeColor)==-1 &&
                            IsThretened(board1d[checker.index + 2], opositeColor)==-1 &&
                            IsThretened(board1d[checker.index + 1], opositeColor)==-1)
                        {
                            if (checker.color == "white")
                            {
                                if (!(board2d[6, 4].color == "black" && board2d[6, 4].name == "p" ||
                                        board2d[6, 6].color == "black" && board2d[6, 6].name == "p" ||
                                        board2d[6, 7].color == "black" && board2d[6, 7].name == "p"))
                                {
                                    possibleMoves.Add(board2d[rowOld, columnOld + 2].index);
                                }
                            }
                            else
                            {
                                if (!(board2d[1, 4].color == "white" && board2d[1, 4].name == "p" ||
                                        board2d[1, 6].color == "white" && board2d[1, 6].name == "p" ||
                                        board2d[1, 7].color == "white" && board2d[1, 7].name == "p"))

                                {
                                    possibleMoves.Add(board2d[rowOld, columnOld + 2].index);
                                }
                            }
                        }
                    }
                    if (board1d[checker.index - 4].name == "r" && !board1d[checker.index - 4].moved &&
                            board1d[checker.index - 3].name == null && board1d[checker.index - 2].name == null &&
                            board1d[checker.index - 1].name == null)
                    {
                        if (IsThretened(checker, opositeColor)==-1 &&
                            IsThretened(board1d[checker.index - 2], opositeColor)==-1 &&
                            IsThretened(board1d[checker.index - 1], opositeColor)==-1)
                        {
                            if (checker.color == "white")
                            {
                                if (!(board2d[6, 1].color == "black" && board2d[6, 1].name == "p" ||
                                        board2d[6, 2].color == "black" && board2d[6, 2].name == "p" ||
                                        board2d[6, 4].color == "black" && board2d[6, 4].name == "p"))
                                {
                                    possibleMoves.Add(board2d[rowOld, columnOld - 2].index);
                                }
                            }
                            else
                            {
                                if (!(board2d[1, 1].color == "white" && board2d[1, 1].name == "p" ||
                                        board2d[1, 2].color == "white" && board2d[1, 2].name == "p" ||
                                        board2d[1, 4].color == "white" && board2d[1, 4].name == "p"))
                                {
                                    possibleMoves.Add(board2d[rowOld, columnOld - 2].index);
                                }
                            }
                        }
                    }
                }
            }
            else if (checker.name == "p")
            {
                int num = -1;
                if (checker.color == "black") { num = 1; }
                if (((checker.color == "white" && checker.index > 47 && checker.index < 56) ||
                        (checker.color == "black" && checker.index > 7 && checker.index < 16)) &&
                        board2d[rowOld + num * 2, columnOld].color == null &&
                        board2d[rowOld + num, columnOld].color == null)
                {
                    possibleMoves.Add(board2d[rowOld + num * 2, columnOld].index);
                }
                try
                {
                    if (board2d[rowOld + num, columnOld].color == null)
                    {
                        possibleMoves.Add(board2d[rowOld + num, columnOld].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld + num, columnOld + 1].color == opositeColor ||
                            (board2d[rowOld + num, columnOld + 1].enPassant && checker.color == "white") ||
                            (board2d[rowOld + num, columnOld + 1].enPassant && checker.color == "black"))
                    {
                        possibleMoves.Add(board2d[rowOld + num, columnOld + 1].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
                try
                {
                    if (board2d[rowOld + num, columnOld - 1].color == opositeColor ||
                            (board2d[rowOld + num, columnOld - 1].enPassant && checker.color == "white") ||
                            (board2d[rowOld + num, columnOld - 1].enPassant && checker.color == "black"))
                    {
                        possibleMoves.Add(board2d[rowOld + num, columnOld - 1].index);
                    }
                }
                catch (IndexOutOfRangeException e) { }
            }
            else if (checker.name == "r")
            {
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld + n < 8)
                    {
                        if (board2d[rowOld + n, columnOld].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld].index);
                        }
                        else if (board2d[rowOld + n, columnOld].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld - n > -1)
                    {
                        if (board2d[rowOld - n, columnOld].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld].index);
                        }
                        else if (board2d[rowOld - n, columnOld].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (columnOld + n < 8)
                    {
                        if (board2d[rowOld, columnOld + n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld + n].index);
                        }
                        else if (board2d[rowOld, columnOld + n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld + n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (columnOld - n > -1)
                    {
                        if (board2d[rowOld, columnOld - n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld - n].index);
                        }
                        else if (board2d[rowOld, columnOld - n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld - n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
            }
            else if (checker.name == "b")
            {
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld + n < 8 && columnOld + n < 8)
                    {
                        if (board2d[rowOld + n, columnOld + n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld + n].index);
                        }
                        else if (board2d[rowOld + n, columnOld + n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld + n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld - n > -1 && columnOld + n < 8)
                    {
                        if (board2d[rowOld - n, columnOld + n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld + n].index);
                        }
                        else if (board2d[rowOld - n, columnOld + n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld + n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld + n < 8 && columnOld - n > -1)
                    {
                        if (board2d[rowOld + n, columnOld - n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld - n].index);
                        }
                        else if (board2d[rowOld + n, columnOld - n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld - n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld - n > -1 && columnOld - n > -1)
                    {
                        if (board2d[rowOld - n, columnOld - n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld - n].index);
                        }
                        else if (board2d[rowOld - n, columnOld - n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld - n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
            }

            else if (checker.name == "q")
            {
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld + n < 8)
                    {
                        if (board2d[rowOld + n, columnOld].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld].index);
                        }
                        else if (board2d[rowOld + n, columnOld].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld - n > -1)
                    {
                        if (board2d[rowOld - n, columnOld].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld].index);
                        }
                        else if (board2d[rowOld - n, columnOld].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (columnOld + n < 8)
                    {
                        if (board2d[rowOld, columnOld + n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld + n].index);
                        }
                        else if (board2d[rowOld, columnOld + n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld + n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (columnOld - n > -1)
                    {
                        if (board2d[rowOld, columnOld - n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld - n].index);
                        }
                        else if (board2d[rowOld, columnOld - n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld, columnOld - n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld + n < 8 && columnOld + n < 8)
                    {
                        if (board2d[rowOld + n, columnOld + n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld + n].index);
                        }
                        else if (board2d[rowOld + n, columnOld + n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld + n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld - n > -1 && columnOld + n < 8)
                    {
                        if (board2d[rowOld - n, columnOld + n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld + n].index);
                        }
                        else if (board2d[rowOld - n, columnOld + n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld + n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld + n < 8 && columnOld - n > -1)
                    {
                        if (board2d[rowOld + n, columnOld - n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld - n].index);
                        }
                        else if (board2d[rowOld + n, columnOld - n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld + n, columnOld - n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
                for (int n = 1; n < 8; n++)
                {
                    if (rowOld - n > -1 && columnOld - n > -1)
                    {
                        if (board2d[rowOld - n, columnOld - n].color == null)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld - n].index);
                        }
                        else if (board2d[rowOld - n, columnOld - n].color == opositeColor)
                        {
                            possibleMoves.Add(board2d[rowOld - n, columnOld - n].index);
                            break;
                        }
                        else { break; }
                    }
                    else { break; }
                }
            }
            return possibleMoves;
        }


        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            Checker_Click(btn);
        }


        private void buttonNew_Click(object sender, EventArgs e)
        {
            NewBoard();
        }

        private bool isValidMove(Checker oldChecker, Checker newChecker)
        {
            int diff = newChecker.index - oldChecker.index;
            int rowOld = oldChecker.index / 8;
            int columnOld = oldChecker.index % 8;
            int rowNew = newChecker.index / 8;
            int columnNew = newChecker.index % 8;
            int rowDiff = rowNew - rowOld;
            int columnDiff = columnNew - columnOld;
            int num = 2;
            string opositTurn = SwitchColor(oldChecker.color);
            if (oldChecker.color == "white") { num = -1; } else { num = 1; }


            bool checkWay(int distance)
            {
                for (int n = 1; n < distance; n++)
                {
                    if (rowDiff > 0 && columnDiff > 0)
                    {
                        if (board2d[rowOld + (rowDiff - n), columnOld + (columnDiff - n)].color != null)
                        {
                            return false;
                        }
                    }
                    else if (rowDiff < 0 && columnDiff < 0)
                    {
                        if (board2d[rowOld + (rowDiff + n), columnOld + (columnDiff + n)].color != null)
                        {
                            return false;
                        }
                    }
                    else if (rowDiff < 0 && columnDiff > 0)
                    {
                        if (board2d[rowOld + (rowDiff + n), columnOld + (columnDiff - n)].color != null)
                        {
                            return false;
                        }
                    }
                    else if (rowDiff > 0 && columnDiff < 0)
                    {
                        if (board2d[rowOld + (rowDiff - n), columnOld + (columnDiff + n)].color != null)
                        {
                            return false;
                        }
                    }
                    else if (rowDiff > 0)
                    {
                        if (board2d[rowOld + (rowDiff - n), columnOld].color != null)
                        {
                            return false;
                        }
                    }
                    else if (rowDiff < 0)
                    {
                        if (board2d[rowOld + (rowDiff + n), columnOld].color != null)
                        {
                            return false;
                        }
                    }
                    else if (columnDiff > 0)
                    {
                        if (board2d[rowOld, columnOld + (columnDiff - n)].color != null)
                        {
                            return false;
                        }
                    }
                    else if (columnDiff < 0)
                    {
                        if (board2d[rowOld, columnOld + (columnDiff + n)].color != null)
                        {
                            return false;
                        }
                    }
                }
                // return true if way is clear
                return true;
            }

            //checks knight move
            if (oldChecker.name == "k")
            {
                if ((Math.Abs(rowDiff) == 2 && Math.Abs(columnDiff) == 1) ||
                    (Math.Abs(rowDiff) == 1 && Math.Abs(columnDiff) == 2))
                {
                    return true;
                }
            }

            //checks pawn move
            else if (oldChecker.name == "p")
            {
                if ((newChecker.color == null && columnDiff == 0 && rowDiff == 1 * num) ||
                    (newChecker.color == opositTurn && Math.Abs(columnDiff) == 1 && rowDiff == num * 1))
                {
                    return true;
                }
                else if (((rowOld == 4 && rowNew == 5 && newChecker.enPassant && oldChecker.color == "black") ||
                            (rowOld == 3 && rowNew == 2 && newChecker.enPassant && oldChecker.color == "white")) &&
                            Math.Abs(columnDiff) == 1)
                {
                    return true;
                }
                else if (((newChecker.color == null && Math.Abs(rowDiff) == 2 && columnDiff == 0) &&
                    ((oldChecker.index > 47 && oldChecker.index < 56) && oldChecker.color == "white" &&
                    board2d[5, columnOld].color == null || (oldChecker.index > 7 && oldChecker.index < 16) &&
                    oldChecker.color == "black" && board2d[2, columnOld].color == null)))
                {
                    return true;
                }
            }

            //checks king move
            else if (oldChecker.name == "K")
            {

                if (((Math.Abs(rowDiff) == 1) && (Math.Abs(columnDiff) == 1)) || ((Math.Abs(rowDiff) == 1) &&
                            (columnDiff == 0)) || ((rowDiff == 0) && (Math.Abs(columnDiff) == 1)))
                {
                    return true;
                }

                //checks kingside castling validity
                else if (!oldChecker.moved)
                {
                    if (diff == 2 &&
                        board1d[oldChecker.index + 3].name == "r" && (!board1d[oldChecker.index + 3].moved) &&
                        board1d[oldChecker.index + 2].name == null && board1d[oldChecker.index + 1].name == null)
                    {
                        if (IsThretened(oldChecker, opositTurn)==-1 &&
                            IsThretened(board1d[oldChecker.index + 2], opositTurn)==-1 &&
                            IsThretened(board1d[oldChecker.index + 1], opositTurn)==-1)
                        {
                            if (oldChecker.color == "white")
                            {
                                if (board2d[6, 4].color == "black" && board2d[6, 4].name == "p" ||
                                    board2d[6, 6].color == "black" && board2d[6, 6].name == "p" ||
                                    board2d[6, 7].color == "black" && board2d[6, 7].name == "p")
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (board2d[1, 4].color == "white" && board2d[1, 4].name == "p" ||
                                    board2d[1, 6].color == "white" && board2d[1, 6].name == "p" ||
                                    board2d[1, 7].color == "white" && board2d[1, 7].name == "p")
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }

                    //checks queenside castling validity
                    else if (diff == -2 && board1d[oldChecker.index - 4].name == "r" &&
                        (!board1d[oldChecker.index - 4].moved) && board1d[oldChecker.index - 3].name == null &&
                        board1d[oldChecker.index - 2].name == null && board1d[oldChecker.index - 1].name == null)
                    {
                        if (IsThretened(oldChecker, opositTurn)==-1 &&
                            IsThretened(board1d[oldChecker.index - 2], opositTurn)==-1 &&
                            IsThretened(board1d[oldChecker.index - 1], opositTurn)==-1)
                        {
                            if (oldChecker.color == "white")
                            {
                                if (board2d[6, 1].color == "black" && board2d[6, 1].name == "p" ||
                                    board2d[6, 2].color == "black" && board2d[6, 2].name == "p" ||
                                    board2d[6, 4].color == "black" && board2d[6, 4].name == "p")
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (board2d[1, 1].color == "white" && board2d[1, 1].name == "p" ||
                                    board2d[1, 2].color == "white" && board2d[1, 2].name == "p" ||
                                    board2d[1, 4].color == "white" && board2d[1, 4].name == "p")
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                }
            }

            //checks rock move
            else if (oldChecker.name == "r")
            {
                for (int i = 1; i < 8; i++)
                {
                    if (((Math.Abs(rowDiff) == i) && (columnDiff == 0)) || ((rowDiff == 0) && (Math.Abs(columnDiff) == i)))
                    {
                        if (i > 1)
                        {
                            if (checkWay(i))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            //checks bishop move
            else if (oldChecker.name == "b")
            {
                for (int i = 1; i < 8; i++)
                {
                    if (((Math.Abs(rowDiff) == i) && (Math.Abs(columnDiff) == i)))
                    {
                        if (i > 1)
                        {
                            return checkWay(i);
                        }
                        else { return true; }
                    }
                }
            }

            //checks queen move
            else if (oldChecker.name == "q")
            {
                for (int i = 1; i < 8; i++)
                {
                    if (((Math.Abs(rowDiff) == i) && (Math.Abs(columnDiff) == i)) || 
                        ((Math.Abs(rowDiff) == i) && (columnDiff == 0)) || ((rowDiff == 0) && 
                        (Math.Abs(columnDiff) == i)))
                    {
                        if (i > 1)
                        {
                            return checkWay(i);
                        }
                        else { return true; }
                    }
                }
            }
            return false;
        }


        private int boardScore(){
            int score = 0;
            foreach (Checker item in board1d)
            {
                if (item.color != null)
                {
                    if (item.color == "black")
                    {
                        switch (item.name)
                        {
                            case "p": score += 100; break;
                            case "k": score += 350; break;
                            case "b": score += 350; break;
                            case "r": score += 525; break;
                            case "q": score += 1000; break;
                            case "K": score += 10000; break;
                            default: break;
                        }
                    }
                    else
                    {
                        switch (item.name)
                        {
                            case "p": score -= 100; break;
                            case "k": score -= 350; break;
                            case "b": score -= 350; break;
                            case "r": score -= 525; break;
                            case "q": score -= 1000; break;
                            case "K": score -= 10000; break;
                            default: break;
                        }
                    }
                }
            }
            return score;
        }

        private int moveScore(string color,int score,int depth)
        {
            int min = 99999;
            int max = -99999;
            int[] piecesArry;
            if (color=="black")
            {
                piecesArry = blackPieces.ToArray();
            }
            else
            {
                piecesArry = whitePieces.ToArray();
            }


            foreach (int index in piecesArry)
            {
                foreach (int index2 in PossibleMoves(board1d[index]))
                {
                    MakeMove(index, index2);
                    if (IsCheck(color))
                    {
                        BackMove();
                        boardPositions.RemoveAt(boardPositions.Count - 1);
                        continue;
                    }
                    if (depth > 1)
                    {
                        if (color == "white")
                        {
                            min = moveScore("black", min, depth - 1);
                        }
                        else
                        {
                            max = moveScore("white", max, depth - 1);
                        }
                    }
                    else
                    {
                        if (color == "white")
                        {
                            if (min > boardScore()) { min = boardScore(); }
                        }
                        else
                        {
                            if (max < boardScore()) { max = boardScore(); }
                        }
                    }

                    BackMove();
                    boardPositions.RemoveAt(boardPositions.Count - 1);

                    if (color == "white")
                    {
                        if (min <= score) { return score; }
                    }
                    else
                    {
                        if (max >= score) { return score; }
                    }
                }
            }
            if (min == 99999) {
                if (!IsCheck("white")) { min = 0; } 
            }
            if (max == -99999) {
                if (!IsCheck("black")) { max = 0; } 
            }
            if (color=="white")
            { 
                return min; 
            }else
            { 
                return max; 
            }
        }


        private BestMove CalculateBestMove(){
            int minMax = 99999;
            int maxMin = -999999;
            BestMove bestMove = new BestMove();
            int[] blackPiecesArr = blackPieces.OrderBy(x => Guid.NewGuid()).ToList().ToArray(); //randomize the black pieces list into arry

            foreach (int index in blackPiecesArr)
            {
                List<int> list = PossibleMoves(board1d[index]);
                list.Shuffle();
                foreach (int index2 in list)
                {
                    MakeMove(index, index2);

                    if (IsCheck("black"))
                    {
                        BackMove();
                        boardPositions.RemoveAt(boardPositions.Count - 1);
                        continue;
                    }

                    minMax = moveScore("white", maxMin, compLvl);

                    if (maxMin < minMax)
                    {
                        maxMin = minMax;
                        bestMove.firstIndex = index;
                        bestMove.lastIndex = index2;
                    }

                    BackMove();
                    boardPositions.RemoveAt(boardPositions.Count - 1);
                }
            }
            return bestMove;
        }

        private async void ComputerTurn()
        {
            BestMove cordinate = new BestMove();            
            cordinate = CalculateBestMove();
            buttons1d[cordinate.firstIndex].BackgroundImage = redBackgound;
            buttons1d[cordinate.lastIndex].BackgroundImage = redBackgound;
            MakeMove(cordinate.firstIndex, cordinate.lastIndex);
            await Task.Delay(700);
            DrawBoard(board1d);

            turn = "white";
            buttonNew.Enabled = true;
            buttonBack.Enabled = true;
            checkBoxVS.Enabled = true;
            radioLevel1.Enabled = true;
            radioLevel2.Enabled = true;
            radioLevel3.Enabled = true;
            label1.Text = "White turn";
            if (IsCheck("white"))
            {
                if (IsCheckMate("white"))
                {
                    if (sound)
                    {
                        soundFail.Play();
                    }
                    label1.Text = "You Loose!!!";
                    foreach (Button button in buttons1d)
                    {
                        button.Enabled = false;
                    }
                }
                else
                {
                    if (sound)
                    {
                        soundCheck.Play();
                    }
                    label1.Text = "Check!";
                }
            }
            else
            {
                if (sound)
                {
                    soundClick.Play();
                }
            }
        }

        private void CheckBoxVS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxVS.Checked) { vsComputer = true; } else { vsComputer = false; }
            if (vsComputer && turn=="black") {
                buttonPlay.Enabled = true;
                label1.Text = "Press > for computer to move";
            }else if (!vsComputer && turn == "black")
            {
                buttonPlay.Enabled = false;
                label1.Text = "black turn";
            }
        }

        private void RadioLevel1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLevel1.Checked) { compLvl = 2; }
        }

        private void RadioLevel2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLevel2.Checked) { compLvl = 3; }
        }

        private void RadioLevel3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLevel3.Checked) { compLvl = 4; }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            foreach (Button button in buttons1d)
            {
                button.Enabled = true;
            }
            if (currentBoardIndex > 0)
            {
                label1.Text = SwitchColor(turn) + " turn";
                buttonForward.Enabled = true;
                BackMove();
                DrawBoard(board1d);
                turn = SwitchColor(turn);
                handPiece = null;
                oldChecker = empty;
            }
            if (vsComputer && turn == "black")
            {
                label1.Text = "press > for computer";
                buttonPlay.Enabled = true;
            }
            else
            {
                buttonPlay.Enabled = false;
            }
        }
        private void ButtonForward_Click(object sender, EventArgs e)
        {
            currentBoardIndex++;
            GetBoard(currentBoardIndex);
            DrawBoard(board1d);
            label1.Text = SwitchColor(turn) + " turn";
            turn = SwitchColor(turn);
            buttonBack.Enabled = true;
            if (currentBoardIndex >= boardPositions.Count - 1)
            {
                buttonForward.Enabled = false;
            }
            if (vsComputer && turn == "black") { 
                buttonPlay.Enabled = true;
                label1.Text = "press > for computer";
            }
            else
            {
                buttonPlay.Enabled = false;
            }
        }
        private async void ButtonPlay_Click(object sender, EventArgs e)
        {
            if (currentBoardIndex+1 < boardPositions.Count)
            {
                boardPositions.RemoveRange(currentBoardIndex+1, (boardPositions.Count - 1) - (currentBoardIndex));
            }
            label1.Text = "Computer thinking..";
            buttonPlay.Enabled = false;
            buttonNew.Enabled = false;
            buttonForward.Enabled = false;
            buttonBack.Enabled = false;
            checkBoxVS.Enabled = false;
            radioLevel1.Enabled = false;
            radioLevel2.Enabled = false;
            radioLevel3.Enabled = false;
            await Task.Delay(50);
            ComputerTurn();
        }

        private void CheckSound_CheckedChanged(object sender, EventArgs e)
        {
            if (checkSound.Checked) { sound = true; }else { sound = false; }
        }
    }
}
    
