using System;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows;
using System.Collections.Generic;

namespace FriendlyCheckers
{
    public class DataHandler
	{
        private String data_file_name;
        private SaveData current_save;
        private String username;
        private String password;

        public DataHandler()
        {
            this.data_file_name = "fc4.dat";
            this.username = this.password = "";
            loadCreds();
        }
        public void saveCreds()
        {
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream fileStream = new IsolatedStorageFileStream(data_file_name, FileMode.Create, myIsolatedStorage);
                StreamWriter writer = new StreamWriter(fileStream);
                writer.WriteLine(username);
                writer.WriteLine(password);
                writer.Close();
            }
            catch (IsolatedStorageException) { };
        }
        public void loadCreds()
        {
            try
            {
                IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication();
                IsolatedStorageFileStream FS = ISF.OpenFile(data_file_name, FileMode.Open, FileAccess.Read);
                StreamReader SR = new StreamReader(FS);

                Boolean first = true;
                while (!SR.EndOfStream)
                {
                    if (first)
                        username = SR.ReadLine();
                    else
                        password = SR.ReadLine();
                    first = false;
                }
                if (username == null || password == null || username.Equals("") || password.Equals(""))
                    username = password = "";
                SR.Close();
            }
            catch (IsolatedStorageException)
            {
                username = password = "";
            }
        }
        public void setSaveData(SaveData sd) { this.current_save = sd; }
        public SaveData getCurrentSaveData() { return current_save; }
        public String getUserName() { return username; }
        public String getPassword() { return password; }
        public void setCreds(String username, String password)
        {
            if (username.Equals("") || password.Equals("")) return;
            this.username = username;
            this.password = password;
            saveCreds();
        }
        public Boolean hasCreds()
        {
            return !(username.Equals("") && password.Equals(""));
        }
	}
    public class GameData
    {
        private List<MoveAttempt> moves;
        private PieceColor whoseMove;

        public GameData(List<MoveAttempt> moves, PieceColor whoseMove)
        {
            this.moves = moves;
            this.whoseMove = whoseMove;
        }
        public List<MoveAttempt> getMoves() { return moves; }
        public PieceColor getWhoseMove() { return whoseMove; }

        public String toString() {
            String s = "";
            if (whoseMove == PieceColor.BLACK) {
                s += "B";
            } else {
                s += "R";
            }
            foreach (MoveAttempt a in moves) {
                s += a.toString();
                s += "|"; 
            }
            return s; 
        }
        public static GameData fromString(String ma) {
            PieceColor whoseM;
            List<MoveAttempt> lala =new List<MoveAttempt>();
            if (ma.Equals(""))
            {
                whoseM = PieceColor.BLACK;
                return new GameData(lala, whoseM);
            }

            if (ma[0] == 'B') {
                whoseM = PieceColor.BLACK;
            } else if (ma[0] == 'R') {
                whoseM = PieceColor.BLACK;
            } else {
                throw new Exception();
            }
            String mas = ma.Substring(1); 
            String []moves = mas.Split(new String[] {"|"}, StringSplitOptions.None); 
            moves[moves.Length -1] = null; 
            foreach (String move in moves) {
                if (move == null) {
                    break;
                }
                lala.Add(MoveAttempt.fromString(move));
            }
            return new GameData(lala, whoseM); 

        }
    }
    public class SaveData
    {
        private int matchID;
        private string opponent;
        private int numMoves;
        private PieceColor myColor, whoseMove;
        public SaveData(int matchID, string opponent, int numMoves, PieceColor myColor, PieceColor whoseMove)
        {
            this.matchID = matchID;
            this.opponent = opponent;
            this.numMoves = numMoves;
            this.myColor = myColor;
            this.whoseMove = whoseMove;
        }
        public int getMatchID() { return matchID; }
        public string getOpponent() { return opponent; }
        public int getNumMoves() { return numMoves; }
        public PieceColor getPlayerColor() { return myColor; }
        public PieceColor getWhoseTurn() { return whoseMove; }
        public static SaveData fromString(String str)
        {
            PieceColor p, whoseMove;
            string[] finished = str.Split(new string[] { " " }, StringSplitOptions.None);
            if (finished[0].Equals("")) return null;

            p = finished[3].Equals("BLACK") ? PieceColor.BLACK : PieceColor.RED;
            whoseMove = finished[4].Equals("BLACK") ? PieceColor.BLACK : PieceColor.RED;

           return new SaveData(Convert.ToInt32(finished[0]), finished[1], Convert.ToInt32(finished[2]), p, whoseMove);

        }
    }
}
