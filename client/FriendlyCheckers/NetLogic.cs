using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace FriendlyCheckers
{
    public class NetLogic
    {
        /*
         * @vnataraj, CS252
         * ideas: to create a Network Logic object that can be called by any of the other objects(aka Caleb's GameLogic object and Joe's GUI)
         * basic concept idea is to create a service on the windows phone that calls a while loop every x seconds(defined in phone prefs)
         * provide getter/setter methods in case manual getting/setting is required
         * relay between all C# and server functions, no functions should be parsed from server directly in other objects
         * codes are 42.xx on success and 666.xx on failure
         * some functions are duplicates, see members for details
         */
        private List removals; //check with caleb on object types!!
        private List additions;
        private string returncode = "\0";
        private int gameID;
        private int matchID;
        private string username;
        private Thread poller;
        private B 
        
        public NetLogic(Move move, UserGame game){   // constructor for NetworkLogic object, querys server for data
            this.additions = move.getAdditions();
            this.removals = move.getRemovals();
            this.gameID = game.getGameID();
            this.matchID = game.getMatchID();
            this.username = game.getUsername();
            if (queryServer(move, position, win).Equals("42.01"))
            {
                writeToServer(move, position, win);
            }
            if (checkGameID(username) != gameID)
            {
                setGameID(username, gameID);
            }
        }

        public string queryServer(int move, int position)
        {
            //call PHP server for last known moves
            //return String (returncode) with either 42.xx for success or 666.xx for failure 
            return returncode;
        }
        public void writeToServer(int move, int position)
        {
            // writes information to server after querying when server was last modified
            //error codes still apply(42.xx for success, 666.xx for failure)
        }
        public int checkGameID(string username)
        {
            //checks gameID with current gameID stored on server
            return gameID;
        }
        public int checkMatchID(string username)
        {
            //checks matchID with current matchID stored on server
            return matchID;
        }
        /*public string setGameID(string username, int gameID)
        {
            //sets game ID on server, returns error code on fail and success code on success
            return returncode;
        }
        public string setMatchID(string username, int matchID)
        {
            // sets matchID to server, returns error code on fail and success code on success
            return returncode;
        }
         */
        public string callOnStart(string username)
        {
            //not too sure about the necessity of this function
            // querys server for all start data? polls every x seconds
            return returncode;
        }
        public GameLogic updateLogic(GameLogic logic)
        {
            return logic;
            // returns new GameLogic object based on server's stored logic
        }
        private void pollServer()
        {

        }
        private void instructThread()
        {
            bool b = true;
            while (b)
            {
                pollServer();
            }
        }
    }
}
