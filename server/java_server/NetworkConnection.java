import java.io.*;
import java.net.*;

public class NetworkConnection
{
	private Socket socket;
	private PrintWriter out;
	private BufferedReader in;

	//create a client-side network connection to an existing
	//host (ip address or machine name) running on a particular
	//port
	public NetworkConnection(String host, int port)
	{
		this(createSocket(host, port));
	}

	private static Socket createSocket(String host, int port)
	{
		try
		{
			return new Socket(host, port);
		}
		catch(UnknownHostException e)
		{
			throw new RuntimeException("cannot connect to " + host + ":" + port, e);
		}
		catch(IOException e)
		{
			throw new RuntimeException(e);
		}
	}

	//IGNORE.  INTERNAL USE ONLY.
	public NetworkConnection(Socket socket)
	{
		this.socket = socket;
		try
		{
			out = new PrintWriter(socket.getOutputStream(), true);
			in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
		}
		catch(IOException e)
		{
			throw new RuntimeException(e);
		}
	}

	//print (send) the given string across the network
	public void println(String s)
	{
		out.println(s);
	}

	//wait for the other party to send a string.
	//returns that string
	public String readLine()
	{
		try
		{
			return in.readLine();
		}
		catch(IOException e)
		{
			throw new RuntimeException("other party disconnected", e);
		}
	}

	//terminate the network connection
	public void close()
	{
		out.close();
		try
		{
			in.close();
			socket.close();
		}
		catch(IOException e)
		{
			throw new RuntimeException(e);
		}
	}
}