using Godot;
using System;

public partial class MainMenu : Control
{
	[Signal]
	public delegate void HostButtonPressedEventHandler();
	[Signal]
	public delegate void JoinButtonPressedEventHandler();

	private LineEdit portNode;
	private string playerName;
	private int port;
	private string address;


	public override void _Ready()
	{
		MultiplayerController main = GetParent<MultiplayerController>();
		address = main.Address;	
		(FindChild("AddressLineEdit") as LineEdit).Text = address;
		port = main.Port;
		(FindChild("PortLineEdit") as LineEdit).Text = port.ToString();
	}

	public void OnNameChange(string name)
	{
		playerName = name;
	}

	public void OnAddressChange(string address)
	{
		this.address = address;
	}

	public void OnPortChange(string port)
	{
		string pattern = @"[^\d]";
		RegEx regEx = new RegEx();
		regEx.Compile(pattern);

		if (regEx.Search(port) == null)
		{
			this.port = int.Parse(port);
		}
		else
		{
			portNode.DeleteCharAtCaret();
		}
	}

	public void OnHostButtonDown()
	{
		EmitSignal(SignalName.HostButtonPressed);
	}

	public void OnJoinButtonDown()
	{
		EmitSignal(SignalName.JoinButtonPressed);
	}
}
