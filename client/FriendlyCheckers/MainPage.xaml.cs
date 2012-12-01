﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;

namespace FriendlyCheckers
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static Color DarkRed;
        public static Color DarkGrey;
        public static Color HighlightRed;
        public static Color HighlightGrey;
        private static Color Brown;
        private static Color Sand;

        private static int checkerX, checkerY;
        public static int w = 400, h = 400;
        private static Canvas mainCanvas;
        private static Boolean FORCE_JUMP = false, ROTATE = false, DIFFICULT = false;
        private static GameLogic logic;
        private static DataHandler dataDude;
        private static BoardSpace[,] spaces;
        private static Checker[,] pieces;
        private Player computerPlayer;

        private static Boolean rotated = false;
        private static Boolean wait_for_timer = false, wait_for_computer = false;
        private static int row_W = 8;
        private static DispatcherTimer TURN_TIMER;
        public enum GameType { OUT_OF_GAME, OPTIONS, ABOUT, SINGLE_PLAYER, ONLINE_MULTI, LOCAL_MULTI };
        public static GameType game_type = GameType.OUT_OF_GAME;

        public MainPage()
        {
            InitializeComponent();
            LayoutRoot.Children.Remove(OptionsPanel);
            LayoutRoot.Children.Remove(AboutPanel);
            checkerX = checkerY = -1;
            computerPlayer = new Player("Computer", PieceColor.RED);
            dataDude = new DataHandler();

            Color shade = new Color();
            shade.R = shade.G = shade.B = 0;
            shade.A = 150;
            Shader.Fill = new SolidColorBrush(shade);

            TURN_TIMER = new DispatcherTimer();
            TURN_TIMER.Tick += timerTick;              // Everytime timer ticks, timer_Tick will be called
            TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 800);  // Timer will tick in 800 milliseconds. This is the wait between moves.
            Op_DiffEasy.IsChecked = true;

            InitializeColors();
            RemoveInGameStats();
            mainCanvas = new Canvas();
            mainCanvas.Width = w;
            mainCanvas.Height = h;
            ContentPanel.Children.Add(mainCanvas);
            createBoard();
            createPieces();
            rotateBoard90();
        }
        private void InitializeColors()
        {
            HighlightRed = new Color();
            HighlightRed.R = 255;
            HighlightRed.G = 100;
            HighlightRed.B = 100;
            HighlightRed.A = 255;

            HighlightGrey = new Color();
            HighlightGrey.R = 100;
            HighlightGrey.G = 100;
            HighlightGrey.B = 100;
            HighlightGrey.A = 255;

            DarkRed = new Color();
            DarkRed.R = 50;
            DarkRed.G = 0;
            DarkRed.B = 0;
            DarkRed.A = 255;

            DarkGrey = new Color();
            DarkGrey.R = DarkGrey.G = DarkGrey.B = 20;
            DarkGrey.A = 255;

            Brown = new Color();
            Brown.R = 120;
            Brown.G = 40;
            Brown.B = 10;
            Brown.A = 255;

            Sand = new Color();
            Sand.R = 200;
            Sand.G = 180;
            Sand.B = 90;
            Sand.A = 255;
        }
        private void createPieces()
        {
            logic = new GameLogic(row_W, row_W);
            pieces = new Checker[8,8];
            int row = 0, col = 0;
            for (int k = 0; k < 24; k++)
            {
                row = (k / 4) + ((k>=12)? 2 : 0);
                col = 2*(k % 4) + (row%2==0?0:1);
                pieces[col,row] = new Checker(col, row, (k < 12) ? Colors.Red : DarkGrey,
                                                (k < 12) ? DarkRed : Colors.Black);
                Vector vect = new Vector(row, col);
                Piece piece = new Piece((k < 12) ? PieceColor.RED: PieceColor.BLACK,vect,PieceType.REGULAR);
                logic.addPiece(piece);
                mainCanvas.Children.Add(pieces[col, row].getEl2());
                mainCanvas.Children.Add(pieces[col, row].getEl1());
                mainCanvas.Children.Add(pieces[col, row].getCrown());
            }
        }
        private void createBoard()
        {
            spaces = new BoardSpace[row_W, row_W];
            int size = 55;
            for (int k = 0; k < row_W; k++)
            {
                for (int i = 0; i < row_W; i++)
                {
                    spaces[k, i] = new BoardSpace(k, i, size, ((i + k) % 2 == 0) ? Sand : Brown);
                    mainCanvas.Children.Add(spaces[k, i].getRect());
                }
            }
        }
        private void ClearMenu()
        {
            ContentPanel.Children.Remove(singleplayer);
            ContentPanel.Children.Remove(multiplayer_local);
            ContentPanel.Children.Remove(multiplayer_online);
            ContentPanel.Children.Remove(options);
            ContentPanel.Children.Remove(about);
            LayoutRoot.Children.Remove(TitlePanel);
        }
        private void RemoveInGameStats()
        {
            HiddenPanel.Children.Remove(Moves);
            HiddenPanel.Children.Remove(Versus);
            ContentPanel.Children.Remove(quit);
            ContentPanel.Children.Remove(WhoseTurn);
            ContentPanel.Children.Remove(Shader);
            ContentPanel.Children.Remove(Search);
        }
        private void AddInGameStats()
        {
            HiddenPanel.Children.Add(Versus);
            HiddenPanel.Children.Add(Moves);
            ContentPanel.Children.Add(quit);
            ContentPanel.Children.Add(WhoseTurn);
        }
        private void SinglePlayer_Setup(object sender, RoutedEventArgs e)
        {
            game_type = GameType.SINGLE_PLAYER;
            TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0); 
            ClearMenu();
            Versus.Text = "Player 1 vs. Computer";
            AddInGameStats();
            resetBoard();
        }
        private void Local_Multi_Setup(object sender, RoutedEventArgs e)
        {
            game_type = GameType.LOCAL_MULTI;
            if(ROTATE)
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 800); 
            else
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0); 
            ClearMenu();
            Versus.Text = "Player 1 vs. Player 2";
            AddInGameStats();
            resetBoard();
        }
        private void Online_Multi_Setup(object sender, RoutedEventArgs e)
        {
            game_type = GameType.ONLINE_MULTI;
            TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0);
            ClearMenu(); 
            Versus.Text = "Player 1 vs. [Searching...]";
            AddInGameStats();
            resetBoard();
            ContentPanel.Children.Add(Shader);
            ContentPanel.Children.Add(Search);
        }
        private void Menu_Setup(object sender, RoutedEventArgs e)
        {
            if (game_type != GameType.OPTIONS && game_type != GameType.ABOUT
                    && MessageBox.Show("The current game will end.", "Exit to main menu?", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)return; 
            RemoveInGameStats();
            if (game_type == GameType.OPTIONS || game_type == GameType.ABOUT)
            {
                ContentPanel.Children.Add(mainCanvas);
                LayoutRoot.Children.Remove(OptionsPanel);
                LayoutRoot.Children.Remove(AboutPanel);
            }
            else
                LayoutRoot.Children.Add(TitlePanel);
            PageTitle.Text = "Checkers";
            ContentPanel.Children.Add(singleplayer);
            ContentPanel.Children.Add(multiplayer_local);
            ContentPanel.Children.Add(multiplayer_online);
            ContentPanel.Children.Add(options);
            ContentPanel.Children.Add(about);
            game_type = GameType.OUT_OF_GAME;
            wait_for_computer = false;
            wait_for_timer = false;
            rotated = false;
            checkerX = checkerY = -1;
            resetBoard();
            rotateBoard90();
        }
        private void Show_Options(object sender, RoutedEventArgs e)
        {
            game_type = GameType.OPTIONS;
            PageTitle.Text = "Options";
            ClearMenu();
            LayoutRoot.Children.Add(TitlePanel);
            LayoutRoot.Children.Add(OptionsPanel);
            ContentPanel.Children.Remove(mainCanvas);
            ContentPanel.Children.Add(quit);
        }
        private void Show_About(object sender, RoutedEventArgs e)
        {
            game_type = GameType.ABOUT;
            PageTitle.Text = "About";
            ClearMenu();
            LayoutRoot.Children.Add(TitlePanel);
            LayoutRoot.Children.Add(AboutPanel);
            ContentPanel.Children.Remove(mainCanvas);
            ContentPanel.Children.Add(quit);
        }
        private void resetBoard()
        {
            for (int k = 0; k < row_W; k++)
            {
                for (int j = 0; j < row_W; j++)
                {
                    if (pieces[k, j] == null) continue;
                    delete(k, j);
                }
            }
            createPieces();
            Moves.Text = "Moves: 0";
            WhoseTurn.Text = "Black to move next.";
        }
        private static void rotateBoard180()
        {
            for (int k = 0; k < row_W; k++)
            {
                for (int i = 0; i < row_W; i++)
                {
                    if (pieces[i, k] == null) continue;
                    Checker c = pieces[i, k];
                    if (!rotated)
                        c.rotate(row_W - c.getY() - 1, row_W - c.getX() - 1);
                    else
                        c.rotate(c.getY(), c.getX());
                }
            }
            rotated = !rotated;
        }
        private static void rotateBoard90()
        {
            for (int k = 0; k < row_W; k++)
            {
                for (int i = 0; i < row_W; i++)
                {
                    if (pieces[i, k] == null) continue;
                    Checker c = pieces[i, k];
                    if (!rotated)
                        c.rotate(c.getX(), c.getY());
                    else
                        c.rotate(c.getY(), c.getX());
                }
            }
        }
        public GameType getGameType()
        {
            return game_type;
        }
        
        
        //////////
        //// HANDLERS FOR BOARD, PIECES, LOGIC AND HIGHLIGHTING LOCATED BELOW HERE
        //////////
        public static void MakeMove(int boardX, int boardY)
        {
            if (wait_for_timer || wait_for_computer) return;
            if (game_type == GameType.OUT_OF_GAME || (checkerX == -1 && checkerY == -1)) return;
            Move m;
            try
            {
                int locX = checkerX;
                int locY = checkerY;
                // Unhighlight the selected piece
                handleHighlighting(checkerX, checkerY);
                m = logic.makeMove(locY, locX, (!rotated ? boardY : (row_W - boardY - 1)), (!rotated ? boardX : (row_W - boardX - 1)));
                handleMove(m);

                TURN_TIMER.Start();
                wait_for_timer = true;
            }
            catch (PieceWrongColorException) { MessageBox.Show("You cannot move your opponent's pieces!"); }
            catch (InvalidMoveException) { }
            checkerX = checkerY = -1;
        }
        private void timerTick(object o, EventArgs sender)
        {
            TURN_TIMER.Stop();
            WhoseTurn.Text = (logic.whoseMove().Equals(PieceColor.RED) ? "Red" : "Black") + " to move next.";
            Moves.Text = "Moves: "+logic.getMoveNumber();
            wait_for_timer = false;
            if (ROTATE && game_type == GameType.LOCAL_MULTI)
                rotateBoard180();
            else if (game_type == GameType.SINGLE_PLAYER)
            {
                wait_for_computer = !wait_for_computer;
                if (wait_for_computer)
                {
                    //Move m
                    //if(DIFFICULT)
                    //  m = computerPlayer.getHardMove(logic); 
                    //else
                    //  m = computerPlayer.getEasyMove(logic);
                   // handleMove(m);
                   // TURN_TIMER.Start();
                   // wait_for_timer = true;
                }
            }
        }
        private static void handleMove(Move move)
        {
            List<Piece> added = move.getAdditions();
            List<Piece> removed = move.getRemovals();

            foreach (Piece p in removed)
            {
                Vector co = p.getCoordinates();
                delete(co.getX(),co.getY());
            }

            foreach (Piece p in added)
            {
                Vector co = p.getCoordinates();

                int col = co.getX();
                int row = co.getY();
                Checker c = new Checker(col, row,p.getColor() == PieceColor.BLACK ? DarkGrey : Colors.Red,
                                                 p.getColor() == PieceColor.BLACK ? Colors.Black : DarkRed);
                pieces[col, row] = c;
                mainCanvas.Children.Add(pieces[col, row].getEl2());
                mainCanvas.Children.Add(pieces[col, row].getEl1());
                mainCanvas.Children.Add(pieces[col, row].getCrown());

                if (p.getType() == PieceType.KING)
                    c.king();
                if(rotated)
                    c.rotate(row_W - c.getY() - 1, row_W - c.getX() - 1);
            }
        }
        private static Checker delete(int x, int y)
        {
            Checker temp = pieces[x, y];
            mainCanvas.Children.Remove(temp.getEl2());
            mainCanvas.Children.Remove(temp.getEl1());
            mainCanvas.Children.Remove(temp.getCrown());
            pieces[x, y] = null;

            return temp;
        }
        public static void handleHighlighting(int x, int y)
        {
            if (wait_for_timer || wait_for_computer) return;
            if (!logic.isSelectable(y,x))return;

            Checker HIGHLIGHTED_PIECE = pieces[x, y];
            if (checkerX != -1 && checkerY != -1)
                pieces[checkerX, checkerY].toggleHighlight();

            //if the already highlighted piece is the same as the one being clicked
            if (checkerX!=-1 && checkerY!=-1 && HIGHLIGHTED_PIECE.Equals(pieces[checkerX,checkerY])) 
            {
                checkerX = checkerY = -1;
                return;
            }
            else //otherwise, a piece is either being clicked for the first time or is switching highlights.
            {
                checkerX = x;
                checkerY = y;
                HIGHLIGHTED_PIECE.toggleHighlight();
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FORCE_JUMP = (Op_ForceJump.IsChecked == true);
            ROTATE = (Op_Rotate.IsChecked == true);
            if(sender.Equals(Op_DiffHard))
                DIFFICULT = (Op_DiffHard.IsChecked == true);
            else
                DIFFICULT = (Op_DiffEasy.IsChecked == false);
            Op_DiffHard.IsChecked = DIFFICULT;
            Op_DiffEasy.IsChecked = !DIFFICULT;

            if (!ROTATE || game_type != GameType.LOCAL_MULTI)
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0); 
            else
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 800); 
        }
    }
    public class BoardSpace
    {
        private Rectangle space;
        private int gridx, gridy, size;
        private Color color;

        public BoardSpace(int x, int y, int size, Color c)
        {
            this.gridx = x;
            this.gridy = y;
            this.color = c;
            this.size = size;

            space = new Rectangle();
            space.Width = size;
            space.Height = size;
            space.MinWidth = size;
            space.MinHeight = size;
            space.MouseLeftButtonUp += Space_Action;
            space.Fill = new SolidColorBrush(color);
            int lm = (x * size) - 20;
            int tm = (y * size) - 115;
            space.Margin = new Thickness(lm, tm, MainPage.w - lm, MainPage.h - tm);
        }
        public int getX(){return this.gridx;}
        public int getY(){return this.gridy;}
        public Rectangle getRect() { return space; }
        public Color getColor() { return color; }
        public void setColor(Color c)
        {
            this.color = c;
            space.Fill = new SolidColorBrush(c);
        }
        private void Space_Action(object o, MouseButtonEventArgs e)
        {
            MainPage.MakeMove(gridx,gridy);
        }
    }
    public class Checker
    {
        private Ellipse el1, el2;
        private Image crown;
        private int offsetx = -111, offsety = 19, marginx = 55, marginy = 55, x, y;
        private Color color, bg, high;
        private bool col, lit;
        public Checker(int x, int y, Color color, Color bg)
        {
            el1 = new Ellipse();
            el2 = new Ellipse();
            crown = new Image();
            BitmapImage bi = new BitmapImage();
            bi.UriSource = new Uri("crown.png", UriKind.Relative);
            crown.Source = bi;
            crown.Visibility = Visibility.Collapsed;
            crown.MouseLeftButtonUp += ellipse_MouseUp;

            this.x = x;
            this.y = y;
            this.lit = false;
            this.color = color;
            this.col = (color.Equals(MainPage.DarkGrey) ? false : true);
            this.bg = bg;
            this.high = !col ? MainPage.HighlightGrey : MainPage.HighlightRed;

            el1.Width = 50;
            el1.MinWidth = 50;
            el1.Height = 50;
            el1.MinHeight = 50;
            el1.Fill = new SolidColorBrush(color);
            el1.Margin = new Thickness(x * marginx - offsety, y * marginy - 2 + offsetx,
                400 - x * marginx + offsety, 400 - y * marginy + 2 - offsetx);
            crown.Margin = new Thickness(x * marginx - offsety - 2, y * marginy - 4 + offsetx,
                400 - x * marginx + offsety+2, 400 - y * marginy + 4 - offsetx);

            el2.Width = 50;
            el2.MinWidth = 50;
            el2.Height = 50;
            el2.MinHeight = 50;
            el2.Margin = new Thickness(x * marginx - offsety + 2, y * marginy + offsetx,
                400 - x * marginx + offsety - 2, 400 - y * marginy - offsetx);
            el2.Fill = new SolidColorBrush(bg);
            el1.MouseLeftButtonUp += ellipse_MouseUp;
            el2.MouseLeftButtonUp += ellipse_MouseUp;
        }
        public Ellipse getEl1() { return el1; }
        public Ellipse getEl2() { return el2; }
        public Image getCrown() { return crown; }
        public int getX() { return this.x; }
        public int getY() { return this.y; }
        public Color getColor() { return this.color; }
        public Color getBG() { return this.bg; }
        public void king()
        {
            crown.Visibility = Visibility.Visible;
        }
        public Boolean Equals(Checker c) { return (c.getX() == this.getX() && c.getY() == this.getY()); }
        public void rotate(int y, int x)
        {
            crown.Margin = new Thickness(x * marginx - offsety - 2, y * marginy - 4 + offsetx,
               400 - x * marginx + offsety + 2, 400 - y * marginy + 4 - offsetx);
            el1.Margin = new Thickness(x * marginx - offsety, y * marginy - 2 + offsetx,
                400 - x * marginx + offsety, 400 - y * marginy + 2 - offsetx);
            el2.Margin = new Thickness(x * marginx - offsety + 2, y * marginy + offsetx,
                400 - x * marginx + offsety - 2, 400 - y * marginy - offsetx);
        }
        public void toggleHighlight()
        {
            if (!lit)
                el1.Fill = new SolidColorBrush(high);
            else
                el1.Fill = new SolidColorBrush(color);
            lit = !lit;
        }
        private void ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MainPage.game_type == MainPage.GameType.OUT_OF_GAME) return;
            MainPage.handleHighlighting(x,y);
        }
    }
}