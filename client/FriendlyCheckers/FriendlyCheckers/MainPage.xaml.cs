using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        Canvas mainCanvas;
        Rectangle[,] spaces;
        Checker[,] pieces;
        int row_W = 8;
        int w = 400, h = 400;
        public enum GameType { OUT_OF_GAME, SINGLE_PLAYER, ONLINE_MULTI, LOCAL_MULTI };
        private GameType game_type = GameType.OUT_OF_GAME;

        public MainPage()
        {
            InitializeComponent();
            Color c = new Color();
            c.R = c.G = c.B = 0;
            c.A = 150;
            Shader.Fill = new SolidColorBrush(c);
            ContentPanel.Children.Remove(Shader);
            RemoveInGameStats();
            mainCanvas = new Canvas();
            mainCanvas.Width = w;
            mainCanvas.Height = h;
            ContentPanel.Children.Add(mainCanvas);
            createBoard();
            createPieces();

            move(2, 0, 3, 1);
            move(2, 2, 3, 3);
            move(5, 1, 4, 0);

            scramble();
        }
        private void createPieces()
        {
            Color DarkRed = new Color();
            DarkRed.R = 50;
            DarkRed.G = 0;
            DarkRed.B = 0;
            DarkRed.A = 255;

            Color DarkGrey = new Color();
            DarkGrey.R = 20;
            DarkGrey.G = 20;
            DarkGrey.B = 20;
            DarkGrey.A = 255;

            pieces = new Checker[8,8];
            int row = 0, col = 0;
            for (int k = 0; k < 24; k++)
            {
                row = (k / 4) + ((k>=12)? 2 : 0);
                col = 2*(k % 4) + (row%2==0?0:1);
                pieces[row,col] = new Checker(row, col, (k < 12) ? Colors.Red : DarkGrey,
                                                (k < 12) ? DarkRed : Colors.Black);
               
                mainCanvas.Children.Add(pieces[row,col].getEl2());
                mainCanvas.Children.Add(pieces[row,col].getEl1());
            }
        }
        private void createBoard()
        {
            Color DarkerRed = new Color();
            DarkerRed.R = 180;
            DarkerRed.G = 0;
            DarkerRed.B = 0;
            DarkerRed.A = 255;
            spaces = new Rectangle[row_W, row_W];
            for (int k = 0; k < row_W; k++)
            {
                for (int i = 0; i < row_W; i++)
                {
                    spaces[k, i] = new Rectangle();
                    spaces[k,i].MinWidth = 50;
                    spaces[k,i].MinHeight = 50;
                    spaces[k, i].Fill = new SolidColorBrush(((i + k) % 2 == 0) ? DarkerRed : Colors.Black);
                    int lm = (k*50);
                    int tm = (i*50) - 100;
                    spaces[k,i].Margin = new Thickness(lm,tm,w-lm,h-tm);
                    mainCanvas.Children.Add(spaces[k,i]);    
                }
            }
        }

        private void ClearMenu()
        {
            ContentPanel.Children.Remove(singleplayer);
            ContentPanel.Children.Remove(multiplayer_local);
            ContentPanel.Children.Remove(multiplayer_online);
            LayoutRoot.Children.Remove(TitlePanel);
        }
        private void RemoveInGameStats()
        {
            HiddenPanel.Children.Remove(Timer);
            HiddenPanel.Children.Remove(Moves);
            HiddenPanel.Children.Remove(Versus);
            ContentPanel.Children.Remove(quit);
            ContentPanel.Children.Remove(Shader);
            ContentPanel.Children.Remove(Search);
        }
        private void AddInGameStats()
        {
            HiddenPanel.Children.Add(Versus);
            HiddenPanel.Children.Add(Timer);
            HiddenPanel.Children.Add(Moves);
            ContentPanel.Children.Add(quit);
        }
        public void scramble()
        {
            for (int k = 0; k < 1000; k++)
            {
                Random rand = new Random();
                int row1 = rand.Next(0, row_W);
                int col1 = rand.Next(0, row_W);
                int row2 = rand.Next(0, row_W);
                int col2 = rand.Next(0, row_W);
                move(row1, col1, row2, col2);
            }
        }
        public Checker delete(int x, int y)
        {
            Checker temp = pieces[x, y];
            mainCanvas.Children.Remove(temp.getEl2());
            mainCanvas.Children.Remove(temp.getEl1());
            pieces[x, y] = null;

            return temp;
        }
        private bool validMove(int x1, int y1, int x2, int y2)
        {
            if (x1 > row_W || x2 > row_W || y1 > row_W || y2 > row_W) return false;
            if (pieces[x1, y1] == null) return false;
            if (pieces[x2, y2] != null) return false;

            return true;
        }
        public void move(int x1, int y1, int x2, int y2)
        {
            if (!validMove(x1, y1, x2, y2)) return;

            Checker temp = delete(x1, y1);
            pieces[x2, y2] = new Checker(x2, y2, temp.getColor(),temp.getBG());

            mainCanvas.Children.Add(pieces[x2, y2].getEl2());
            mainCanvas.Children.Add(pieces[x2, y2].getEl1());
        }
        public void SinglePlayer_Setup(object sender, RoutedEventArgs e)
        {
            ClearMenu();
            Versus.Text = "Player 1 vs. Computer";
            AddInGameStats();
            game_type = GameType.SINGLE_PLAYER;
            resetBoard();
        }
        public void Local_Multi_Setup(object sender, RoutedEventArgs e)
        {
            ClearMenu();
            Versus.Text = "Player 1 vs. Player 2";
            AddInGameStats();
            game_type = GameType.LOCAL_MULTI;
            resetBoard();
        }
        public void Online_Multi_Setup(object sender, RoutedEventArgs e)
        {
            ClearMenu();
            Versus.Text = "Player 1 vs. [Searching...]";
            AddInGameStats();
            game_type = GameType.ONLINE_MULTI;
            resetBoard();
            ContentPanel.Children.Add(Shader);
            ContentPanel.Children.Add(Search);
        }
        public void Menu_Setup(object sender, RoutedEventArgs e)
        {
            RemoveInGameStats();
            LayoutRoot.Children.Add(TitlePanel);
            ContentPanel.Children.Add(singleplayer);
            ContentPanel.Children.Add(multiplayer_local);
            ContentPanel.Children.Add(multiplayer_online);
            game_type = GameType.OUT_OF_GAME;
            scramble();
        }
        public void resetBoard()
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
        }
        public GameType getGameType()
        {
            return game_type;
        }
    }
    public class Checker
    {
        Ellipse el1, el2;
        int offset = -100;
        int x, y;
        Color color, bg;
        public Checker(int x, int y, Color color, Color bg)
        {
            el1 = new Ellipse();
            el2 = new Ellipse();
            this.x = x;
            this.y = y;
            this.color = color;
            this.bg = bg;

            el1.Width = 50;
            el1.MinWidth = 50;
            el1.Height = 50;
            el1.MinHeight = 50;
            el1.Fill = new SolidColorBrush(color);
            el1.Margin = new Thickness(y * 50 - 2, x * 50 - 2 + offset,
                400 - x * 50 + 2, 400 - y * 50 + 2 - offset);

            el2.Width = 50;
            el2.MinWidth = 50;
            el2.Height = 50;
            el2.MinHeight = 50;
            el2.Margin = new Thickness(y * 50, x * 50 + offset,
                400 - x * 50, 400 - y * 50 - offset);
            el2.Fill = new SolidColorBrush(bg);
        }
        public Ellipse getEl1() { return el1; }
        public Ellipse getEl2() { return el2; }
        public int getX() { return this.x; }
        public int getY() { return this.y; }
        public Color getColor() { return this.color; }
        public Color getBG() { return this.bg; }
    }
}