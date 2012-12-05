using System;
using SocketEx;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using System.Runtime.Serialization;
using Microsoft.Phone.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace FriendlyCheckers{
    public class UnknownException : System.Exception { }
    public class LoginException : System.Exception { }
    public class QueueMatchException : System.Exception { }
    public class RequestMatchException : System.Exception { }
    public class PollMatchException : System.Exception { }
    public class PollRequestException : System.Exception { }
    public class WriteToServerException : System.Exception { }
    public class CheckUserException : System.Exception { }
    public class CreateUserException : System.Exception { }
    public class GetSaveDataException : System.Exception { }
    public class GetGameDataException : System.Exception { }
    
    public class NetworkLogic
    {

        /*
         * @vnataraj, CS252
         * 
         */
        private HttpWebRequest webreq;
        private HttpWebRequest request;
        private HttpWebResponse response;
        private Stream dataStream;
        private StreamReader reader;
        private List<SaveData> saveData;
        private GameData gameData;

        private int moveNumber;
        private int gameID;
        private int matchID;

        private string username;
        private string opponentname;
        private string server = "pod2-3.cs.purdue.edu";
        private int port = 1200;
        private string serverpath;
        private string responseFromServer;
        private string returncode = "\0";
        private static string loginSuccess = "42.1";
        private static string queueMatchSuccess = "42.2";
        private static string requestMatchSuccess = "42.3";
        private static string pollMatchSuccess = "42.4";
        private static string writeToServerSuccess = "42.5";
        private static string pollRequestSuccess = "42.6";
        private static string acceptMatchSuccess="42.7";
        private static string pollMatchNoMoveSuccess="42.8";
        private static string getSaveDataSuccess = "42.9";
        private static string checkUserExistsSuccess = "42.10";
        private static string getGameDataSuccess = "42.12";
        private static string createUserSuccess = "42.13";
        private static string loginFailure = "666.1";
        private static string queueMatchFailure = "666.2";
        private static string requestMatchFailure = "666.3";
        private static string pollMatchFailure = "666.4";
        private static string writeToServerFailure = "666.5";
        private static string pollRequestFailure = "666.6";
        private static string acceptMatchFailure = "666.7";
        private static string getSaveDataFailure = "666.9";
        private static string checkUserFailure = "666.10";
        private static string getGameDataFailure = "666.12";
        private static string createUserFailure = "666.13";
        private static string unknownError = "666.666";

        private bool getLoginState;
        private bool getQueueState;
        private bool getRequestState;
        private bool getPollMatchState;
        private bool writeState;
        private bool getPollRequestState;
        private bool checkUserExistsState;
        private bool getSaveDataState;
        private bool getGameDataState;
        private bool communication;
        private bool getAcceptMatchState;
        private bool createUserState;
        private bool internetState;
        public NetworkLogic(){   // constructor for NetworkLogic object, querys server for data
            this.internetState = false;
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                this.internetState = true;
            }
            this.getQueueState = false;
            this.getLoginState = false;
            this.getRequestState = false;
            this.getPollRequestState = false;
            this.getPollMatchState = false;
            this.writeState = false;
            this.checkUserExistsState = false;
            this.communication=false;
            this.getGameDataState = false;
            this.getSaveDataState = false;
            this.getAcceptMatchState = false;
            this.createUserState = false;
            saveData = new List<SaveData>();
           // this.opponentname= game.getOpponentName();
            // do startup stuff
        }

        private void requestHandler(IAsyncResult result)
        {
            webreq = (HttpWebRequest)result.AsyncState;
            response = (HttpWebResponse)webreq.EndGetResponse(result);
            Stream dataStream = response.GetResponseStream();

            using (reader = new StreamReader(dataStream))
            {
                responseFromServer = reader.ReadToEnd();
                this.checker(responseFromServer);
                Deployment.Current.Dispatcher.BeginInvoke(() => {});
            }
            dataStream.Close();
            reader.Close();
            response.Close();
            return;
        }

        private void checker(string responseFromServer)
        {
            System.Diagnostics.Debug.WriteLine("in checker, responseFromServer is:" + responseFromServer + ":0"); 
            if (responseFromServer.Contains(queueMatchSuccess))
            {
                this.getQueueState = true;
                return;
            } 
            else if (responseFromServer.Contains(createUserSuccess))
            {
                this.createUserState=true;
                return;
            }
            else if (responseFromServer.Contains(requestMatchSuccess))
            {
                this.getRequestState = true;
                return;
            } 
            else if (responseFromServer.Contains(pollMatchSuccess))
            {
                this.getPollMatchState = true;
                return;
            } 
            else if (responseFromServer.Contains(writeToServerSuccess))
            {
                this.writeState = true;
                return;
            }
            else if (responseFromServer.Contains(acceptMatchSuccess))
            {
                this.getAcceptMatchState=true;
                return;
            }
            else if (responseFromServer.Contains(pollRequestSuccess))
            {
                this.getRequestState = true;
                return;
            }
            else if (responseFromServer.Contains(checkUserExistsSuccess))
            {
                this.checkUserExistsState = true;
                System.Diagnostics.Debug.WriteLine("set user to exists");
                return;
            }
            else if(responseFromServer.Contains(getGameDataSuccess))
            {
                decodeGameData(responseFromServer);
                return;
            }
            else if(responseFromServer.Contains(getSaveDataSuccess))
            {
                this.getSaveDataState = true;
                parseSaveData(responseFromServer);
                return;
            }
            else if (responseFromServer.Contains(loginSuccess))
            {
                System.Diagnostics.Debug.WriteLine("bbbbb");
                this.getLoginState = true;
                return;
            }
            else if (responseFromServer.Contains(checkUserFailure))
            {
                this.checkUserExistsState = false;
                return;
            }
            else if(responseFromServer.Contains(getGameDataFailure))
            {
                this.getGameDataState=false;
                return;
            }
            else if (responseFromServer.Contains(createUserFailure))
            {
                this.createUserState=false;
                return;
            }
            else if (responseFromServer.Contains(loginFailure))
            {
                this.getLoginState = false;
                return;
            }
            else if (responseFromServer.Contains(queueMatchFailure))
            {
                this.getQueueState = false;
                return;
            }
            else if (responseFromServer.Contains(requestMatchFailure))
            {
                this.getRequestState = false;
                return;
            }
            else if (responseFromServer.Contains(pollMatchFailure))
            {
                this.getPollMatchState = false;
                return;
            }
            else if (responseFromServer.Contains(writeToServerFailure))
            {
                this.writeState = false;
                return;
            }
            else if (responseFromServer.Contains(pollRequestFailure))
            {
                this.getPollRequestState = false;
                return;
            }
            else if(responseFromServer.Contains(getSaveDataFailure))
            {
                this.getSaveDataState=false;
                return;
            }
            return;
        }
        private void socketHandler(string serverpath)
        {
            var connection = new TcpClient(server, port);
            var stream = connection.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);
            writer.WriteLine(serverpath);
            writer.Flush();

            var fullMessage = new StringBuilder();

            string message;
            while ((message = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(message))
                    break;

                fullMessage.AppendLine(message);
            }

            string str = fullMessage.ToString();
            this.checker(str);
            return;
        }
        private void parseSaveData(string a)
        {
            System.Diagnostics.Debug.WriteLine("here's the string response from server  :"+a+":0");
            saveData=new List<SaveData>();
            string[] lines = a.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            for (int i = 1; i < lines.Length; i++)
            {
                SaveData data = SaveData.fromString(lines[i]);
                if (data == null) continue;
                saveData.Add(data);
            }
            return;
        }
        private string encodeGameData(string gameData)
        {
            return HttpUtility.UrlEncode(gameData);
        }
        private void decodeGameData(string responseFromServer)
        {
            System.Diagnostics.Debug.WriteLine("Here's the responseFromServer string:" +
                responseFromServer + ":0");
            string[] lines = responseFromServer.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string a = HttpUtility.UrlDecode(lines[1]);
            gameData = GameData.fromString(a);
            if (gameData == null)
                gameData = new GameData(new List<MoveAttempt>(), PieceColor.BLACK);
            System.Diagnostics.Debug.WriteLine("Lines[1] = " + lines[1] + " a=" + a + " gameData=NULL? " + (gameData == null));
            this.getGameDataState = true;
            return;
        }
        public bool checkUser(string username) //should be void
        {
            serverpath ="?message=CheckUser&" + "Username=" + username;
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + e.ToString());  //...
                CheckUserException cue = new CheckUserException();
                throw cue;
            }
            if (this.checkUserExistsState)
            {
                return true;
            }
            return false;
        }
        public List<SaveData> getSaveData(string username)
        {
            System.Diagnostics.Debug.WriteLine("getSaveData called!!!!!-------------"); 
            serverpath = "?message=GetSaveData&" + "Username=" + username;
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + we.ToString());
                throw new GetSaveDataException();
            }
            if (this.getSaveDataState)
            {
                return saveData;
            }
            return null;
        }
        public GameData getGameData(string username, int matchID)
        {
            System.Diagnostics.Debug.WriteLine("entered getGameData");
            serverpath = "?message=GetGameData&" + "Username=" + username + "&MatchID=" + matchID.ToString();
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + we.ToString());
                GetGameDataException ggde = new GetGameDataException();
                throw ggde;
            }
            if (this.getGameDataState)
            {
                return gameData;
            }
            return null;
        }
        public bool writeToServer(string username, SaveData saveData, GameData gameData)
        {
            System.Diagnostics.Debug.WriteLine("entered writeToServer");
            string str = gameData.ToString();
            serverpath ="?message=RecordMove&" + "Username=" + username +
                "&MatchID=" + saveData.getMatchID().ToString() + "&Notation=" + encodeGameData(gameData.toString()); // parse move!!!
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException we)
            {
                we.ToString();
                  //...
                WriteToServerException wtse = new WriteToServerException();
                throw wtse;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception : " + e.ToString()); // debug
            }
            if (this.writeState)
            {
                return true;
            }
            return false;
        }

        public bool login(string username, string password)// should be void
        {
            serverpath="?message=Login&" + "Username=" + username + "&Password=" + password;
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException e)
            {
                Console.WriteLine("Caught exception : "+e.ToString());  //...
                LoginException le = new LoginException();
                throw le;
            }
            if (this.getLoginState)
            {
                return true;
            }
            return false;
        }
        private string user;
        public bool createUser(string username, string password)
        {
            if (createUserState && user!=null && user.Equals(username))
            {
                return false;
            }
            user = username;
            serverpath = "?message=CreateUser&" + "Username=" + username + "&Password=" + password;
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException we)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception : " + we.ToString());
            }
            if (this.createUserState)
            {
                return true;
            }
            return false;
        }
        public void queueMatch(string username, int gameID)
        {
            string serverpath = "?message=QueueMatch&" + "UserID=" + username + "&GameID="+gameID.ToString();
            try
            {
                this.socketHandler(serverpath);
            } 
            catch (WebException e)
            {
                Console.WriteLine("Caught error :" + e.ToString());  //...
                QueueMatchException qe = new QueueMatchException();
                throw qe;
            }
            return;
        }

        public void requestMatch(string username, int gameID, string opponentName)
        {
            serverpath = "?message=RequestMatch&" + "Username=" + username + "&GameID=" + gameID.ToString() + "&Opponentname="+opponentName;
            try
            {
                this.socketHandler(serverpath);
            }
            catch (WebException we)
            {
                we.ToString();
                  //...
                RequestMatchException rme = new RequestMatchException();
                throw rme;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception : " + e.ToString()); // debug
            }
            return;
        } 
        public void pollRequest(string username) // called by Caleb's GameLogic to poll server, threads not necessary!
        {
            /*
            try
            {
                //HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=PollRequest&" + "Username=" + username);
                //HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
             */
        }
        public void pollMatch(string username, int gameID, int matchID)
        {
            /*
            try
            {
                HttpWebRequest request = (HttpWebRequest)Webrequest.Create(server + path + "?message=PollMatch&" + "Username=" + username + "&GameID=" + gameID.ToString);
                HttpWebResponse response = (HttpWebResponse)request.GetResponseStream();
            }
            catch (Exception e)
            {
                //...
            }
             */
        }
        public bool getCheckUserState()
        {
            return checkUserExistsState;
        }
        private void setCommunicationState(bool b)
        {
            this.communication = b;
        }
        public bool getGetLoginState()
        {
            return getLoginState;
        }
        public bool getGetRequestState()
        {
            return getRequestState;
        }
        public bool getQueueMatchState()
        {
            return getQueueState;
        }
        public bool getGetSaveDataState()
        {
            return getSaveDataState;
        }
        public bool getGetGameDataState()
        {
            System.Diagnostics.Debug.WriteLine("entered getGetGameDataState");
            return getGameDataState;
        }
        public bool getGetAcceptMatchState()
        {
            return getAcceptMatchState;
        }
        public bool getCreateUserState()
        {
            return createUserState;
        }
        public bool getInternetState()
        {
            return internetState;
        }
        public bool getWriteState()
        {
            return writeState;
        }
        public List<SaveData> getGetSaveData()
        {
            this.getSaveDataState = false;
            return saveData;
        }
        public GameData getGameDataReal()
        {
            return gameData;
        }
    }
}
