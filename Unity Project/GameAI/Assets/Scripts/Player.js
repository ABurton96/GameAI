#pragma strict

enum soundLevel {Nothing, Quite, Loud};

public var sound: soundLevel;
public var soundLevelAI: String;

function Update () 
{
	if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
	{
		if( Input.GetKey(KeyCode.LeftShift))
		{
			sound = soundLevel.Loud;
		}
		else
		{
			sound = soundLevel.Quite;
		}
	}
	else
	{
		sound = soundLevel.Nothing;
	}

	soundLevelAI = sound.ToString();
}
