using System.Collections.Generic;
using Yarn.Unity;

namespace AC
{ 

    public class ACYarnVariables : VariableStorageBehaviour
	{

		public override void SetValue (string variableName, string stringValue)
		{
			GVar variable = GetVariable (variableName);
			if (variable != null)
			{
				variable.TextValue = stringValue;
			}
		}


		public override void SetValue (string variableName, float floatValue)
		{
			GVar variable = GetVariable (variableName);
			if (variable != null)
			{
				variable.IntegerValue = (int) floatValue;
				variable.FloatValue = floatValue;
			}
		}


		public override void SetValue (string variableName, bool boolValue)
		{
			GVar variable = GetVariable (variableName);
			if (variable != null)
			{
				variable.BooleanValue = boolValue;
			}
		}
		
	 
	 	public override bool TryGetValue<T> (string variableName, out T result)
		{
			GVar variable = GetVariable (variableName);
			if (variable != null)
			{
				switch (variable.type)
				{
					case VariableType.Boolean:
						result = (T) (object) variable.BooleanValue;
						return true;

					case VariableType.Integer:
						result = (T) (object) variable.IntegerValue;
						return true;

					case VariableType.Float:
						result = (T) (object) variable.FloatValue;
						return true;

					case VariableType.String:
						result = (T) (object) variable.TextValue;
						return true;

					default:
						break;
				}
			}
			result = default (T);
			return false;
		}


		public override bool Contains (string variableName)
		{
			GVar variable = GetVariable (variableName);
			return variable != null;
		}


		public override void Clear () {}


		public override void SetAllVariables (Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
		{
			foreach (var key in floats.Keys)
			{
				SetValue (key, floats[key]);
			}

			foreach (var key in strings.Keys)
			{
				SetValue (key, strings[key]);
			}

			foreach (var key in bools.Keys)
			{
				SetValue (key, bools[key]);
			}
		}


		public override (Dictionary<string, float> FloatVariables, Dictionary<string, string> StringVariables,  Dictionary<string, bool> BoolVariables) GetAllVariables ()
		{
			var floats = new Dictionary<string, float> ();
			var strings = new Dictionary<string, string> ();
			var bools = new Dictionary<string, bool> ();

			return (floats, strings, bools);
		}


		private GVar GetVariable (string variableName)
		{
			string varName = variableName;
			if (varName.StartsWith ("$"))
			{
				varName = varName.Substring (1);
			}

			if (!varName.StartsWith ("l_"))
			{
				GVar variable = GlobalVariables.GetVariable (varName);
				if (variable != null)
				{
					return variable;
				}
			}

			if (!varName.StartsWith ("g_"))
			{
				GVar variable = LocalVariables.GetVariable (varName);
				if (variable != null)
				{
					return variable;
				}
			}

			ACDebug.LogWarning ("Cannot find AC Variable of the name " + varName);
			return null;
		}

	}
	
}