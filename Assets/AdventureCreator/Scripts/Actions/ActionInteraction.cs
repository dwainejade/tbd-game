﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionInteraction.cs"
 * 
 *	This Action can enable and disable
 *	a Hotspot's individual Interactions.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionInteraction : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;
		protected Hotspot runtimeHotspot;

		public InteractionType interactionType;
		public ChangeType changeType = ChangeType.Enable;
		public int number = 0;

		private enum NumberRepresents { Index, ID };
		[SerializeField] private NumberRepresents numberRepresents = NumberRepresents.Index;
		
		public override ActionCategory Category { get { return ActionCategory.Hotspot; }}
		public override string Title { get { return "Change interaction"; }}
		public override string Description { get { return "Enables and disables individual Interactions on a Hotspot."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			runtimeHotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}

		
		public override float Run ()
		{
			if (runtimeHotspot == null)
			{
				return 0f;
			}

			int index = number;

			if (interactionType == InteractionType.Use)
			{
				if (parameterID >= 0 && numberRepresents == NumberRepresents.ID)
				{
					bool foundValue = false;
					for (int i = 0; i < runtimeHotspot.useButtons.Count; i++)
					{
						if (runtimeHotspot.useButtons[i].iconID == number)
						{
							index = i;
							foundValue = true;
							break;
						}
					}

					if (!foundValue)
					{
						LogWarning ("Cannot change Hotspot " + runtimeHotspot + "'s Use interaction ID " + number + " because it doesn't exist!");
						return 0f;
					}
				}

				if (runtimeHotspot.useButtons.Count > index)
				{
					ChangeButton (runtimeHotspot.useButtons [index]);
				}
				else
				{
					LogWarning ("Cannot change Hotspot " + runtimeHotspot + "'s Use interaction " + index + " because it doesn't exist!");
				}
			}
			else if (interactionType == InteractionType.Examine)
			{
				ChangeButton (runtimeHotspot.lookButton);
			}
			else if (interactionType == InteractionType.Inventory)
			{
				if (parameterID >= 0 && numberRepresents == NumberRepresents.ID)
				{
					bool foundValue = false;
					for (int i = 0; i < runtimeHotspot.invButtons.Count; i++)
					{
						if (runtimeHotspot.invButtons[i].invID == number)
						{
							index = i;
							foundValue = true;
							break;
						}
					}

					if (!foundValue)
					{
						LogWarning ("Cannot change Hotspot " + runtimeHotspot + "'s Use inventory ID " + number + " because it doesn't exist!");
						return 0f;
					}
				}

				if (runtimeHotspot.invButtons.Count > index)
				{
					ChangeButton (runtimeHotspot.invButtons [index]);
				}
				else
				{
					LogWarning ("Cannot change Hotspot " + runtimeHotspot + "'s Inventory interaction " + index + " because it doesn't exist!");
				}
			}
			runtimeHotspot.ResetMainIcon ();

			return 0f;
		}


		protected void ChangeButton (AC.Button button)
		{
			if (button == null)
			{
				return;
			}

			switch (changeType)
			{
				case ChangeType.Enable:
					runtimeHotspot.SetButtonState (button, true);
					break;

				case ChangeType.Disable:
					runtimeHotspot.SetButtonState (button, false);
					break;
			}
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			if (KickStarter.settingsManager)
			{
				ComponentField ("Hotspot to change:", ref hotspot, ref constantID, parameters, ref parameterID);

				interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction to change:", interactionType);

				if ((!isAssetFile && hotspot != null) || isAssetFile)
				{
					switch (interactionType)
					{
						case InteractionType.Use:
							if (hotspot == null || parameterID >= 0)
							{
								number = EditorGUILayout.IntField ("Use interaction:", number);

								if (parameterID >= 0)
									numberRepresents = (NumberRepresents) EditorGUILayout.EnumPopup ("Value represents:", numberRepresents);
							}
							else if (KickStarter.cursorManager)
							{
								// Multiple use interactions
								if (hotspot.useButtons.Count > 0 && hotspot.provideUseInteraction)
								{
									List<string> labelList = new List<string> ();

									foreach (AC.Button button in hotspot.useButtons)
									{
										labelList.Add (hotspot.useButtons.IndexOf (button) + ": " + KickStarter.cursorManager.GetLabelFromID (button.iconID, 0));
									}

									number = EditorGUILayout.Popup ("Use interaction:", number, labelList.ToArray ());

								}
								else
								{
									EditorGUILayout.HelpBox ("No 'Use' interactions defined!", MessageType.Info);
								}
							}
							else
							{
								EditorGUILayout.HelpBox ("A Cursor Manager is required.", MessageType.Warning);
							}
							break;

						case InteractionType.Examine:
							if (hotspot != null && !hotspot.provideLookInteraction)
							{
								EditorGUILayout.HelpBox ("No 'Examine' interaction defined!", MessageType.Info);
							}
							break;

						case InteractionType.Inventory:
							if (hotspot == null || parameterID >= 0)
							{
								number = EditorGUILayout.IntField ("Inventory interaction:", number);

								if (parameterID >= 0)
									numberRepresents = (NumberRepresents) EditorGUILayout.EnumPopup ("Value represents:", numberRepresents);
							}
							else if (KickStarter.inventoryManager)
							{
								if (hotspot.invButtons.Count > 0 && hotspot.provideInvInteraction)
								{
									List<string> labelList = new List<string> ();

									foreach (AC.Button button in hotspot.invButtons)
									{
										labelList.Add (hotspot.invButtons.IndexOf (button) + ": " + KickStarter.inventoryManager.GetLabel (button.invID));
									}

									number = EditorGUILayout.Popup ("Inventory interaction:", number, labelList.ToArray ());
								}
								else
								{
									EditorGUILayout.HelpBox ("No 'Inventory' interactions defined!", MessageType.Info);
								}
							}
							else
							{
								EditorGUILayout.HelpBox ("An Inventory Manager is required.", MessageType.Warning);
							}
							break;
					}
				}

				changeType = (ChangeType) EditorGUILayout.EnumPopup ("Change to make:", changeType);
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required for this Action.", MessageType.Warning);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberHotspot> (hotspot);
			}

			constantID = AssignConstantID<Hotspot> (hotspot, constantID, parameterID);
		}
		
		
		public override string SetLabel ()
		{
			if (hotspot != null)
			{
				return hotspot.name + " - " + changeType + " " + interactionType;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject _gameObject, int id)
		{
			if (parameterID < 0)
			{
				if (hotspot && hotspot.gameObject == _gameObject) return true;
				if (constantID == id) return true;
			}
			return base.ReferencesObjectOrID (_gameObject, id);
		}
		
		#endif


		/**
		 * <summary>Creates a new instance of the 'Hotspot: Change interaction' Action</summary>
		 * <param name = "hotspot">The Hotspot to affect</param>
		 * <param name = "changeType">What kind of change to make</param>
		 * <param name = "interactionType">The type of Hotspot interaction to affect</param>
		 * <param name = "interactionIndex">The index number of the interactions to affect</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionInteraction CreateNew (Hotspot hotspot, ChangeType changeType, InteractionType interactionType, int interactionIndex = 0)
		{
			ActionInteraction newAction = CreateNew<ActionInteraction> ();
			newAction.hotspot = hotspot;
			newAction.TryAssignConstantID (newAction.hotspot, ref newAction.constantID);
			newAction.interactionType = interactionType;
			newAction.changeType = changeType;
			newAction.number = interactionIndex;

			return newAction;
		}
		
	}

}