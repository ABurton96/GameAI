using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class NewEditorTest {

	[MenuItem("Open Scene/Menu")]
	public static void OpenMenu() {

		OpenScene("Menu");
	}

	[MenuItem("Open Scene/Test 1")]
	public static void OpenTest1() {
		
		OpenScene("Test 1");
	}

	[MenuItem("Open Scene/Test 2")]
	public static void OpenTest2() {

		OpenScene("Test 2");
	}

	[MenuItem("Open Scene/Test 3")]
	public static void OpenTest3() {

		OpenScene("Test 3");
	}

	public static void OpenScene(string name)
	{
		if(EditorApplication.SaveCurrentSceneIfUserWantsTo())
		{
			EditorApplication.OpenScene("Assets/Scenes/" + name + ".unity");
		}
	}
}
