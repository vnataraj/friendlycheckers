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
        private static Color Valid;
        private static Color Invalid;

        private static int checkerX, checkerY;
        public static int w = 400, h = 400;
        private static Canvas mainCanvas;
        private static Boolean FORCE_JUMP = false, ROTATE = false, DIFFICULT = false;
        private static GameLogic logic;
        private static DataHandler dataDude;
        private static NetworkLogic netLogic;
        private static BoardSpace[,] spaces;
        private static Checker[,] pieces;
        private SaveDataBox MatchMaking;
        private List<SaveDataBox> saveButtons;

        private static Boolean rotated = false;
        private static Boolean wait_for_timer = false, wait_for_computer = false, used_make_move = false;
        private static int row_W = 8;
        private static DispatcherTimer TURN_TIMER, COMPUTER_DELAY;
        public enum GameState { OUT_OF_GAME, END_GAME, OPTIONS, ABOUT, CREDS, SAVE_GAME, SINGLE_PLAYER, ONLINE_MULTI, LOCAL_MULTI };
        public static GameState game_state = GameState.OUT_OF_GAME;

        public MainPage()
        {
            InitializeComponent();
            InitializeColors();
            LayoutRoot.Children.Remove(OptionsPanel);
            LayoutRoot.Children.Remove(AboutPanel);
            LayoutRoot.Children.Remove(CredPanel);
            LayoutRoot.Children.Remove(SaveGamePanel);
            ContentPanel.Children.Remove(back_to_saves);
            ContentPanel.Children.Remove(RefreshInGameButton);
            checkerX = checkerY = -1;

            netLogic = new NetworkLogic();
            dataDude = new DataHandler();
            ResetCredsPanel();

            COMPUTER_DELAY = new DispatcherTimer();
            COMPUTER_DELAY.Tick += Computer_Delay_Tick;
            COMPUTER_DELAY.Interval = new TimeSpan(0, 0, 0, 0, 400);           

            TURN_TIMER = new DispatcherTimer();
            TURN_TIMER.Tick += timerTick;              // Everytime timer ticks, timer_Tick will be called
            TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 800);  // Timer will tick in 800 milliseconds. This is the wait between moves.
            Op_DiffEasy.IsChecked = true;

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
            
            Valid = new Color();
            Valid.R = Valid.B = 0;
            Valid.G = 255;
            Valid.A = 255;

            Invalid = new Color();
            Invalid.G = Invalid.B = 0;
            Invalid.R = 255;
            Invalid.A = 255;

        }
        private void createPieces()
        {
            logic = new GameLogic(row_W, row_W, FORCE_JUMP);
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
        }
        private void RemoveInGameStats()
        {
            LayoutRoot.Children.Remove(HiddenPanel);
            ContentPanel.Children.Remove(quit);
            ContentPanel.Children.Remove(Make_A_Move);
            ContentPanel.Children.Remove(WhoseTurn);
        }
        private void AddInGameStats()
        {
            LayoutRoot.Children.Add(HiddenPanel);
            ContentPanel.Children.Add(quit);
            ContentPanel.Children.Add(Make_A_Move);
            ContentPanel.Children.Add(WhoseTurn);
        }
        private void SinglePlayer_Setup(object sender, RoutedEventArgs e)
        {
            game_state = GameState.SINGLE_PLAYER;
            TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0); 
            ClearMenu();
            LayoutRoot.Children.Remove(TitlePanel);
            Versus.Text = "Player 1 vs. Computer";
            AddInGameStats();
            resetBoard();
        }
        private void Local_Multi_Setup(object sender, RoutedEventArgs e)
        {
            game_state = GameState.LOCAL_MULTI;
            if(ROTATE)
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 800); 
            else
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0); 
            ClearMenu();
            LayoutRoot.Children.Remove(TitlePanel);
            Versus.Text = "Player 1 vs. Player 2";
            AddInGameStats();
            ContentPanel.Children.Remove(Make_A_Move);
            resetBoard();
        }
        private static Boolean InLocalGame()
        {
            return (game_state == GameState.SINGLE_PLAYER || game_state==GameState.LOCAL_MULTI);
        }
        private Boolean MenuState()
        {
            return (game_state == GameState.CREDS || game_state == GameState.ABOUT || game_state == GameState.OPTIONS || game_state == GameState.SAVE_GAME);
        }
        private void Online_Multi_Setup(object sender, RoutedEventArgs e)
        {
            if (game_state == GameState.SAVE_GAME)
            {
                LayoutRoot.Children.Remove(SaveGamePanel);
                ContentPanel.Children.Remove(quit);
                ContentPanel.Children.Add(back_to_saves);
                ContentPanel.Children.Add(RefreshInGameButton);
                ContentPanel.Children.Add(mainCanvas);
            }
            game_state = GameState.ONLINE_MULTI;
            TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0);
            ClearMenu();
            LayoutRoot.Children.Remove(TitlePanel);
            AddInGameStats();
            ContentPanel.Children.Remove(quit);
            ContentPanel.Children.Remove(Make_A_Move);
        }
        private void SaveGame_Setup(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("inside saveGame_setup called."); 
            if (!netLogic.getInternetState())
            {
                MessageBox.Show("You need to turn on data or connect to wifi to use that feature.");
                return;
            }
            System.Diagnostics.Debug.WriteLine("getSaveDataCalled."); 
            game_state = GameState.SAVE_GAME;
            PageTitle.Text = "Active Games";
            NewGame.IsEnabled = true;
            RemoveInGameStats();
            ClearMenu();
            if(!LayoutRoot.Children.Contains(TitlePanel))
                LayoutRoot.Children.Add(TitlePanel);
            ContentPanel.Children.Remove(mainCanvas);
            ContentPanel.Children.Remove(back_to_saves);
            ContentPanel.Children.Remove(RefreshInGameButton);
            ContentPanel.Children.Add(quit);
            if (!dataDude.hasCreds())
            {
                Show_Creds(sender, e);
                return;
            }
            else
            {
                LayoutRoot.Children.Add(SaveGamePanel);
                RefreshSaveDataBoxes();
            }
        }
        private void RefreshSaveDataBoxes()
        {
            System.Diagnostics.Debug.WriteLine("RefreshSaveDataBoxes called.");
            saveButtons = new List<SaveDataBox>();
            List<SaveData> saveData = netLogic.getSaveData(dataDude.getUserName());
            if (saveData == null) return;
            System.Diagnostics.Debug.WriteLine("SaveGamePanel count before: " + SaveGamePanel.Children.Count);
            //Clear old savedata
            for (int k = SaveGamePanel.Children.Count - 1; k >= 0; k--)
                SaveGamePanel.Children.RemoveAt(k);
            SaveGamePanel.Children.Add(NewGame);
            SaveGamePanel.Children.Add(RefreshButton);
            System.Diagnostics.Debug.WriteLine("SaveGamePanel count after: " + SaveGamePanel.Children.Count);
            //

            int ind = 0;
            foreach (SaveData sd in saveData) // add enabled boxes
            {
                if (!sd.getPlayerColor().Equals(sd.getWhoseTurn())) continue;

                SaveDataBox sdbox = new SaveDataBox(ind, sd);
                saveButtons.Add(sdbox);
                sdbox.getButton().Click += SaveDataBoxClick;
                SaveGamePanel.Children.Add(sdbox.getButton());
                ind++;
            }
            foreach (SaveData sd in saveData)// then add disabled boxes.
            {
                if (sd.getPlayerColor().Equals(sd.getWhoseTurn())) continue;

                SaveDataBox sdbox = new SaveDataBox(ind, sd);
                sdbox.setEnabled(false);
                saveButtons.Add(sdbox);
                sdbox.getButton().Click += SaveDataBoxClick;
                SaveGamePanel.Children.Add(sdbox.getButton());
                ind++;
            }
        }
        private void Menu_Setup(object sender, RoutedEventArgs e)
        {
            if (InLocalGame() && MessageBox.Show("The current game will end.", "Exit to main menu?", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)return;
            RemoveInGameStats();
            clearCredStats();
            if (MenuState())
            {
                ContentPanel.Children.Add(mainCanvas);
                LayoutRoot.Children.Remove(OptionsPanel);
                LayoutRoot.Children.Remove(AboutPanel);
                LayoutRoot.Children.Remove(CredPanel);
                LayoutRoot.Children.Remove(SaveGamePanel);
                quit.Content = "Quit to Menu";
            }
            else
                LayoutRoot.Children.Add(TitlePanel);
            PageTitle.Text = "Checkers";
            Versus.Text = "Player 1 vs. Player 2";

            ///// restore Login stuff
            ResetCredsPanel();
            ContentPanel.Children.Remove(back_to_saves);
            /////

            ///// restore main menu
            ContentPanel.Children.Add(singleplayer);
            ContentPanel.Children.Add(multiplayer_local);
            ContentPanel.Children.Add(multiplayer_online);
            ContentPanel.Children.Add(options);
            ContentPanel.Children.Add(about);
            /////

            game_state = GameState.OUT_OF_GAME;

            ///// reset game vars
            wait_for_computer = false;
            wait_for_timer = false;
            rotated = false;
            checkerX = checkerY = -1;
            resetBoard();
            rotateBoard90();
            /////
        }
        private void LoginSuccess(bool success)
        {
            LoginConfirm.Foreground = new SolidColorBrush(success ? Valid : Invalid);
            LoginConfirm.BorderBrush = new SolidColorBrush(success ? Valid : Invalid);
            LoginConfirm.Content = success ? "Success" : "Failed";
        }
        private void CheckUserValidity(bool valid)
        {
            AvailableRect.Foreground = new SolidColorBrush(valid ? Valid : Invalid);
            AvailableRect.BorderBrush = new SolidColorBrush(valid ? Valid : Invalid);
            AvailableRect.Content = valid ? "Available" : "Unavailable";
        }
        private void Show_Options(object sender, RoutedEventArgs e)
        {
            game_state = GameState.OPTIONS;
            quit.Content = "Save and Quit to Menu";
            PageTitle.Text = "Options";
            ClearMenu();
            LayoutRoot.Children.Add(OptionsPanel);
            ContentPanel.Children.Remove(mainCanvas);
            ContentPanel.Children.Add(quit);
        }
        private void Show_Creds(object sender, RoutedEventArgs e)
        {
            if (!netLogic.getInternetState())
            {
                MessageBox.Show("You need to turn on data or connect to wifi to use that feature.");
                return;
            }
            game_state = GameState.CREDS;
            quit.Content = "Save and Quit to Menu";
            PageTitle.Text = "Credentials";
            LayoutRoot.Children.Remove(OptionsPanel);
            LayoutRoot.Children.Add(CredPanel);
            if (!CredPanel.Children.Contains(NewUser))
                CredPanel.Children.Add(NewUser);
            FocusLost(sender, e);
        }
        private void NewGame_Setup(object sender, RoutedEventArgs e)
        {
            /// other stuff will be done, then the game will be displayed.
            Online_Multi_Setup(sender, e);
        }
        private void Show_About(object sender, RoutedEventArgs e)
        {
            game_state = GameState.ABOUT;
            PageTitle.Text = "About";
            ClearMenu();
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
            createPieces(); //makes new GameLogic instance
            Moves.Text = "Moves: 0";
            WhoseTurn.Text = "Black to move next.";
            rotated = false;
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
        public GameState getGameType()
        {
            return game_state;
        }
        private void ResetCredsPanel()
        {
            CredPanel.Children.Remove(ChangeUser);
            CredPanel.Children.Remove(NewUser);
            CredPanel.Children.Remove(CheckAvailability);
            CredPanel.Children.Remove(AvailableRect);
            clearCredStats();
            Login.Content = "Login";
            UserName.Text = "";
            Password.Password = "";

            if (dataDude.hasCreds())
            {
                UserName.Text = dataDude.getUserName();
                Password.Password = dataDude.getPassword();

                UserName.IsEnabled = false;
                Password.IsEnabled = false;
                Login.IsEnabled = false;
                ChangeUser.IsEnabled = true;

                LoginName.Text = dataDude.getUserName();
                LoginSuccess(true);
            }
            else
            {
                UserName.IsEnabled = true;
                Password.IsEnabled = true;
                Login.IsEnabled = true;
                ChangeUser.IsEnabled = true;
            }
            CredPanel.Children.Add(ChangeUser);
            CredPanel.Children.Add(NewUser);
        }
        private void postGameStateToServer()
        {
            SaveData old = dataDude.getCurrentSaveData();
            SaveData sd = new SaveData(old.getMatchID(), old.getOpponent(), logic.getMoveNumber(), old.getPlayerColor(), logic.whoseMove(), old.getWinner());
            GameData gd = new GameData(logic.getMoveAttemptsMade(), logic.whoseMove());
            netLogic.writeToServer(dataDude.getUserName(), sd, gd);
            System.Diagnostics.Debug.WriteLine("Write to server: [MatchId](" + old.getMatchID() + ") [Opponent](" + old.getOpponent() +
                ") [NumMoves](" + logic.getMoveNumber() + ") [PlayerColor](" + old.getPlayerColor() + ") [WhoseMove](" + logic.whoseMove() + ")");
            dataDude.setSaveData(sd);
        }
        
        //////////
        //// HANDLERS FOR BOARD, PIECES, LOGIC AND HIGHLIGHTING LOCATED BELOW HERE
        //////////
        private void RefreshInGame(object o, RoutedEventArgs e)
        {
            GameData dat = netLogic.getGameData(dataDude.getUserName(), dataDude.getCurrentSaveData().getMatchID());
            SaveData old = dataDude.getCurrentSaveData();
            SaveData save = new SaveData(old.getMatchID(),old.getOpponent(),logic.getMoveNumber(),old.getPlayerColor(),dat.getWhoseMove(),old.getWinner());
            dataDude.setSaveData(save);
            int turn = logic.getMoveNumber();

            List<MoveAttempt> moves = dat.getMoves();
            System.Diagnostics.Debug.WriteLine("~~~~~TURN: " + turn + " COUNT: " + moves.Count);
            for (int k = turn; k < moves.Count; k++ )
            {
                System.Diagnostics.Debug.WriteLine("MOVES ARE BEING MADE.");
                MoveAttempt move = moves[k];
                Move m = logic.makeMove(move);
                handleMove(m);
            }
            if (logic.whoseMove().Equals(PieceColor.RED))
            {
                if (!rotated && save.getPlayerColor().Equals(PieceColor.RED))
                    rotateBoard180();
                WhoseTurn.Text = "Red to move next.";
            }
            else
                WhoseTurn.Text = "Black to move next.";

        }
        private void Refresh(object o, RoutedEventArgs e)
        {
            bool hasMatchMaking = MatchMaking == null ? false : SaveGamePanel.Children.Contains(MatchMaking.getButton());
            int thenGames = SaveGamePanel.Children.Count - 2;
            RefreshSaveDataBoxes();
            int nowGames = SaveGamePanel.Children.Count - 2;
            System.Diagnostics.Debug.WriteLine("GAMES: "+thenGames+", "+nowGames);
            if (thenGames > nowGames)
                addMatchmakingBox();
        }
        private void addMatchmakingBox()
        {
            SaveData dat = new SaveData(1, "Matchmaking...", 0, PieceColor.BLACK, PieceColor.BLACK, PieceColor.NONE);
            MatchMaking = new SaveDataBox(SaveGamePanel.Children.Count - 2, dat);
            MatchMaking.getButton().IsEnabled = false;
            SaveGamePanel.Children.Add(MatchMaking.getButton());
            NewGame.IsEnabled = false;
        }
        private void FindRandomUser(object o, RoutedEventArgs e)
        {
            Versus.Text = "Player 1 vs. [Searching...]";
            resetBoard();
            netLogic.queueMatch(dataDude.getUserName());
            addMatchmakingBox();
        }
        private void Find_Player_Setup(object o, RoutedEventArgs e)
        {


        }
        private void SaveDataBoxClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("SaveDataBoxClick("+sender.ToString()+","+ e.ToString()+")");
            foreach (SaveDataBox box in saveButtons)
            {
                if (box.getButton().Equals(sender))
                {
                    System.Diagnostics.Debug.WriteLine("A save data box was clicked!");
                    SaveData data = box.getSaveData();
                    netLogic.getGameData(dataDude.getUserName(), data.getMatchID());
                    LoadSaveGame(box.getSaveData());
                    Online_Multi_Setup(sender, e);
                    ContentPanel.Children.Remove(quit);
                }
            }
        }
        public void LoadSaveGame(SaveData data)
        {
            System.Diagnostics.Debug.WriteLine("LoadSaveGame(" + data.ToString() + ")------------------");
            GameData gameData = netLogic.getGameData(dataDude.getUserName(), data.getMatchID());
            System.Diagnostics.Debug.WriteLine("Is there game data? " + (gameData != null));

            if (gameData == null) return;
            dataDude.setSaveData(data);
            resetBoard();

            System.Diagnostics.Debug.WriteLine("Whose turn is it in GameLogic? " + logic.whoseMove());
            System.Diagnostics.Debug.WriteLine("Whose turn is it in GameData? " + gameData.getWhoseMove());
            System.Diagnostics.Debug.WriteLine("How many moves? " + data.getNumMoves());
            foreach (MoveAttempt move in gameData.getMoves())
            {
                Move m = logic.makeMove(move);
                handleMove(m);
            }

            Versus.Text = dataDude.getUserName() + " vs. " + data.getOpponent();
            Moves.Text = "Moves: " + data.getNumMoves();
            if (data.getWhoseTurn().Equals(PieceColor.RED))
            {
                rotateBoard180();
                WhoseTurn.Text = "Red to move next.";
            }
            else
                WhoseTurn.Text = "Black to move next.";

            System.Diagnostics.Debug.WriteLine("-----------------------------------------");
        }
        public static void MakeMove(int boardX, int boardY)
        {
            if (wait_for_timer || wait_for_computer || !canMove()) return;
            if (game_state == GameState.OUT_OF_GAME || game_state == GameState.END_GAME||(checkerX == -1 && checkerY == -1)) return;
            Move m;
            try
            {
                PieceColor whoseTurn = logic.whoseMove();
                int locX = checkerX;
                int locY = checkerY;
                // Unhighlight the selected piece
                handleHighlighting(checkerX, checkerY);
                m = logic.makeMove(locY, locX, (!rotated ? boardY : (row_W - boardY - 1)), (!rotated ? boardX : (row_W - boardX - 1)));
                handleMove(m);

                TURN_TIMER.Start();
                wait_for_timer = true;

                checkerX = checkerY = -1;
            }
            catch (PieceWrongColorException) { }
            catch (PlayerMustJumpException) { MessageBox.Show("You must take an available jump!"); }
            catch (WrongMultiJumpPieceException) { MessageBox.Show("You must finish the multijump!"); }
            catch (InvalidMoveException) { }
            catch (GameLogicException) { }
        }
        private static bool canMove()
        {
            return  !(game_state == GameState.END_GAME || (game_state == GameState.ONLINE_MULTI &&
                !logic.whoseMove().Equals(dataDude.getCurrentSaveData().getPlayerColor())));
        }
        private Boolean checkEndGame()
        {
            GameStatus status = logic.getGameStatus();
            if (status == GameStatus.BLACKWINS)
            {
                WhoseTurn.Text = "Black player wins!";
                System.Diagnostics.Debug.WriteLine("game status is BLACKWINS");
                return true;
            }
            else if (status == GameStatus.REDWINS)
            {
                WhoseTurn.Text = "Red player wins!";
                System.Diagnostics.Debug.WriteLine("game status is REDWINS");
                return true;
            }
            else if (status == GameStatus.DRAW)
            {
                WhoseTurn.Text = "Game is a draw.";
                System.Diagnostics.Debug.WriteLine("game status is DRAW");
                return true;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("game status is NOWINNER");
                return false;
            }
        }
        private void Make_Educated_Move(object sender, EventArgs e)
        {
            if (!canMove() || (wait_for_computer && sender.Equals(Make_A_Move))) return;
            if(sender.Equals(Make_A_Move))used_make_move = true;
            PieceColor whoseMove = logic.whoseMove();
            MoveAttempt a;
            if (DIFFICULT && wait_for_computer)
                a = logic.getHardMove();
            else
                a = logic.getEasyMove();

            Move m = logic.makeMove(a);
            handleMove(m);
            if (!TURN_TIMER.IsEnabled)
            {
                TURN_TIMER.Start();
                wait_for_timer = true;
            }
        }
        private void timerTick(object o, EventArgs e)
        {
            TURN_TIMER.Stop();
            if (checkEndGame())
            {
                game_state = GameState.END_GAME;
                return;
            }
            PieceColor last = logic.getWhoMovedLast();

            // if there is a double jump available and it isn't forced
            if (last.Equals(logic.whoseMove()))
            {
                // if black is making the jump
                if (!last.Equals(PieceColor.RED) || (game_state != GameState.SINGLE_PLAYER))
                {
                    if (used_make_move)
                        Make_Educated_Move(o, e);
                    else if (!FORCE_JUMP && game_state != GameState.ONLINE_MULTI && MessageBox.Show("Double Jump Available!", "Take the double jump?", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        logic.skipMultiJump();
                }
                else
                {
                    if (game_state == GameState.SINGLE_PLAYER)
                        COMPUTER_DELAY.Start();
                }
            }
            else
                  used_make_move = false;
            WhoseTurn.Text = (logic.whoseMove().Equals(PieceColor.RED) ? "Red" : "Black") + " to move next.";
            Moves.Text = "Moves: "+logic.getMoveNumber();
            wait_for_timer = false;
            if (ROTATE && game_state == GameState.LOCAL_MULTI)
                rotateBoard180();
            else if (game_state == GameState.SINGLE_PLAYER)
            {
                wait_for_computer = !wait_for_computer;
                if (wait_for_computer && logic.whoseMove().Equals(PieceColor.RED))
                    COMPUTER_DELAY.Start();
                else
                    wait_for_computer = false;
            }
            if (game_state == GameState.ONLINE_MULTI)
            {
                System.Diagnostics.Debug.WriteLine("Check one!");
                postGameStateToServer();
                System.Diagnostics.Debug.WriteLine("Check two!");
            }
        }
        private void Computer_Delay_Tick(object o, EventArgs e)
        {
            COMPUTER_DELAY.Stop();
            Make_Educated_Move(o, e);
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
            if (wait_for_timer || wait_for_computer || game_state==GameState.END_GAME || !canMove()) return;
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

            if (!ROTATE || game_state != GameState.LOCAL_MULTI)
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 0); 
            else
                TURN_TIMER.Interval = new TimeSpan(0, 0, 0, 0, 800); 
        }
        private void Process_Username(object sender, EventArgs e)
        {
            Boolean valid = !netLogic.checkUser(UserName.Text);
            if (UserName.Text.Equals("")) 
                valid = false;
            CheckUserValidity(valid);
        }
        private void Login_Confirm(object sender, EventArgs e)
        {
            bool create = Login.Content.Equals("Create Account"), success;
            if (create)
               success = netLogic.createUser(UserName.Text, Password.Password);
            else
               success = netLogic.login(UserName.Text, Password.Password);

            if (UserName.Text.Equals("") || Password.Password.Equals("")) 
                success = false;
            LoginSuccess(success);

            if (success)
            {
                dataDude.setCreds(UserName.Text, Password.Password);
                ResetCredsPanel();
            }
            else
                dataDude.setCreds("", "");
        }
        private void FocusLost(object sender, EventArgs e)
        {
            CheckAvailability.IsEnabled = true;
            AvailableRect.IsEnabled = true;
        }
        private void FocusGained(object sender, EventArgs e)
        {
            CheckAvailability.IsEnabled = false;
            AvailableRect.IsEnabled = true;
            clearCredStats();
        }
        private void Create_User(object o, EventArgs e)
        {
            UserName.Text = Password.Password = "";
            Login.Content = "Create Account";
            UserName.IsEnabled = true;
            Password.IsEnabled = true;
            Login.IsEnabled = true;
            CredPanel.Children.Add(CheckAvailability);
            CredPanel.Children.Add(AvailableRect);
            CredPanel.Children.Remove(NewUser);
            CredPanel.Children.Remove(ChangeUser);
            clearCredStats();
        }
        private void Change_User(object o, EventArgs e)
        {
            netLogic.login(UserName.Text, Password.Password);
            clearCredStats();
            UserName.IsEnabled = true;
            Password.IsEnabled = true;
            Login.IsEnabled = true;
            ChangeUser.IsEnabled = false;
        }
        private void clearCredStats()
        {
            LoginConfirm.Foreground = LoginConfirm.BorderBrush = new SolidColorBrush(Colors.White);
            LoginConfirm.Content = AvailableRect.Content = "";
            AvailableRect.Foreground = AvailableRect.BorderBrush = new SolidColorBrush(Colors.White);
        }
    }
    public class SaveDataBox
    {
        private SaveData data;
        private Button button;
        private bool highlighted;
        public SaveDataBox(int index, SaveData data) 
        { 
            this.data = data;
            this.highlighted = false;
            button = new Button();
            button.Content = "Moves: "+data.getNumMoves()+"          "+data.getOpponent()+"          ";
            button.HorizontalContentAlignment = HorizontalAlignment.Right;
            if (data.getOpponent().Equals("Matchmaking..."))
            {
                button.Content = "Matchmaking...";
                button.HorizontalContentAlignment = HorizontalAlignment.Center;
            }
            button.FontSize = 30;
            button.Height = 80;
            button.Width = 450;
            button.Margin = new Thickness(0, -500 + 120 * index, 0, 0);
        }
        public void Highlight(bool b){ highlighted = b; }
        public bool isHighlighted() { return highlighted; }
        public Button getButton() { return button; }
        public SaveData getSaveData() { return data; }
        public void setEnabled(bool b){ button.IsEnabled = b; }
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
            if (MainPage.game_state == MainPage.GameState.OUT_OF_GAME) return;
            MainPage.handleHighlighting(x,y);
        }
    }
}