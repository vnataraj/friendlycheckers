import java.io.*;
import java.net.*;

public class Server
{
	private ServerSocket server;

	//runs a server on the given port
	public Server(int port)
	{
		try
		{
			server = new ServerSocket(port);
		}
		catch(IOException e)
		{
			throw new RuntimeException("port " + port + " already in use", e);
		}
	}

	//waits for a connection to the server, accepts it,
	//and returns a NetworkConnection object for talking with
	//the other party
	public NetworkConnection accept()
	{
		try
		{
			return new NetworkConnection(server.accept());
		}
		catch(IOException e)
		{
			throw new RuntimeException(e);
		}
	}

	//closes the server
	public void close()
	{
		try
		{
			server.close();
		}
		catch(IOException e)
		{
			throw new RuntimeException(e);
		}
	}
}