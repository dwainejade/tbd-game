﻿/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"EventManager.cs"
 * 
 *	This script handles events that are run at certain times during a game.
 *	They can be subscribed to by custom script, to aid with third-party integration.
 * 
 */

using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Handles events that are run at certain times during a game.
 	 * They can be subscribed to by custom script, to aid with third-party integration.
	 */
	public class EventManager : MonoBehaviour
	{

		private void OnEnable ()
		{
			#if UNITY_2019_4_OR_NEWER
			if (KickStarter.settingsManager)
			{
				foreach (EventBase _event in KickStarter.settingsManager.events)
				{
					if (_event == null) continue;
					_event.Register ();
				}
			}
			#endif
		}


		private void OnDisable ()
		{
			#if UNITY_2019_4_OR_NEWER
			if (KickStarter.settingsManager)
			{
				foreach (EventBase _event in KickStarter.settingsManager.events)
				{
					if (_event == null) continue;
					_event.Unregister ();
				}
			}
			#endif
		}


		#region Speech

		/** A delegate for the OnStartSpeech and OnEndSpeechScroll events */
		public delegate void Delegate_StartSpeech (AC.Char speakingCharacter, string speechText, int lineID);
		/** A delegate for the OnStopSpeech event */
		public delegate void Delegate_StopSpeech (AC.Char speakingCharacter);
		/** A delegate for the OnStartSpeech_Alt, OnStopSpeech_Alt, OnStartSpeechScroll_Alt, OnEndSpeechScroll_Alt and OnCompleteSpeechScroll_Alt events */
		public delegate void Delegate_Speech (Speech speech);
		/** A delegate for the OnSpeechToken event */
		public delegate void Delegate_SpeechToken (AC.Char speakingCharacter, int lineID, string tokenKey, string tokenValue);
		/** A delegate for the OnSpeechToken_Alt event */
		public delegate void Delegate_SpeechTokenAlt (Speech speech, string tokenKey, string tokenValue);
		/** A delegate for the OnReqeustSpeechTokenReplacement event */
		public delegate string Delegate_OnRequestSpeechTokenReplacement (Speech speech, string tokenKey, string tokenValue);
		/** A delegate for the OnRequestTextTokenReplacement event */
		public delegate string Delegate_OnRequestTextTokenReplacement (string tokenKey, string tokenValue);
		/** A delegate for the OnLoadSpeechAssetBundle event */
		public delegate void Delegate_OnLoadSpeechAssetBundle (int language);
		/** A delegate for the OnSkipSpeech event */
		public delegate void Delegate_OnSkipSpeech (Speech speech, bool justCompletingScroll);
		/** An event triggered whenever a new line of dialogue begins */
		public static event Delegate_StartSpeech OnStartSpeech;
		/** An event triggered whenever a new line of dialogue begins */
		public static event Delegate_Speech OnStartSpeech_Alt;
		/** An event triggered whenever a line of dialogue ends */
		public static event Delegate_StopSpeech OnStopSpeech;
		/** An event triggered whenever a line of dialogue ends */
		public static event Delegate_Speech OnStopSpeech_Alt;
		/** An event triggered whenever a line of dialogue starts scrolling */
		public static event Delegate_StartSpeech OnStartSpeechScroll;
		/** An event triggered whenever a line of dialogue starts scrolling */
		public static event Delegate_Speech OnStartSpeechScroll_Alt;
		/** An event triggered whenever a line of dialogue stops scrolling */
		public static event Delegate_StartSpeech OnEndSpeechScroll;
		/** An event triggered whenever a line of dialogue stops scrolling */
		public static event Delegate_Speech OnEndSpeechScroll_Alt;
		/** An event triggred whenever a line of dialogue has completed scrolling */
		public static event Delegate_StartSpeech OnCompleteSpeechScroll;
		/** An event triggred whenever a line of dialogue has completed scrolling */
		public static event Delegate_Speech OnCompleteSpeechScroll_Alt;
		/** An event triggered whenever a line of dialogue uses a speech token */
		public static event Delegate_SpeechToken OnSpeechToken;
		/** An event triggered whenever a line of dialogue uses a speech token */
		public static event Delegate_SpeechTokenAlt OnSpeechToken_Alt;
		/** An event triggered whenever a speech token is used in speech text, and it's replacement is requested (it will be removed otherwise) */
		public static event Delegate_OnRequestSpeechTokenReplacement OnRequestSpeechTokenReplacement;
		/** An event triggered whenever a custom text token is used in text, and it's replacement is requested (it will be removed otherwise) */
		public static event Delegate_OnRequestTextTokenReplacement OnRequestTextTokenReplacement;
		/** An event triggered whenever an audio or lipsync asset bundle has completed loading */
		public static event Delegate_OnLoadSpeechAssetBundle OnLoadSpeechAssetBundle;
		/** An event triggered when speech is skipped due to input */
		public static event Delegate_OnSkipSpeech OnSkipSpeech;


		/**
		 * <summary>Triggers the OnStartSpeech and OnStartSpeech_Alt events.</summary>
		 * <param name = "speech">The Speech class instance that has begun</param>
		 * <param name = "speakingCharacter">The character who is speaking. If null, the line is considered to be a narration</param>
		 * <param name = "speechText">The dialogue text</param>
		 * <param name = "lineID">The ID number of the speech line, as generated by the SpeechManager</param>
		 */
		public void Call_OnStartSpeech (Speech speech, AC.Char speakingCharacter, string speechText, int lineID)
		{
			if (OnStartSpeech != null)
			{
				OnStartSpeech (speakingCharacter, speechText, lineID);
			}
			if (OnStartSpeech_Alt != null)
			{
				OnStartSpeech_Alt (speech);
			}
		}


		/**
		 * <summary>Triggers the OnSkipSpeech event.</summary>
		 * <param name = "speech">The Speech class instance being skipped</param>
		 * <param name = "justCompletingScroll">If True, then the Speech class has not ended - and the skipping input merely ended scrolling</param>
		 */
		public void Call_OnSkipSpeech (Speech speech, bool justCompletingScroll)
		{
			if (OnSkipSpeech != null)
			{
				OnSkipSpeech (speech, justCompletingScroll);
			}
		}


		/**
		 * <summary>Triggers the OnStopSpeech and OnStopSpeech events.</summary>
		 * <param name = "speech">The Speech class instance that has ended</param>
		 * <param name = "speakingCharacter">The character who is speaking. If null, the line is considered to be a narration</param>
		 */
		public void Call_OnStopSpeech (Speech speech, AC.Char speakingCharacter)
		{
			if (OnStopSpeech != null)
			{
				OnStopSpeech (speakingCharacter);
			}
			if (OnStopSpeech_Alt != null)
			{
				OnStopSpeech_Alt (speech);
			}
		}


		/** 
		 * <summary>Triggers the OnStartSpeechScroll and OnStartSpeechScroll_Alt events.</summary>
		 * <param name = "speech">The Speech class instance that has started scrolling</param>
		 * <param name = "speakingCharacter">The character who is speaking. If null, the line is considered to be a narration</param>
		 * <param name = "speechText">The dialogue text</param>
		 * <param name = "lineID">The ID number of the speech line, as generated by the SpeechManager</param>
		 */
		public void Call_OnStartSpeechScroll (Speech speech, AC.Char speakingCharacter, string speechText, int lineID)
		{
			if (OnStartSpeechScroll != null)
			{
				OnStartSpeechScroll (speakingCharacter, speechText, lineID);
			}
			if (OnStartSpeechScroll_Alt != null)
			{
				OnStartSpeechScroll_Alt (speech);
			}
		}


		/**
		 * <summary>Triggers the OnEndSpeechScroll and OnEndSpeechSroll_Alt events.</summary>
		 * <param name = "speech">The Speech class instance that has stopped scrolling</param>
		 * <param name = "speakingCharacter">The character who is speaking. If null, the line is considered to be a narration</param>
		 * <param name = "speechText">The dialogue text</param>
		 * <param name = "lineID">The ID number of the speech line, as generated by the SpeechManager</param>
		 */
		public void Call_OnEndSpeechScroll (Speech speech, AC.Char speakingCharacter, string speechText, int lineID)
		{
			if (OnEndSpeechScroll != null)
			{
				OnEndSpeechScroll (speakingCharacter, speechText, lineID);
			}
			if (OnEndSpeechScroll_Alt != null)
			{
				OnEndSpeechScroll_Alt (speech);
			}
		}


		/**
		 * <summary>Triggers the OnCompleteSpeechScroll and OnCompleteSpeechScroll_Alt events.</summary>
		 * <param name = "speech">The Speech class instance that has completed scrolling</param>
		 * <param name = "speakingCharacter">The character who is speaking. If null, the line is considered to be a narration</param>
		 * <param name = "speechText">The dialogue text</param>
		 * <param name = "lineID">The ID number of the speech line, as generated by the SpeechManager</param>
		 */
		public void Call_OnCompleteSpeechScroll (Speech speech, AC.Char speakingCharacter, string speechText, int lineID)
		{
			if (OnCompleteSpeechScroll != null)
			{
				OnCompleteSpeechScroll (speakingCharacter, speechText, lineID);
			}
			if (OnCompleteSpeechScroll_Alt != null)
			{
				OnCompleteSpeechScroll_Alt (speech);
			}
		}


		/**
		 * <summary>Triggers the OnSpeechToken event.</summary>
		 * <param name = "speech">The Speech class instance that contains the token text</param>
		 * <param name = "tokenKey">The token text to the left of the colon, i.e 'var'</param>
		 * <param name = "tokenKey">The token text to the right of the colon, i.e '2'</param>
		 */
		public void Call_OnSpeechToken (Speech speech, string tokenKey, string tokenValue)
		{
			if (OnSpeechToken != null)
			{
				OnSpeechToken (speech.speaker, speech.log.lineID, tokenKey, tokenValue);
			}
			if (OnSpeechToken_Alt != null)
			{
				OnSpeechToken_Alt (speech, tokenKey, tokenValue);
			}
		}


		/**
		 * <summary>Triggers the OnRequstSpeechTokenReplacement event.</summary>
		 * <param name = "speech">The Speech class instance that contains the token text</param>
		 * <param name = "tokenKey">The token text to the left of the colon, i.e 'var'</param>
		 * <param name = "tokenKey">The token text to the right of the colon, i.e '2'</param>
		 * <returns>A string to replace the token text with. This can be empty, which will just remove the token</returns>
		 */
		public string Call_OnRequestSpeechTokenReplacement (Speech speech, string tokenKey, string tokenValue)
		{
			if (OnRequestSpeechTokenReplacement != null)
			{
				return OnRequestSpeechTokenReplacement (speech, tokenKey, tokenValue);
			}
			return string.Empty;
		}


		/**
		 * <summary>Triggers the OnRequstTextTokenReplacement event.</summary>
		 * <param name = "tokenKey">The token text to the left of the colon, i.e 'var'</param>
		 * <param name = "tokenKey">The token text to the right of the colon, i.e '2'</param>
		 * <returns>A string to replace the token text with. This can be empty, which will just remove the token</returns>
		 */
		public string Call_OnRequestTextTokenReplacement (string tokenKey, string tokenValue)
		{
			if (OnRequestTextTokenReplacement != null)
			{
				return OnRequestTextTokenReplacement (tokenKey, tokenValue);
			}
			return string.Empty;
		}


		/**
		 * <summary>Triggers the OnLoadSpeechAssetBundle event.</summary>
		 * <param name = "language">The language index of the asset bundle that was loaded</param>
		 */
		public void Call_OnLoadSpeechAssetBundle (int language)
		{
			if (OnLoadSpeechAssetBundle != null)
			{
				OnLoadSpeechAssetBundle (language);
			}
		}

		#endregion


		#region GameState

		/** A delegate for the OnEnterGameState and OnExitGameState events */
		public delegate void Delegate_ChangeGameState (GameState gameState);
		/** An event triggered whenever a GameState is entered */
		public static event Delegate_ChangeGameState OnEnterGameState;
		/** An event triggered whenever a GameState is exited */
		public static event Delegate_ChangeGameState OnExitGameState;

		/**
		 * <summary>Triggers the OnEnterGameState and OnExitGameState events.</summary>
		 * <param name = "oldGameState">The previous GameState (Normal, Cutscene, DialogOptions, Paused)</param>
		 */
		public void Call_OnChangeGameState (GameState oldGameState, GameState newGameState)
		{
			if (OnExitGameState != null)
			{
				OnExitGameState (oldGameState);
			}
			if (OnEnterGameState != null)
			{
				OnEnterGameState (newGameState);
			}
		}

		#endregion


		#region Conversations

		/** A delegate for the OnStartConversation event */
		public delegate void Delegate_Conversation (Conversation conversation);
		/** A delegate for the OnClickConversation event */
		public delegate void Delegate_ConversationChoice (Conversation conversation, int optionID);
		/** An event triggered whenever a Conversation begins */
		public static event Delegate_Conversation OnStartConversation;
		/** An event triggered whenever a Conversation option is chosen */
		public static event Delegate_ConversationChoice OnClickConversation;
		/** An event triggered whenever a Conversation, and its dialogue option ActionLists, has ended. Note that this does not trigger when options are overridden. */
		public static event Delegate_Conversation OnEndConversation;


		/**
		 * <summary>Triggers the OnStartConversation event.</summary>
		 * <param name = "conversation">The Conversation that was started</param>
		 */
		public void Call_OnStartConversation (Conversation conversation)
		{
			if (OnStartConversation != null && conversation != null)
			{
				OnStartConversation (conversation);
			}
		}


		/**
		 * <summary>Triggers the OnEndConversation event.</summary>
		 * <param name = "conversation">The Conversation that was ended</param>
		 */
		public void Call_OnEndConversation (Conversation conversation)
		{
			if (OnEndConversation != null && conversation != null)
			{
				OnEndConversation (conversation);
			}
		}


		/**
		 * <summary>Triggers the OnClickConversation event.</summary>
		 * <param name = "conversation">The Conversation that was interacted with</param>
		 * <param name = "optionID">The ID number of the conversation's clicked ButtonDialog</param>
		 */
		public void Call_OnClickConversation (Conversation conversation, int optionID)
		{
			if (OnClickConversation != null)
			{
				OnClickConversation (conversation, optionID);
			}
		}

		#endregion


		#region Hotspots

		/** A delegate for the OnHotspotSelect and OnHotspotDeselect events */
		public delegate void Delegate_ChangeHotspot (Hotspot hotspot);
		/** A delegate for the OnHotspotInteract event */
		public delegate void Delegate_InteractHotspot (Hotspot hotspot, AC.Button button);
		/** A delegate for the OnHotspotSetInteractionState event */
		public delegate void Delegate_OnHotspotSetInteractionState (Hotspot hotspot, AC.Button button, bool newState);
		/** A delegate for the OnModifyHotspotDetectorCollection event */
		public delegate List<Hotspot> Delegate_HotspotCollection (DetectHotspots hotspotDetector, List<Hotspot> hotspots);
		/** An event triggered whenever a Hotspot is selected */
		public static Delegate_ChangeHotspot OnHotspotSelect;
		/** An event triggered whenever a Hotspot is de-selected */
		public static Delegate_ChangeHotspot OnHotspotDeselect;
		/** An event triggered whenever a Hotspot's button is interacted with */
		public static Delegate_InteractHotspot OnHotspotInteract;
		/** An event triggered whenever a Hotspot is double-clicked */
		public static Delegate_InteractHotspot OnDoubleClickHotspot;
		/** An event triggered whenever a Hotspot is turned on */
		public static Delegate_ChangeHotspot OnHotspotTurnOn;
		/** An event triggered whenever a Hotspot is turned off */
		public static Delegate_ChangeHotspot OnHotspotTurnOff;
		/** An event triggered when the act of the Player moving to a given Hotspot in order to interact with it is cancelled */
		public static Delegate_ChangeHotspot OnHotspotStopMovingTo;
		/** An event triggered when the Player has finished moving and turning to a given Hotspot prior to interacting with it */
		public static Delegate_InteractHotspot OnHotspotReach;
		/** An event triggered whenever a DetectHotspots script modifies its internal collection of nearby Hotspots */
		public static Delegate_HotspotCollection OnModifyHotspotDetectorCollection;
		/** An event triggered whenever a Hotspot is registered to the StateHandler - typically by enabling its GameObject */
		public static Delegate_ChangeHotspot OnRegisterHotspot;
		/** An event triggered whenever a Hotspot is unregistered from the StateHandler - typically by disabling its GameObject */
		public static Delegate_ChangeHotspot OnUnregisterHotspot;
		/** An event triggered whenever a Hotspot button's enabled state is changed */
		public static Delegate_OnHotspotSetInteractionState OnHotspotSetInteractionState;
		/** An event triggered whenever the FlashHotspots input is invoked to flash all Hotspots in the scene*/
		public static Delegate_Generic OnHotspotsFlash;
		/** An event triggered when Highlight components are to be updated */
		public static Delegate_Generic OnUpdateHighlights;


		/**
		 * <summary>Triggers either the OnHotspotSelect or OnHotspotDeselect event.</summary>
		 * <param name = "hotspot">The Hotspot that was affected</param>
		 * <param name = "wasSelected">If True, the OnHotspotSelect event will be triggered. If False, the OnHotspotDeselect Event will be triggered.</param>
		 */
		public void Call_OnChangeHotspot (Hotspot hotspot, bool wasSelected)
		{
			if (hotspot == null) return;

			if (wasSelected && OnHotspotSelect != null)
			{
				OnHotspotSelect (hotspot);
			}
			else if (!wasSelected && OnHotspotDeselect != null)
			{
				OnHotspotDeselect (hotspot);
			}
		}


		/**
		 * <summary>Triggers the OnHotspotInteract event.</summary>
		 * <param name = "hotspot">The Hotspot the was interacted with</param>
		 * <param name = "button">The specific Button on the Hotspot that was interacted with. This will be null if the interaction is unhandled.</param>
		 */
		public void Call_OnInteractHotspot (Hotspot hotspot, AC.Button button)
		{
			if (hotspot == null) return;

			if (OnHotspotInteract != null)
			{
				OnHotspotInteract (hotspot, button);
			}
		}


		/**
		 * <summary>Triggers the OnDoubleClickHotspot event, regardless of the Hotspot's doubleClickingHotspot variable.</summary>
		 * <param name = "hotspot">The Hotspot that was double-clicked.</param>
		 */
		public void Call_OnDoubleClickHotspot (Hotspot hotspot, AC.Button button)
		{
			if (hotspot == null) return;

			if (OnDoubleClickHotspot != null)
			{
				OnDoubleClickHotspot (hotspot, button);
			}
		}


		/**
		 * <summary>Triggers either the OnHotspotTurnOn or OnHotspotTurnOff event</summary>
		 * <param name = "hotspot">The affected Hotspot</param>
		 * <param name = "isOn">If True, OnHotspotTurnOn will be triggered. Otherwise, OnHotspotTurnOff will be triggered</param>
		 */
		public void Call_OnTurnHotspot (Hotspot hotspot, bool isOn)
		{
			if (hotspot == null) return;

			if (isOn)
			{
				if (OnHotspotTurnOn != null)
				{
					OnHotspotTurnOn (hotspot);
				}
			}
			else
			{
				if (OnHotspotTurnOff != null)
				{
					OnHotspotTurnOff (hotspot);
				}
			}
		}


		/**
		 * <summary>Triggers the OnHotspotSetButtonState  event</summary>
		 * <param name = "hotspot">The affected Hotspot</param>
		 * <param name = "button">The Button that was updated</param>
		 * <param name = "isOn">If True, Button was enabled, otherwise it was disabled</param>
		 */
		public void Call_OnHotspotSetInteractionState(Hotspot hotspot, Button button, bool isOn)
		{
			if (hotspot == null || button == null) return;

			if (OnHotspotSetInteractionState != null)
			{
				OnHotspotSetInteractionState(hotspot, button, isOn);
			}
		}


		/**
		 * <summary>Triggers the OnHotspotStopMovingTo event</summary>
		 * <param name = "hotspot">The Hotspot that the Player is moving towards</param>
		 */
		public void Call_OnHotspotStopMovingTo (Hotspot hotspot)
		{
			if (hotspot == null) return;

			if (OnHotspotStopMovingTo != null)
			{
				OnHotspotStopMovingTo (hotspot);
			}
		}


		/**
		 * <summary>Triggers the OnHotspotReach event</summary>
		 * <param name = "hotspot">The Hotspot that the Player has reached</param>
		 * <param name = "button">The specific Button on the Hotspot that was interacted with. This will be null if the interaction is unhandled.</param>
		 */
		public void Call_OnHotspotReach (Hotspot hotspot, AC.Button button)
		{
			if (hotspot == null) return;

			if (OnHotspotReach != null)
			{
				OnHotspotReach (hotspot, button);
			}
		}


		/**
		 * <summary>Triggers the OnModifyHotspotDetectorCollection event</summary>
		 * <param name = "hotspotDetector">The DetectHotspots component that is modifying its own collection of Hotspots</param>
		 * <param name = "hotspots">The List of Hotspot components that the hotspot detector has modified</param>
		 * <returns>The list of Hotspot components gathered by the hotspot detector. This list can be modified to control which Hotspots are interactive, and in what order</returns>
		 */
		public List<Hotspot> Call_OnModifyHotspotDetectorCollection (DetectHotspots hotspotDetector, List<Hotspot> hotspots)
		{
			if (hotspots == null) return null;

			if (OnModifyHotspotDetectorCollection != null)
			{
				return OnModifyHotspotDetectorCollection (hotspotDetector, hotspots);
			}
			return hotspots;
		}


		/**
		 * <summary>Triggers the OnRegisterHotspot or OnUnregisterHotspot event</summary>
		 * <param name = "hotspots">The Hotspot that was registered or unregistered from the StateHandler</param>
		 * <param name = "wasRegistered">If True, the Hotspot was register and OnRegisterHotspot will be triggered.  Otherwise, the Hotspot was unregistered and OnUnregisterHotspot will be triggered</param>
		 */
		public void Call_OnRegisterHotspot (Hotspot hotspot, bool wasRegistered)
		{
			if (wasRegistered)
			{
				if (OnRegisterHotspot != null)
				{
					OnRegisterHotspot (hotspot);
				}
			}
			else
			{
				if (OnUnregisterHotspot != null)
				{
					OnUnregisterHotspot (hotspot);
				}
			}
		}


		/** Triggers the OnHotspotsFlash event */
		public void Call_OnHotspotsFlash ()
		{
			if (OnHotspotsFlash != null)
			{
				OnHotspotsFlash ();
			}
		}

		#endregion


		#region Triggers

		/** A delegate for the OnRunTrigger event */
		public delegate void Delegate_OnRunTrigger (AC_Trigger trigger, GameObject collidingObject);
		/** An event triggered whenever a Trigger is run */
		public static Delegate_OnRunTrigger OnRunTrigger;


		/**
		 * <summary>Triggers the OnRunTrigger event.</summary>
		 * <param name = "trigger">The Trigger that was run</param>
		 * <param name = "collidingObject">The GameObject that collided with the Trigger</param>
		 */
		public void Call_OnRunTrigger (AC_Trigger trigger, GameObject collidingObject)
		{
			if (trigger == null) return;

			if (OnRunTrigger != null)
			{
				OnRunTrigger (trigger, collidingObject);
			}
		}


		/** Triggers the OnUpdateHighlights event */
		public void Call_OnUpdateHighlights ()
		{
			if (OnUpdateHighlights != null)
			{
				OnUpdateHighlights ();
			}
		}
		
		#endregion


		#region Misc

		/** A delegate for the OnTeleport event */
		public delegate void Delegate_OnTeleport (GameObject gameObject);
		/** An event triggered when an object is teleported using the 'Object: Teleport' Action */
		public static Delegate_OnTeleport OnTeleport;


		/**
		 * <summary>Triggers the OnTeleport event</summary>
		 * <param name="_object">The object that was teleported</param>
		 */
		public void Call_OnTeleport (GameObject _object)
		{
			if (_object == null) return;

			if (OnTeleport != null)
			{
				OnTeleport (_object);
			}
		}


		#endregion


		#region Variables

		/** A delegate for the OnVariableChange event */
		public delegate void Delegate_OnVariableChange (GVar variable);
		/** A delegate for the OnVariableUpload and OnVariableDownload events */
		public delegate void Delegate_OnVariableUpload (GVar variable, Variables variables);
		/** An event triggered whenever a Variable is changed via an Action */
		public static Delegate_OnVariableChange OnVariableChange;
		/** An event triggered whenever a Variable's value is to be uploaded to a custom script it is linked to */
		public static Delegate_OnVariableUpload OnUploadVariable;
		/** An event triggered whenever a Variable's value is to be downloaded from a custom script it is linked to */
		public static Delegate_OnVariableUpload OnDownloadVariable;
		/** A delegate for the OnTimerStart, OnTimerUpdate and OnTimerComplete events */
		public delegate void Delegate_Timer (Timer variableTimer);
		/** An event triggered when a Timer starts */
		public static Delegate_Timer OnTimerStart;
		/** An event triggered when a Timer updates */
		public static Delegate_Timer OnTimerUpdate;
		/** An event triggered when a Timer completes */
		public static Delegate_Timer OnTimerComplete;


		/**
		 * <summary>Triggers the OnVariableChange event.</summary>
		 * <param name = "_variable">The variable that was changed</param>
		 */
		public void Call_OnVariableChange (GVar _variable)
		{
			if (OnVariableChange != null)
			{
				OnVariableChange (_variable);
			}
		}


		/**
		 * <summary>Triggers the OnDownloadVariable event.</summary>
		 * <param name = "_variable">The variable to download</param>
		 * <param name = "variables">The Variables component it is from, if a component variable</param>
		 */
		public void Call_OnDownloadVariable (GVar _variable, Variables variables = null)
		{
			if (OnDownloadVariable != null)
			{
				OnDownloadVariable (_variable, variables);
			}
		}


		/**
		 * <summary>Triggers the OnUploadVariable event.</summary>
		 * <param name = "_variable">The variable to upload</param>
		 * <param name = "variables">The Variables component it is from, if a component variable</param>
		 */
		public void Call_OnUploadVariable (GVar _variable, Variables variables = null)
		{
			if (OnUploadVariable != null)
			{
				OnUploadVariable (_variable, variables);
			}
		}


		/**
		 * <summary>Triggers the OnTimerStart event.</summary>
		 * <param name = "timer">The Timer that was started</param>
		 */
		public void Call_OnTimerStart (Timer timer)
		{
			if (OnTimerStart != null)
			{
				OnTimerStart (timer);
			}
		}


		/**
		 * <summary>Triggers the OnTimerUpdate event.</summary>
		 * <param name = "timer">The Timer that was updated</param>
		 */
		public void Call_OnTimerUpdate (Timer timer)
		{
			if (OnTimerUpdate != null)
			{
				OnTimerUpdate (timer);
			}
		}


		/**
		 * <summary>Triggers the OnTimerComplete event.</summary>
		 * <param name = "timer">The Timer that was completed</param>
		 */
		public void Call_OnTimerComplete (Timer timer)
		{
			if (OnTimerComplete != null)
			{
				OnTimerComplete (timer);
			}
		}

		#endregion


		#region Menus

		/** A delegate for the OnMenuElementClick event */
		public delegate void Delegate_OnMenuElementClick (AC.Menu _menu, MenuElement _element, int _slot, int buttonPressed);
		/** A delegate for the OnMouseOverMenu event */
		public delegate void Delegate_OnMouseOverMenu (AC.Menu _menu, MenuElement _element, int _slot);
		/** A delegate for the OnMenuElementShow and OnMenuElementHide events */
		public delegate void Delegate_OnMenuElementVisiblity (MenuElement _element);
		/** A delegate for the OnMenuElementShift event */
		public delegate void Delegate_OnMenuElementShift (MenuElement _element, AC_ShiftInventory shiftType);
		/** A delegate for the OnMenuTurnOn and OnMenuTurnOff events */
		public delegate void Delegate_OnMenuTurnOn (AC.Menu _menu, bool isInstant);
		/** A delegate for the OnUpdateDragLine event */
		public delegate void Delegate_OnUpdateDragLine (Vector2 startScreenPosition, Vector2 endScreenPosition);
		/** A delegate for the OnEnableInteractionMenus event */
		public delegate void Delegate_OnEnableInteractionMenus (Hotspot hotspot, InvItem invItem);
		/** A delegate for the OnJournalPageAdd and OnJournalPageRemove events */
		public delegate void Delegate_OnModifyJournalPage (MenuJournal journal, JournalPage page, int index);
		/** A delegate for the Delegate_OnRequestMenuElementHotspotLabel event */
		public delegate string Delegate_OnRequestMenuElementHotspotLabel (AC.Menu _menu, MenuElement _element, int _slot, int _language);
		/** A delegate for the OnRequestInventoryCountText event */
		public delegate string Delegate_OnRequestInventoryCountText (InvInstance invInstance, bool isSelectedCursor);
		/** A delegate for the OnHideSelectedElement event */
		public delegate void Delegate_OnHideSelectedElement (AC.Menu _menu, MenuElement _element, int _slot);

		/** An event triggered whenever a MenuElement inside a Menu is clicked */
		public static Delegate_OnMenuElementClick OnMenuElementClick;
		/** An event triggered whenever the mouse hovers over a new menu element */
		public static Delegate_OnMouseOverMenu OnMouseOverMenu;
		/** An event triggered whenever a menu element is made visible */
		public static Delegate_OnMenuElementVisiblity OnMenuElementShow;
		/** An event triggered whenever a menu element is made invisible */
		public static Delegate_OnMenuElementVisiblity OnMenuElementHide;
		/** An event triggered whenever a menu element's slots are shifted */
		public static Delegate_OnMenuElementShift OnMenuElementShift;
		/** An event triggered once the Menus have been generated when the game begins */
		public static Delegate_Generic OnGenerateMenus;
		/** An event triggered whenever a menu is turned on */
		public static Delegate_OnMenuTurnOn OnMenuTurnOn;
		/** An event triggered whenever a menu is turned off */
		public static Delegate_OnMenuTurnOn OnMenuTurnOff;
		/** An event triggered every frame if the Player is drag-controlled */
		public static Delegate_OnUpdateDragLine OnUpdateDragLine;
		/** An event triggered whenever Interaction menus are enabled for a Hotspot or InvItem */
		public static Delegate_OnEnableInteractionMenus OnEnableInteractionMenus;
		/** An event triggered whenever a Journal element has a new page added to it */
		public static Delegate_OnModifyJournalPage OnJournalPageAdd;
		/** An event triggered whenever a Journal element has a page removed to it */
		public static Delegate_OnModifyJournalPage OnJournalPageRemove;
		/** An event triggered whenever the Hotspot label for a menu element is requested */
		public static Delegate_OnRequestMenuElementHotspotLabel OnRequestMenuElementHotspotLabel;
		/** An event triggered whenever the Inventory "Count" label for an Inventory item instance is requested */
		public static Delegate_OnRequestInventoryCountText OnRequestInventoryCountText;
		/** An event triggered whenever the currently-selected UI GameObject is hidden */
		public static Delegate_OnHideSelectedElement OnHideSelectedElement;


		/**
		 * <summary>Triggers the OnMenuElementClick event.</summary>
		 * <param name = "menu">The Menu that the clicked MenuElement is a part of</param>
		 * <param name = "element">The MenuElement that was clicked on</param>
		 * <param name = "slot">The slot index that was clicked, if the MenuElement consists of multiple slots (0 otherwise)</param>
		 * <param name = "buttonPressed">Equals 1 if a left-click, or 2 if a right-click</param>
		 */
		public void Call_OnMenuElementClick (AC.Menu menu, MenuElement element, int slot, int buttonPressed)
		{
			if (OnMenuElementClick != null)
			{
				OnMenuElementClick (menu, element, slot, buttonPressed);
			}
		}


		/**
		 * <summary>Triggers the OnMouseOverMenuElement event.</summary>
		 * <param name = "menu">The Menu that the mouse is over</param>
		 * <param name = "element">The MenuElement that the mouse is over</param>
		 * <param name = "slot">The slot index that the mouse is over, if the MenuElement consists of multiple slots (0 otherwise)</param>
		 */
		public void Call_OnMouseOverMenuElement (AC.Menu menu, MenuElement element, int slot)
		{
			if (OnMouseOverMenu != null)
			{
				OnMouseOverMenu (menu, element, slot);
			}
		}


		/**
		 * <summary>Triggers either the OnMenuElementShow or OnMenuElementHide events, depending on the state of the element's isVisible property.</summary>
		 * <param name = "element">The MenuElement whose visibility has changed</param>
		 */
		public void Call_OnMenuElementChangeVisibility (MenuElement element)
		{
			if (element.IsVisible)
			{
				if (OnMenuElementShow != null)
				{
					OnMenuElementShow (element);
				}
			}
			else
			{
				if (OnMenuElementHide != null)
				{
					OnMenuElementHide (element);
				}
			}
		}


		/**
		 * <summary>Triggers the OnMenuElementShift</summary>
		 * <param name = "element">The MenuElement whose slots have been shifted</param>
		 * <param name = "shiftType">The direction in which the slots where shifted (ShiftLeft, ShiftRight)</param>
		 */
		public void Call_OnMenuElementShift (MenuElement element, AC_ShiftInventory shiftType)
		{
			if (OnMenuElementShift != null)
			{
				OnMenuElementShift (element, shiftType);
			}
		}


		/** Triggers the OnGenerateMenus event. */
		public void Call_OnGenerateMenus ()
		{
			if (OnGenerateMenus != null)
			{
				OnGenerateMenus ();
			}
		}


		/**
		 * <summary>Triggers the OnMenuTurnOn event.</summary>
		 * <param name = "menu">The Menu that is being turned on</param>
		 * <param name = "isInstant">If true, the transition is being skipped and the Menu is being turned on instantly</param>
		 */
		public void Call_OnMenuTurnOn (AC.Menu menu, bool isInstant)
		{
			if (OnMenuTurnOn != null)
			{
				OnMenuTurnOn (menu, isInstant);
			}
		}


		/**
		 * <summary>Triggers the OnMenuTurnOff event.</summary>
		 * <param name = "menu">The Menu that is being turned off</param>
		 * <param name = "isInstant">If true, the transition is being skipped and the Menu is being turned off instantly</param>
		 */
		public void Call_OnMenuTurnOff (AC.Menu menu, bool isInstant)
		{
			if (OnMenuTurnOff != null)
			{
				OnMenuTurnOff (menu, isInstant);
			}
		}


		/**
		 * <summary>Updates the co-ordinates of the on-screen drag line if Touch Screen input is used, or the Movement method is Drag.</summary?
		 * <param name = "startScreenPosition">The starting position, in screen co-ordinates, of the drag line.  If no dragging is occuring, this will be equal to Vector2.zero</param>
		 * <param name = "endScreenPosition">The ending position, in screen co-ordinates, of the drag line.  If no dragging is occuring, this will be equal to Vector2.zero</param>
		 */
		public void Call_OnUpdateDragLine (Vector2 startScreenPosition, Vector2 endScreenPosition)
		{
			if (OnUpdateDragLine != null)
			{
				OnUpdateDragLine (startScreenPosition, endScreenPosition);
			}
		}


		/**
		 * <summary>Triggers the OnEnableInteractionMenus event</summary>
		 * <param name = "hotspot">The Hotspot for which Interaction menus were turned on for. Null if invItem is not.</param>
		 * <param name = "invInstance">The Inventory item instance for which Interaction menus were turned on for.  Null if hotspot is not.</param>
		 */
		public void Call_OnEnableInteractionMenus (Hotspot hotspot, InvInstance invInstance)
		{
			if (OnEnableInteractionMenus != null)
			{
				OnEnableInteractionMenus (hotspot, (InvInstance.IsValid (invInstance)) ? invInstance.InvItem : null);
			}
		}


		/**
		 * <summary>Triggers either the OnJournalPageAdd or OnJournalPageRemove event</summary>
		 * <param name = "journal">The MenuJournal element that was modifiyed</param>
		 * <param name = "page">The JournalPage class that was modified</param>
		 * <param name = "index">The page index that was modified</param>
		 * <param name = "wasAdded">If True, the page was added.  If False, the page was removed</param>
		 */
		public void Call_OnModifyJournalPage (MenuJournal journal, JournalPage page, int index, bool wasAdded)
		{
			if (wasAdded)
			{
				if (OnJournalPageAdd != null)
				{
					OnJournalPageAdd (journal, page, index);
				}
			}
			else
			{
				if (OnJournalPageRemove != null)
				{
					OnJournalPageRemove (journal, page, index);
				}
			}
		}


		/**
		 * <summary>Triggers the OnRequestMenuElementHotspotLabel event</summary>
		 * <param name = "_menu">The Menu containing the element</param>
		 * <param name = "_element">The element being requested</param>
		 * <param name = "_slot">The element's slot index number</param>
		 * <param name = "language">The current language's index number, where 0 = the game's original language</param>
		 * <returns>A string to display in a 'Hotspot' label element.  If empty, default text from the menu element will be used</returns>
		 */
		public string Call_OnRequestMenuElementHotspotLabel (AC.Menu _menu, MenuElement _element, int _slot, int language)
		{
			if (OnRequestMenuElementHotspotLabel != null)
			{
				return OnRequestMenuElementHotspotLabel (_menu, _element, _slot, language);
			}
			return string.Empty;
		}


		/**
		 * <summary>Triggers the OnRequestInventoryCountText event</summary>
		 * <param name = "invInstance">The item instance to get the "count" text for</param>
		 * <param name = "isSelectedCursor">True if the item is currently selected and this is for the active cursor</param>
		 * <returns>The custom "count" text for the item instance, or empty for the default value</returns>
		 */
		public string Call_OnRequestInventoryCountText (InvInstance invInstance, bool isSelectedCursor)
		{
			if (OnRequestInventoryCountText != null)
			{
				return OnRequestInventoryCountText (invInstance, isSelectedCursor);
			}
			return string.Empty;
		}


		/**
		 * <summary>Triggers the OnHideSelectedElement event.</summary>
		 * <param name = "menu">The Menu that the hidden MenuElement is a part of</param>
		 * <param name = "element">The MenuElement that was hidden</param>
		 * <param name = "slot">The slot index that was hidden, if the MenuElement consists of multiple slots (0 otherwise)</param>
		 */
		public void Call_OnHideSelectedElement (AC.Menu menu, MenuElement element, int slot)
		{
			if (OnHideSelectedElement != null)
			{
				OnHideSelectedElement (menu, element, slot);
			}
		}

		#endregion


		#region Cursor

		/** A delegate for the OnChangeCursorMode event */
		public delegate void Delegate_OnChangeCursorMode (int cursorID);
		/** A delegate for the OnSetHardwareCursor event */
		public delegate void Delegate_OnSetHardwareCursor (Texture2D cursorTexture, Vector2 clickOffset);
		/** A delegate for the OnCursorLock event */
		public delegate void Delegate_OnCursorLock (bool isLocked);

		/** An event triggered when the active cursor mode is changed */
		public static Delegate_OnChangeCursorMode OnChangeCursorMode;
		/** An event triggered when the Hardware cursor is changed */
		public static Delegate_OnSetHardwareCursor OnSetHardwareCursor;
		/** An event triggered when the cursor's locked state is set */
		public static event Delegate_OnCursorLock OnCursorLock;


		/**
		 * <summary>Triggers the OnChangeCursorMode.</summary>
		 * <param name = "cursorID">The ID value of the new cursor.  For values >= 0, the value corresponds to the ID number of the interaction cursor listed in the Cursor Manager.  If -1, this is the "main" cursor.  If -2, this is the "inventory" cursor.</param>
		 */
		public void Call_OnChangeCursorMode (int cursorID)
		{
			if (OnChangeCursorMode != null)
			{
				OnChangeCursorMode (cursorID);
			}
		}


		/**
		 * <summary>Triggers the OnSetCursor event.</summary>
		 * <param name = "cursorTexture">The Texture2D that the Hardware cursor was set to</param>
		 * <param name = "clickOffset">The offset from the top-left to use as the target point</param> 
		 */
		public void Call_OnSetHardwareCursor (Texture2D cursorTexture, Vector2 clickOffset)
		{
			if (OnSetHardwareCursor != null)
			{
				OnSetHardwareCursor (cursorTexture, clickOffset);
			}
		}


		/**
		 * <summary>Triggers the OnCursorLock event</summary>
		 * <param name = "isLocked">True if the cursor has been locked, False if unlocked</param>
		 */
		public void Call_OnCursorLock (bool isLocked)
		{
			if (OnCursorLock != null)
			{
				OnCursorLock (isLocked);
			}
		}

		#endregion


		#region Saving

		/** A delegate for events that take no arguments and return null */
		public delegate void Delegate_Generic ();
		/** A delegate for the OnBeforeLoading and OnFinishSaving events */
		public delegate void Delegate_SaveFile (SaveFile saveFile);
		/** A delegate for the OnBeforeSaving, OnFailSaving and OnFailLoading events */
		public delegate void Delegate_SaveID (int saveID);
		/** A delegate for the OnSwitchProfile event */
		public delegate void Delegate_OnSwitchProfile (int profileID);
		/** A delegate for the OnGatherSaves event */
		public delegate void Delegate_OnGatherSaves (ref List<SaveFile> foundSaveFiles);
		/** An event triggered before a save game file is created */
		public static Delegate_SaveID OnBeforeSaving;
		/** An event triggered after a save game file is created */
		public static Delegate_SaveFile OnFinishSaving;
		/** An event triggered after an attempt to save a game fails */
		public static Delegate_SaveID OnFailSaving;
		/** An event triggered before a save game file is loaded */
		public static Delegate_SaveFile OnBeforeLoading;
		/** An event triggered after a save game file is loaded */
		public static Delegate_SaveID OnFinishLoading;
		/** An event triggered after an attempt to load a game fails */
		public static Delegate_SaveID OnFailLoading;
		/** An event triggered before the variables in a save game file are imported */
		public static Delegate_Generic OnBeforeImporting;
		/** An event triggered after the variables in a save game file are imported */
		public static Delegate_Generic OnFinishImporting;
		/** An event triggered after an attempt to import a game fails */
		public static Delegate_Generic OnFailImporting;
		/** An event triggered after switching profile */
		public static Delegate_OnSwitchProfile OnSwitchProfile;
		/** An event triggered when restarting the game */
		public static Delegate_Generic OnRestartGame;
		/** An event triggered as a separate thread is about to be used to save the game */
		public static Delegate_SaveFile OnPrepareSaveThread;
		/** An event triggered when save files are gathered */
		public static Delegate_OnGatherSaves OnGatherSaves;


		/**
		 * <summary>Triggers either the OnBeforeSaving, OnFinishSaving or OnFailSaving events.</summary>
		 * <param name = "fileAccessState">The state of the file access (Before, After, Fail)</param>
		 * <param name = "saveID">The ID of the slot being saved</param>
		 * <param name = "saveFile">The save file being loaded</param>
		 */
		public void Call_OnSave (FileAccessState fileAccessState, int saveID, SaveFile saveFile = null)
		{
			if (fileAccessState == FileAccessState.Before && OnBeforeSaving != null)
			{
				OnBeforeSaving (saveID);
			}
			else if (fileAccessState == FileAccessState.After && OnFinishSaving != null)
			{
				OnFinishSaving (saveFile);
			}
			else if (fileAccessState == FileAccessState.Fail && OnFailSaving != null)
			{
				OnFailSaving (saveID);
			}
		}

		/**
		 * <summary>Triggers either the OnBeforeLoading, OnFinishLoading or OnFailLoading events.</summary>
		 * <param name = "fileAccessState">The state of the file access (Before, After, Fail)</param>
		 * <param name = "saveID">The ID of the slot being loaded</param>
		 * <param name = "saveFile">The save file being save</param>
		 */
		public void Call_OnLoad (FileAccessState fileAccessState, int saveID, SaveFile saveFile = null)
		{
			if (fileAccessState == FileAccessState.Before && OnBeforeLoading != null)
			{
				OnBeforeLoading (saveFile);
			}
			else if (fileAccessState == FileAccessState.After && OnFinishLoading != null)
			{
				OnFinishLoading (saveID);
			}
			else if (fileAccessState == FileAccessState.Fail && OnFailLoading != null)
			{
				OnFailLoading (saveID);
			}
		}

		/**
		 * <summary>Triggers either the OnBeforeImporting, OnFinishImporting or OnFailImporting events.</summary>
		 * <param name = "fileAccessState">The state of the file access (Before, After, Fail)</param>
		 */
		public void Call_OnImport (FileAccessState fileAccessState)
		{
			if (fileAccessState == FileAccessState.Before && OnBeforeImporting != null)
			{
				OnBeforeImporting ();
			}
			else if (fileAccessState == FileAccessState.After && OnFinishImporting != null)
			{
				OnFinishImporting ();
			}
			else if (fileAccessState == FileAccessState.Fail && OnFailImporting != null)
			{
				OnFailImporting ();
			}
		}

		/**
		 * <summary>Triggers the OnSwitchProfile event</summary>
		 * <param name="profileID">The ID number of the profile that was switched to</param>
		 */
		public void Call_OnSwitchProfile (int profileID)
		{
			if (OnSwitchProfile != null)
			{
				OnSwitchProfile (profileID);
			}
		}

		/** Triggers the OnRestartGame event. */
		public void Call_OnRestartGame ()
		{
			if (OnRestartGame != null)
			{
				OnRestartGame ();
			}
		}


		/** 
		 * <summary>Triggers the OnPrepareSaveThread event</summary>
		 * <param name = "saveFile">The SaveFile being written to</param>
		 */
		public void Call_OnPrepareSaveThread (SaveFile saveFile)
		{
			if (OnPrepareSaveThread != null)
			{
				OnPrepareSaveThread (saveFile);
			}
		}


		/** 
		 * <summary>Triggers the OnGatherSaves event</summary>
		 * <param name = "foundSaveFiles">The SaveFiles that have been gathered</param>
		 */
		public void Call_OnGatherSaves (ref List<SaveFile> foundSaveFiles)
		{
			if (OnGatherSaves != null)
			{
				OnGatherSaves (ref foundSaveFiles);
			}
		}

		#endregion


		#region Characters

		/** A delegate for the OnSetPlayer, OnPlayerSpawn, and OnPlayerRemove events */
		public delegate void Delegate_Player (Player player);
		/** An event triggered whenever a new Player is loaded into the scene */
		public static Delegate_Player OnSetPlayer;
		/** An event triggered after a Player is spawned in the scene */
		public static Delegate_Player OnPlayerSpawn;
		/** An event triggered before a Player is removed the scene */
		public static Delegate_Player OnPlayerRemove;
		/** An event triggered whenever the Player jumps */
		public static Delegate_Player OnPlayerJump;

		/** A delegate for the OnCharacterEnterTimeline and OnCharacterExitTimeline events */
		public delegate void Delegate_OnCharacterTimeline (AC.Char character, PlayableDirector director, int trackIndex);
		/** An event triggered whenever a character is about to be controlled by a Timeline */
		public static Delegate_OnCharacterTimeline OnCharacterEnterTimeline;
		/** An event triggered whenevr a character is no longer controlled by a Timeline */
		public static Delegate_OnCharacterTimeline OnCharacterExitTimeline;

		/** A delegate for the OnCharacterEndPath event */
		public delegate void Delegate_OnCharacterEndPath (AC.Char character, Paths path);
		/** An event triggered whenever a character's path is ended */
		public static Delegate_OnCharacterEndPath OnCharacterEndPath;
		/** A delegate for the OnCharacterSetPath event */
		public delegate void Delegate_OnCharacterSetPath (AC.Char character, Paths path);
		/** An event triggered whenever a character's path is set */
		public static Delegate_OnCharacterSetPath OnCharacterSetPath;
		/** A delegate for the OnCharacterReachNode event */
		public delegate void Delegate_OnCharacterReachNode (AC.Char character, Paths path, int node);
		/** An event triggered whenever a character reaches a node along a path */
		public static Delegate_OnCharacterReachNode OnCharacterReachNode;
		/** A delegate for the OnCharacterRecalculatePathfind event */
		public delegate void Delegate_OnCharacterRecalculatePathfind (AC.Char character, ref Vector3 targetPosition);
		/** An event triggered whenever a character's active pathfinding is recalculated */
		public static Delegate_OnCharacterRecalculatePathfind OnCharacterRecalculatePathfind;

		/** A delegate for the OnSetHeadTurnTarget event */
		public delegate void Delegate_SetHeadTurnTarget (AC.Char character, Transform headTurnTarget, Vector3 targetOffset, bool isInstant);
		/** An event triggered whenever a character's head is given a target to look at */
		public static Delegate_SetHeadTurnTarget OnSetHeadTurnTarget;

		/** A delegate for the OnClearHeadTurnTarget event */
		public delegate void Delegate_ClearHeadTurnTarget (AC.Char character, bool isInstant);
		/** An event triggered whenever a character stops looking at a target with their head */
		public static Delegate_ClearHeadTurnTarget OnClearHeadTurnTarget;
		/** A delegate for the OnOccupyPlayerStart event */
		public delegate void Delegate_OnOccupyPlayerStart (Player player, PlayerStart playerStart);
		/** An event triggered whenever the player snaps to a PlayerStart */
		public static Delegate_OnOccupyPlayerStart OnOccupyPlayerStart;

		/** A delegate for the OnPointClick event */
		public delegate void Delegate_OnPointAndClick (ref Vector3[] pointArray, bool run);
		/** An event triggered whenever the player is commanded to move via point-and-click */
		public static Delegate_OnPointAndClick OnPointAndClick;

		/** A delegate for the OnSetLookDirection event */
		public delegate void Delegate_OnSetLookDirection (AC.Char character, Vector3 direction, bool isInstant);
		/** An event triggered whenever a character updates their facing direction */
		public static Delegate_OnSetLookDirection OnSetLookDirection;

		/** A delegate for the OnCharacterSetExpression event */
		public delegate void Delegate_OnCharacterSetExpression (AC.Char character, Expression expression);
		/** An event triggered whenever a character's expression is updated */
		public static Delegate_OnCharacterSetExpression OnCharacterSetExpression;

		/** A delegate for the OnCharacterTeleport event */
		public delegate void Delegate_OnCharacterTeleport (AC.Char character, Vector3 position, Quaternion rotation);
		/** An event triggered whenever a character is teleported */
		public static Delegate_OnCharacterTeleport OnCharacterTeleport;

		/** A delegate for the OnCharacerHoldObject / OnCharacerDropObject events */
		public delegate void Delegate_OnCharacterHoldObject (AC.Char character, GameObject heldObject, int attachmentPointID);
		/** An event triggered whenever a character holds an object */
		public static Delegate_OnCharacterHoldObject OnCharacterHoldObject;
		/** An event triggered whenever a character drops an object */
		public static Delegate_OnCharacterHoldObject OnCharacterDropObject;


		/** 
		 * <summary>Triggers the OnSetPlayer event.</summary>
		 * <param name = "player">The new Player object</param>
		 */
		public void Call_OnSetPlayer (Player player)
		{
			if (player == null) return;

			if (OnSetPlayer != null)
			{
				OnSetPlayer (player);
			}
		}


		/**
		 * <summary>Triggers the OnPlayerSpawn event.</summary>
		 * <param name="player">The Player being spawned</param>
		 */
		public void Call_OnPlayerSpawn (Player player)
		{
			if (player == null) return;

			if (OnPlayerSpawn != null)
			{
				OnPlayerSpawn (player);
			}
		}


		/**
		 * <summary>Triggers the OnPlayerRemove event.</summary>
		 * <param name="player">The Player being removed</param>
		 */
		public void Call_OnPlayerRemove (Player player)
		{
			if (player == null) return;

			if (OnPlayerRemove != null)
			{
				OnPlayerRemove (player);
			}
		}


		/**
		 * <summary>Triggers the OnPlayerJump event. This event will also be fired if a jump attempt is made, but the Player does not have the correct Rigidbody component to do so</summary>
		 * <param name="player">The Player jumping</param>
		 */
		public void Call_OnPlayerJump (Player player)
		{
			if (OnPlayerJump != null)
			{
				OnPlayerJump (player);
			}
		}


		/** 
		 * <summary>Calls either the OnCharacterEnterTimeline or OnCharacterExitTimeline events</summary>
		 * <param name = "character">The character on the Timeline</param>
		 * <param name = "director">The PlayableDirector that is playing the Timeline</param>
		 * <param name = "trackIndex">The index number of the track within the director's TimelineAsset that the character appears on</param>
		 * <param name = "isEntering">If True, OnCharacterEnterTimeline will be called.  Otherwise, OnChracterExitTimeline will be called</param>
		 */
		public void Call_OnCharacterTimeline (AC.Char character, PlayableDirector director, int trackIndex, bool isEntering)
		{
			if (character != null)
			{
				if (isEntering)
				{
					if (OnCharacterEnterTimeline != null)
					{
						OnCharacterEnterTimeline (character, director, trackIndex);
					}
				}
				else
				{
					if (OnCharacterExitTimeline != null)
					{
						OnCharacterExitTimeline (character, director, trackIndex);
					}
				}
			}
		}


		/**
		 * <summary>Triggers the OnSetHeadTurnTarget event.</summary>
		 * <param name = "character">The character who is turning their head.</param>
		 * <param name = "headTurnTarget">The Transform to look at</param>
		 * <param name = "targetOffset">An offset in world-space to look at, relative to the headTurnTarget transform</param>
		 * <param name = "isInstant">If True, the head-turn snaps instantly</param>
		 */
		public void Call_OnSetHeadTurnTarget (AC.Char character, Transform headTurnTarget, Vector3 targetOffset, bool isInstant)
		{
			if (OnSetHeadTurnTarget != null)
			{
				OnSetHeadTurnTarget (character, headTurnTarget, targetOffset, isInstant);
			}
		}


		/**
		 * <summary>Triggers the OnClearHeadTurnTarget event.</summary>
		 * <param name = "character">The character who is no longer turning their head</param>
		 * <param name = "isInstant">If True, the head stops turning instantly</param>
		 */
		public void Call_OnClearHeadTurnTarget (AC.Char character, bool isInstant)
		{
			if (OnClearHeadTurnTarget != null)
			{
				OnClearHeadTurnTarget (character, isInstant);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterEndPath event.</summary>
		 * <param name = "character">The character whose path has ended</param>
		 * <param name = "path">The Paths component that the character was following.  If the character was pathfinding, this will be their own Paths component</param>
		 */
		public void Call_OnCharacterEndPath (AC.Char character, Paths path)
		{
			if (OnCharacterEndPath != null)
			{
				OnCharacterEndPath (character, path);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterSetPath event.</summary>
		 * <param name = "character">The character whose path has been set</param>
		 * <param name = "path">The Paths component that the character is following.  If the character is pathfinding, this will be their own Paths component</param>
		 */
		public void Call_OnCharacterSetPath (AC.Char character, Paths path)
		{
			if (OnCharacterSetPath != null)
			{
				OnCharacterSetPath (character, path);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterReachNode event</summary>
		 * <param name = "character">The character who has reached a node</param>
		 * <param name = "path">The Paths component that the character is following.  If the character is pathfinding, this will be their own Paths component</param>
		 * <param name = "node">The index number of the paths's List of nodes that has been reached</param>
		 */
		public void Call_OnCharacterReachNode (AC.Char character, Paths path, int node)
		{
			if (OnCharacterReachNode != null)
			{
				OnCharacterReachNode (character, path, node);
			}
		}


		/**
		 * <summary>Triggers the OnOccupyPlayerStart event.</summary>
		 * <param name = "player">The Player that was affected</param>
		 * <param name = "playerStart">The PlayerStart that the Player has been set to occupy</param>
		 */
		public void Call_OnOccupyPlayerStart (Player player, PlayerStart playerStart)
		{
			if (OnOccupyPlayerStart != null && player != null)
			{
				OnOccupyPlayerStart (player, playerStart);
			}
		}


		/**
		 * <summary>Triggers the OnPointAndClick event.</summary>
		 * <param name = "pointArray">An array of points for the Player to move along</param>
		 * <param name = "run">If True, the Player should run along the points</param>
		 */
		public void Call_OnPointAndClick (ref Vector3[] pointArray, bool run)
		{
			if (OnPointAndClick != null)
			{
				OnPointAndClick (ref pointArray, run);
			}
		}


		/**
		 * <summary>Triggers the OnSetLookDirection event</summary>
		 * <param name="character">The character that is turning</param>
		 * <param name="direction">The character's intended facing direction</param>
		 * <param name="isInstant">If True, the character will turn instantly to face this new direction</param>
		 */
		public void Call_OnSetLookDirection (AC.Char character, Vector3 direction, bool isInstant)
		{
			if (OnSetLookDirection != null)
			{
				OnSetLookDirection (character, direction, isInstant);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterSetExpression event</summary>
		 * <param name="character">The affected character</param>
		 * <param name="expression">The character's expression. This will be null if the character's expression is reset</param>
		 */
		public void Call_OnCharacterSetExpression (AC.Char character, Expression expression)
		{
			if (OnCharacterSetExpression != null)
			{
				OnCharacterSetExpression.Invoke (character, expression);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterTeleport event.</summary>
		 * <param name="character">The character being teleported</param>
		 * <param name="position">The new position</param>
		 * <param name="rotation">The new rotation</param>
		 */
		public void Call_OnCharacterTeleport (AC.Char character, Vector3 position, Quaternion rotation)
		{
			if (OnCharacterTeleport != null)
			{
				OnCharacterTeleport (character, position, rotation);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterRecalculatePathfind event</summary>
		 * <param name="character">The character being affected</param>
		 * <param name="destination">The character's destination.  This can be modified.</param>
		 */
		public void Call_OnCharacterRecalculatePathfind (AC.Char character, ref Vector3 destination)
		{
			if (OnCharacterRecalculatePathfind != null)
			{
				OnCharacterRecalculatePathfind (character, ref destination);
			}
		}


		/**
		 * <summary>Triggers the OnCharacterHoldObject event</summary>
		 * <param name="character">The character being affected</param>
		 * <param name="heldObject">The held object</param>
		 * <param name="attachmentPointID">The ID of the attachment point the object is attached to</param>
		 */
		public void Call_OnCharacterHoldObject (AC.Char character, GameObject heldObject, int attachmentPointID)
		{
			if (OnCharacterHoldObject != null)
			{
				OnCharacterHoldObject (character, heldObject, attachmentPointID);
			}
		}


		/**
		 * <summary>Triggers the OnCharacerDropObject event</summary>
		 * <param name="character">The character being affected</param>
		 * <param name="heldObject">The dropped object</param>
		 * <param name="attachmentPointID">The ID of the attachment point the object is attached to</param>
		 */
		public void Call_OnCharacterDropObject (AC.Char character, GameObject heldObject, int attachmentPointID)
		{
			if (OnCharacterDropObject != null)
			{
				OnCharacterDropObject (character, heldObject, attachmentPointID);
			}
		}

		#endregion


		#region Inventory

		/** A delegate for the OnInventoryAdd, OnInventoryRemove and OnInventoryInteract events */
		public delegate void Delegate_ChangeInventory (InvItem invItem, int amount);
		/** A delegate for the OnInventoryAdd_Alt and OnInventoryRemove_Alt events */
		public delegate void Delegate_ChangeInventory_Alt (InvCollection invCollection, InvInstance invInstance, int amount);
		/** A delegate for the OnInventoryCombine events */
		public delegate void Delegate_CombineInventory (InvItem invItem, InvItem invItem2);
		/** A delegate for the OnInventoryCombine_Alt events */
		public delegate void Delegate_CombineInventory_Alt (InvInstance invInstanceA, InvInstance invInstanceB);
		/** A delegate for the OnInventoryInteract_Alt events */
		public delegate void Delegate_InteractInventory_Alt (InvInstance invInstance, int iconID);
		/** A delegate for the OnInventorySelect and OnInventoryDeselect events */
		public delegate void Delegate_Inventory (InvItem invItem);
		/** A delegate for the OnInventorySelect_Alt and OnInventoryDeselect_Alt events */
		public delegate void Delegate_Inventory_Alt (InvCollection invCollection, InvInstance invInstance);
		/** A delegate for the OnContainerAdd and OnContainerRemove events */
		public delegate void Delegate_ContainerItem (Container container, InvInstance containerItem);
		/** A delegate for the OnContainerOpen and OnContainerClose events */
		public delegate void Delegate_Container (Container container);
		/** A delegate for the OnInventoryHighlight event */
		public delegate void Delegate_InventoryHighlight (InvItem invItem, HighlightType highlightType);
		/** A delegate for the OnInventoryHighlight_Alt event */
		public delegate void Delegate_InventoryHighlight_Alt (InvInstance invInstance, HighlightType highlightType);
		/** A delegate for the OnCraftingSucceed event */
		public delegate void Delegate_Crafting (Recipe recipe, InvInstance invInstance);
		/** An event triggered whenever an item is added to the player's inventory */
		public static Delegate_ChangeInventory OnInventoryAdd;
		/** An event triggered whenever an item is added to the player's inventory */
		public static Delegate_ChangeInventory_Alt OnInventoryAdd_Alt;
		/** An event triggered whenever an item is removed from the player's inventory */
		public static Delegate_ChangeInventory OnInventoryRemove;
		/** An event triggered whenever an item is removed from the player's inventory */
		public static Delegate_ChangeInventory_Alt OnInventoryRemove_Alt;
		/** An event triggered whenever an inventory item is selected by the player */
		public static Delegate_Inventory OnInventorySelect;
		/** An event triggered whenever an inventory item is selected by the player */
		public static Delegate_Inventory_Alt OnInventorySelect_Alt;
		/** An event triggered whenever an inventory item is de-selected by the player */
		public static Delegate_Inventory OnInventoryDeselect;
		/** An event triggered whenever an inventory item is de-selected by the player */
		public static Delegate_Inventory_Alt OnInventoryDeselect_Alt;
		/** An event triggered whenever an item in the Player's Inventory is hovered over (will be null when un-hovered) */
		public static Delegate_Inventory_Alt OnInventoryHover;
		/** An event triggered whenever an inventory item is interacted with */
		public static Delegate_ChangeInventory OnInventoryInteract;
		/** An event triggered whenever an inventory item is interacted with */
		public static Delegate_InteractInventory_Alt OnInventoryInteract_Alt;
		/** An event triggered whenever two inventory items are combined together. This is triggered even if the item is "used" with itself */
		public static Delegate_CombineInventory OnInventoryCombine;
		/** An event triggered whenever two inventory items are combined together. This is triggered even if the item is "used" with itself */
		public static Delegate_CombineInventory_Alt OnInventoryCombine_Alt;
		/** An event triggered whenever an item is added to a Container */
		public static Delegate_ContainerItem OnContainerAdd;
		/** An event triggered whenever an item is removed from a Container */
		public static Delegate_ContainerItem OnContainerRemove;
		/** An event triggered whenever an item cannot be removed from a Container */
		public static Delegate_ContainerItem OnContainerRemoveFail;
		/** An event triggered when a Container is opened */
		public static Delegate_Container OnContainerOpen;
		/** An event triggered when a Container is closed */
		public static Delegate_Container OnContainerClose;
		/** An event triggered whenever a recipe has been succesfully created */
		public static Delegate_Crafting OnCraftingSucceed;
		/** An event triggered whenever an item is highlighted using the "Object: Highlight" Action */
		public static Delegate_InventoryHighlight OnInventoryHighlight;
		/** An event triggered whenever an item is highlighted using the "Object: Highlight" Action */
		public static Delegate_InventoryHighlight_Alt OnInventoryHighlight_Alt;
		/** A delegate for the OnInventorySpawn event*/
		public delegate void Delegate_OnInventorySpawn (InvInstance invInstance, SceneItem sceneItem);
		/** An event triggered whenever an inventory item is spawned in the scene */
		public static Delegate_OnInventorySpawn OnInventorySpawn;


		/**
		 * <summary>Triggers either the OnInventoryAdd, OnInventoryRemove, OnInventorySelect or OnInventoryDeselect events.<summary>
		 * <param name = "invCollection">The collection of items that was affected</param>
		 * <param name = "invInstance">The instance of the inventory item that was manipulated</param>
		 * <param name = "inventoryEventType">How the inventory item was manipulated (Add, Remove, Select, Deselect)</param>
		 * <param name = "amountOverride">If non-negative, how many instances of the inventory item were affected, if not that used in InvInstance</param>
		 */
		public void Call_OnChangeInventory (InvCollection invCollection, InvInstance invInstance, InventoryEventType inventoryEventType, int amountOverride = -1)
		{
			if (invInstance == null) return;

			if (invCollection == null) invCollection = invInstance.GetSource ();
			
			bool isPlayerInventory = (invCollection != null) ? invCollection.IsPlayerInventory () : false;
			Container container = (isPlayerInventory || invCollection == null) ? null : invCollection.GetSourceContainer ();
			
			switch (inventoryEventType)
			{
				case InventoryEventType.Add:
					if (isPlayerInventory)
					{
						if (OnInventoryAdd != null)
						{
							OnInventoryAdd (invInstance.InvItem, (amountOverride < 0) ? invInstance.Count : amountOverride);
						}
					}
					else if (container)
					{
						if (OnContainerAdd != null)
						{
							OnContainerAdd (container, invInstance);
						}
					}
					if (OnInventoryAdd_Alt != null && invCollection != null)
					{
						OnInventoryAdd_Alt (invCollection, invInstance, (amountOverride < 0) ? invInstance.Count : amountOverride);
					}
					
					break;

				case InventoryEventType.Remove:
					if (isPlayerInventory)
					{
						if (OnInventoryRemove != null)
						{
							OnInventoryRemove (invInstance.InvItem, (amountOverride < 0) ? invInstance.Count : amountOverride);
						}
					}
					else if (container)
					{
						if (OnContainerRemove != null)
						{
							OnContainerRemove (container, invInstance);
						}
					}
					if (OnInventoryRemove_Alt != null && invCollection != null)
					{
						OnInventoryRemove_Alt (invCollection, invInstance, (amountOverride < 0) ? invInstance.Count : amountOverride);
					}
					break;

				case InventoryEventType.Select:
					if (OnInventorySelect != null)
					{
						OnInventorySelect (invInstance.InvItem);
					}
					if (OnInventorySelect_Alt != null && invCollection != null)
					{
						OnInventorySelect_Alt (invCollection, invInstance);
					}
					break;

				case InventoryEventType.Deselect:
					if (OnInventoryDeselect != null)
					{
						OnInventoryDeselect (invInstance.InvItem);
					}
					if (OnInventoryDeselect_Alt != null)
					{
						OnInventoryDeselect_Alt (invCollection, invInstance);
					}
					break;

				default:
					break;
			}
		}


		/**
		 * <summary>Triggers the OnInventoryHover event.</summary>
		 * <param name = "invCollection">The Player's InvCollection</param>
		 * <param name = "invInstance">The instance of the inventory item that was hovered over (will be null if the item is no longer hovered over</param>
		 */
		public void Call_OnInventoryHover (InvCollection invCollection, InvInstance invInstance)
		{
			if (OnInventoryHover != null)
			{
				OnInventoryHover (invCollection, invInstance);
			}
		}


		/**
		 * <summary>Triggers the InventoryInteract and OnInventoryInteract_Alt events.</summary>
		 * <param name = "invInstance">The instance of the inventory item that was manipulated</param>
		 * <param name = "iconID">The ID number of the 'use' icon, as defined in CursorManager, if the item was used</param>
		 */
		public void Call_OnUseInventory (InvInstance invInstance, int iconID)
		{
			if (!InvInstance.IsValid (invInstance)) return;

			if (OnInventoryInteract != null)
			{
				OnInventoryInteract (invInstance.InvItem, iconID);
			}
			if (OnInventoryInteract_Alt != null)
			{
				OnInventoryInteract_Alt (invInstance, iconID);
			}
		}


		/**
		 * <summary>Triggers the OnInventoryCombine and OnInventoryCombine_Alt events.</summary>
		 * <param name = "invInstanceA">The first inventory item instance</param>
		 * <param name = "invInstanceB">The second inventory item instance</param>
		 */
		public void Call_OnCombineInventory (InvInstance invInstanceA, InvInstance invInstanceB)
		{
			if (!InvInstance.IsValid (invInstanceA) || !InvInstance.IsValid (invInstanceB)) return;

			if (OnInventoryCombine != null)
			{
				OnInventoryCombine (invInstanceA.InvItem, invInstanceB.InvItem);
			}
			if (OnInventoryCombine_Alt != null)
			{
				OnInventoryCombine_Alt (invInstanceA, invInstanceB);
			}
		}


		/**
		 * <summary>Triggers the OnContainerRemoveFail event.<summary>
		 * <param name = "container">The Container being manipulated</param>
		 * <param name = "invInstance">The inventory item instance that could not be removed from the Container</param>
		 */
		public void Call_OnUseContainerFail (Container container, InvInstance invInstance)
		{
			if (!InvInstance.IsValid (invInstance) || container == null) return;

			if (OnContainerRemoveFail != null)
			{
				OnContainerRemoveFail (container, invInstance);
			}
		}


		/**
		 * <summary>Triggers either the OnContainerOpen or OnContainerClose event.<summary>
		 * <param name = "container">The Container being manipulated</param>
		 * <param name = "wasOpened">If True, OnContainerOpen will be run. Otherwise, OnContainerClose</param>
		 */
		public void Call_OnContainerOpenClose (Container container, bool wasOpened)
		{
			if (container == null) return;

			if (wasOpened)
			{
				if (OnContainerOpen != null)
				{
					OnContainerOpen (container);
				}
			}
			else
			{
				if (OnContainerClose != null)
				{
					OnContainerClose (container);
				}
			}
		}


		/**
		 * <summary>Triggers the OnCraftingSucceed event.</summary>
		 * <param name = "recipe">The Recipe that was completed</param>
		 * <param name = "resultingInvInstance">The instance of the resulting inventory item</param>
		 */
		public void Call_OnCraftingSucceed (Recipe recipe, InvInstance resultingInvInstance)
		{
			if (OnCraftingSucceed != null)
			{
				OnCraftingSucceed (recipe, resultingInvInstance);
			}
		}


		/**
		 * <summary>Triggers the OnInventoryHiglight and OnInventoryHighlight_Alt event</summary>
		 * <param name = "invInstance">The instance of the item being highlight</param>
		 * <param name = "highlightType">The highlighting effect being applied</param>
		 */
		public void Call_OnInventoryHighlight (InvInstance invInstance, HighlightType highlightType)
		{
			if (!InvInstance.IsValid (invInstance)) return;

			if (OnInventoryHighlight != null)
			{
				OnInventoryHighlight (invInstance.InvItem, highlightType);
			}
			if (OnInventoryHighlight_Alt != null)
			{
				OnInventoryHighlight_Alt (invInstance, highlightType);
			}
		}


		/**
		 * <summary>Triggers the OnInventorySpawn event</summary>
		 * <param name = "invInstance">The instance of the item that was spawned.  If the spawned object was detached from its source, this will not be the same as the spawned SceneItem's LinkedInvInstance.</param>
		 * <param name = "sceneItem">The SceneItem component attached to the spawned item's Linked Prefab</param>
		 */
		public void Call_OnInventorySpawn (InvInstance invInstance, SceneItem sceneItem)
		{
			if (OnInventorySpawn != null)
			{
				OnInventorySpawn (invInstance, sceneItem);
			}
		}

		#endregion


		#region Moveable objects

		/** A delegate for the OnGrabMoveable and OnDropMoveable events */
		public delegate void Delegate_OnMoveable (DragBase dragBase);
		/** An event triggered whenever a moveable object is picked up by the player */
		public static event Delegate_OnMoveable OnGrabMoveable;
		/** An event triggered whenever a moveable object is dropped by the player */
		public static event Delegate_OnMoveable OnDropMoveable;
		/** A delegate for the OnDraggableSnap event */
		public delegate void Delegate_OnDraggableSnap (DragBase dragBase, DragTrack track, TrackSnapData trackSnapData);
		/** An event triggered whenever a draggable object snaps into a pre-set position */
		public static event Delegate_OnDraggableSnap OnDraggableSnap;

		/**
		 * <summary>Triggers the OnGrabMoveable event.</summary>
		 * <param name = "dragBase">The object being picked up</param>
		 */
		public void Call_OnGrabMoveable (DragBase dragBase)
		{
			if (OnGrabMoveable != null)
			{
				OnGrabMoveable (dragBase);
			}
		}

		/**
		 * <summary>Triggers the OnDropMoveable event.</summary>
		 * <param name = "dragBase">The object being dropped</param>
		 */
		public void Call_OnDropMoveable (DragBase dragBase)
		{
			if (OnDropMoveable != null)
			{
				OnDropMoveable (dragBase);
			}
		}


		/**
		 * <summary>Triggers the OnDraggableSnap event.</summary>
		 * <param name = "dragBase">The object snapping</param>
		 * <param name = "track">The DragTrack the object is snapped to</param>
		 * <param name = "trackSnapData">Data related to the region that the object is snapping to</param>
		 */
		public void Call_OnDraggableSnap (DragBase dragBase, DragTrack track, TrackSnapData trackSnapData)
		{
			if (OnDraggableSnap != null)
			{
				OnDraggableSnap (dragBase, track, trackSnapData);
			}
		}


		/** A delegate for the OnPickUpThrow event */
		public delegate void Delegate_OnPickUpThrow (Moveable_PickUp pickUp);
		/** An event triggered whenever a Moveable_PickUp is thrown */
		public static event Delegate_OnPickUpThrow OnPickUpThrow;

		/**
		 * <summary>Triggers the OnPickUp throw event</summary>
		 * <param name = "pickUp">The object being thrown</param>
		 */
		public void Call_OnPickUpThrow (Moveable_PickUp pickUp)
		{
			if (OnPickUpThrow != null)
			{
				OnPickUpThrow (pickUp);
			}
		}

		#endregion


		#region Camera

		/** A delegate for the OnSwitchCamera event */
		public delegate void Delegate_OnSwitchCamera (_Camera fromCamera, _Camera toCamera, float transitionTime);
		/** A delegate for the Delegate_OnShakeCamera events */
		public delegate void Delegate_OnShakeCamera (float intensity, float duration);
		/** An event triggered whenever the MainCamera switches to a new _Camera */
		public static event Delegate_OnSwitchCamera OnSwitchCamera;
		/** An event triggered whenever the MainCamera is shaken */
		public static event Delegate_OnShakeCamera OnShakeCamera;
		/** An event triggered whenever the MainCamera updates its internal record of the playable screen area, due to the aspect ratio or screen size changing */
		public static event Delegate_Generic OnUpdatePlayableScreenArea;

		/** A delegate for the OnCameraSplitScreenStart event */
		public delegate void Delegate_OnCameraSplitScreenStart (_Camera camera, CameraSplitOrientation splitOrientation, float splitAmountMain, float splitAmountOther, bool isTopLeftSplit);
		/** An event triggered when the split-screen effect begins */
		public static event Delegate_OnCameraSplitScreenStart OnCameraSplitScreenStart;
		/** A delegate for the OnCameraSplitScreenStop event */
		public delegate void Delegate_OnCameraSplitScreenStop (_Camera camera);
		/** An event triggered when the split-screen effect ends */
		public static event Delegate_OnCameraSplitScreenStop OnCameraSplitScreenStop;


		/**
		 * <summary>Triggers the OnSwitchCamera event.</summary>
		 * <param name = "dragBase">The object being picked up</param>
		 */
		public void Call_OnSwitchCamera (_Camera fromCamera, _Camera toCamera, float transitionTime)
		{
			if (OnSwitchCamera != null && toCamera != null)
			{
				OnSwitchCamera (fromCamera, toCamera, transitionTime);
			}
		}


		/**
		 * <summary>Triggers the OnShakeCamera event.</summary>
		 * <param name = "intensity">The intensity of the shake</param>
		 * <param name = "duration">The duration, in seconds</param>
		 */
		public void Call_OnShakeCamera (float intensity, float duration)
		{
			if (OnShakeCamera != null)
			{
				OnShakeCamera (intensity, duration);
			}
		}


		/** Triggers the OnUpdatePlayableScreenArea event */
		public void Call_OnUpdatePlayableScreenArea ()
		{
			if (OnUpdatePlayableScreenArea != null)
			{
				OnUpdatePlayableScreenArea ();
			}
		}


		/** 
		 * <summary>Triggers the OnCameraSplitScreenStart event</summary>
		 * <param name = "camera">The camera used in the effect</param>
		 * <param name = "splitOrientation">The orientation of the effect (Horizontal, Vertical)</param>
		 * <param name = "splitAmountMain">The proportion of the screen used by the MainCamera</param>
		 * <param name = "splitAmountOther">The proportion of the screen used by the other camera</param>
		 * <param name = "isTopLeftSplit">If True, the MainCamera will be attached to the top or left camera (depending on the orientation)</param>
		 */
		public void Call_OnCameraSplitScreenStart (_Camera camera, CameraSplitOrientation splitOrientation, float splitAmountMain, float splitAmountOther, bool isTopLeftSplit)
		{
			if (OnCameraSplitScreenStart != null && camera)
			{
				OnCameraSplitScreenStart (camera, splitOrientation, splitAmountMain, splitAmountOther, isTopLeftSplit);
			}
		}


		/** 
		 * <summary>Triggers the OnCameraSplitScreenStop event</summary>
		 * <param name = "splitCamera">The camera used in the effect that was not used by the MainCamera</param>
		 */
		public void Call_OnCameraSplitScreenStop (_Camera splitCamera)
		{
			if (OnCameraSplitScreenStop != null && splitCamera != null)
			{
				OnCameraSplitScreenStop (splitCamera);
			}
		}

		#endregion


		#region Options

		/** A delegate for the Delegate_OnChangeLanguage event */
		public delegate void Delegate_OnChangeLanguage (int language);
		/** A delegate for the Delegate_OnChangeVolume event */
		public delegate void Delegate_OnChangeVolume (SoundType soundType, float volume);
		/** A delegate for the Delegate_OnChangeSubtitles event */
		public delegate void Delegate_OnChangeSubtitles (bool showSubtitles);
		/** An event triggered whenever the current language is changed */
		public static event Delegate_OnChangeLanguage OnChangeLanguage;
		/** An event triffered whenever the current voice language is changed, provided that this is not synced to the text language */
		public static event Delegate_OnChangeLanguage OnChangeVoiceLanguage;
		/** An event triggered whenever the Music, Speech or SFX volumes are changed */
		public static event Delegate_OnChangeVolume OnChangeVolume;
		/** An event triggered whenever subtitles are turns on or off */
		public static event Delegate_OnChangeSubtitles OnChangeSubtitles;

		/**
		 * <summary>Triggers the OnChangeLanguage event.</summary>
		 * <param name = "language">The index number of the new language</param>
		 */
		public void Call_OnChangeLanguage (int language)
		{
			if (OnChangeLanguage != null)
			{
				OnChangeLanguage (language);
			}
		}

		/**
		 * <summary>Triggers the OnChangeVoiceLanguage event.</summary>
		 * <param name = "voiceLanguage">The index number of the new language</param>
		 */
		public void Call_OnChangeVoiceLanguage (int voiceLanguage)
		{
			if (OnChangeVoiceLanguage != null)
			{
				OnChangeVoiceLanguage (voiceLanguage);
			}
		}

		/**
		 * <summary>Triggers the OnChangeLanguage event.</summary>
		 * <param name = "soundType">The SoundType that was changed (Music, SFX, Speech)</param>
		 * <param name = "volume">The new volume</param>
		 */
		public void Call_OnChangeVolume (SoundType soundType, float volume)
		{
			if (OnChangeVolume != null)
			{
				OnChangeVolume (soundType, volume);
			}
		}

		/**
		 * <summary>Triggers the OnChangeSubtitles event.</summary>
		 * <param name = "showSubtitles">If True, subtitles are now displayed.</param>
		 */
		public void Call_OnChangeSubtitles (bool showSubtitles)
		{
			if (OnChangeSubtitles != null)
			{
				OnChangeSubtitles (showSubtitles);
			}
		}

		#endregion#


		#region Scene management

		/** An event triggered when the game begins */
		public static event Delegate_Generic OnBeginGame;
		/** A delegate for the events that need no parameters */
		public delegate void Delegate_NoParameters ();
		/** A delegate for the OnAfterSceneChange event */
		public delegate void Delegate_AfterSceneChange (LoadingGame loadingGame);
		/** A delegate for the OnCompleteScenePreload event */
		public delegate void Delegate_OnCompleteScenePreload (string nextSceneName);
		/** A delegate for the OnAddSubScene event */
		public delegate void Delegate_Scene (SubScene subScene);
		/** An event triggered just before the active scene is changed */
		public static event Delegate_OnCompleteScenePreload OnBeforeChangeScene;
		/** An event triggered just after the active scene is changed */
		public static event Delegate_AfterSceneChange OnAfterChangeScene;
		/** An event triggered whenever a scene starts, but not due to loading a save file */
		public static event Delegate_NoParameters OnStartScene;
		/** An event triggered after a request to preload a scene is completed */
		public static event Delegate_OnCompleteScenePreload OnCompleteScenePreload;
		/** An event triggered once the a scene load is complete, but awaits a call to SceneChanger.ActivateLoadedScene before gameplay continues */
		public static event Delegate_OnCompleteScenePreload OnAwaitSceneActivation;
		/** An event triggered when an AC scene is loaded in as a sub-scene (i.e. not the active scene) */
		public static event Delegate_Scene OnAddSubScene;
		/** A delegate for the OnDelayChangeScene event */
		public delegate void Delegate_OnDelayChangeScene (SceneInfo sceneInfo, System.Action callback);
		/** An event triggered just before the active scene is changed, but with a callback - the scene will not change until this is invoked */
		public static event Delegate_OnDelayChangeScene OnDelayChangeScene;


		/** Triggers the OnBeginGame event */
		public void Call_OnBeginGame ()
		{
			if (OnBeginGame != null)
			{
				OnBeginGame ();
			}
		}


		/** 
		 * <summary>Triggers the OnBeforeChangeScene event.</summary>
		 * <param name="nextSceneName">The name of the scene to be loaded next</param>
		 */
		public void Call_OnBeforeChangeScene (string nextSceneName)
		{
			if (OnBeforeChangeScene != null)
			{
				OnBeforeChangeScene (nextSceneName);
			}
		}


		/** 
		 * <summary>Triggers the OnDelayChangeScene event.</summary>
		 * <param name="sceneInfo">Details of the scene to be loaded next</param>
		 * <param name="callback">The callback to invoke once the delay is over</param>
		 */
		public void Call_OnDelayChangeScene (SceneInfo sceneInfo, System.Action callback)
		{
			if (OnDelayChangeScene != null)
			{
				OnDelayChangeScene (sceneInfo, callback);
			}
			else
			{
				callback.Invoke ();
			}
		}


		/** 
		 * <summary>Triggers the OnAddSubScene event</summary> 
		 * <param name = "subScene">The SubScene class instance that represents the opened scene</param>
		 */
		public void Call_OnAddSubScene (SubScene subScene)
		{
			if (OnAddSubScene != null)
			{
				OnAddSubScene (subScene);
			}
		}


		/**
		 * <summary>Triggers the OnAfterChangeScene event.</summary>
		 * <param name = "loadingGame">The current 'loading' state (No, InSameScene, InNewScene, JustSwitchingPlayer</param>
		 */
		public void Call_OnAfterChangeScene (LoadingGame loadingGame)
		{
			if (OnAfterChangeScene != null)
			{
				OnAfterChangeScene (loadingGame);
			}
		}


		/** Triggers the OnStartScene event. */
		public void Call_OnStartScene ()
		{
			if (OnStartScene != null)
			{
				OnStartScene ();
			}
		}


		/**
		 * <summary>Triggers the OnCompleteScenePreload event.</summary>
		 * <param name = "preloadedSceneName">The name of the scene that was preloaded</param>
		 */
		public void Call_OnCompleteScenePreload (string preloadedSceneName)
		{
			if (OnCompleteScenePreload != null)
			{
				OnCompleteScenePreload (preloadedSceneName);
			}
		}


		/**
		 * <summary>Triggers the OnAwaitSceneActivation event.</summary>
		 * <param name = "nextSceneName">The name of the next scene</param>
		 */
		public void Call_OnAwaitSceneActivation (string nextSceneName)
		{
			if (OnAwaitSceneActivation != null)
			{
				OnAwaitSceneActivation (nextSceneName);
			}
		}

		#endregion


		#region Engine management

		/** An event triggered if AC is manually turned on by calling KickStarter.TurnOnAC (); */
		public static event Delegate_NoParameters OnManuallyTurnACOn;
		/** An event triggered if AC is manually turned off by calling KickStarter.TurnOffAC (); */
		public static event Delegate_NoParameters OnManuallyTurnACOff;
		/** An event triggered once an AC scene is initialised, but before any save data is loaded in */
		public static event Delegate_NoParameters OnInitialiseScene;
		/** A delegate for the OnDebugLog event */
		public delegate object Delegate_OnDebugLog (object message, DebugLogType debugLogType, UnityEngine.Object context, bool isDisplayed);
		/** An event triggered when a Debug message is fired at runtime */
		public static event Delegate_OnDebugLog OnDebugLog;


		/**
		 * <summary>Triggers either the OnManuallyTurnOnAC or OnManuallyTurnOfAC event</summary>
		 * <param name = "turnOn">True if AC was turned on</param>
		 */
		public void Call_OnManuallySwitchAC (bool turnOn)
		{
			if (turnOn)
			{
				if (OnManuallyTurnACOn != null)
				{
					OnManuallyTurnACOn ();
				}
			}
			else
			{
				if (OnManuallyTurnACOff != null)
				{
					OnManuallyTurnACOff ();
				}
			}
		}


		/** Triggers the OnInitialiseScene event */
		public void Call_OnInitialiseScene ()
		{
			if (OnInitialiseScene != null)
			{
				OnInitialiseScene ();
			}
		}


		/**
		 * <summary>Triggers the OnDebugLog event</summary>
		 * <param name = "message">The message being logged</param>
		 * <param name = "debugLogType">The type of log</param>
		 * <param name = "context">The context, i.e. the object that is the reason for the log</param>
		 * <param name = "isDisplayed">If True, the message will be displayed in the Unity Console</param>
		 * <returns>The message, which can be modified if necessary</returns>
		 */
		public object Call_OnDebugLog (object message, DebugLogType debugLogType, UnityEngine.Object context, bool isDisplayed)
		{
			if (OnDebugLog != null)
			{
				return OnDebugLog (message, debugLogType, context, isDisplayed);
			}
			return message;
		}

		#endregion


		#region Documents

		/** A delegate for the OnOpenDocument and OnCloseDocument events */
		public delegate void Delegate_HandleDocument (DocumentInstance documentInstance);
		/** An event triggered when a new Document is opened */
		public static event Delegate_HandleDocument OnDocumentOpen;
		/** An event triggered when a new Document is closed */
		public static event Delegate_HandleDocument OnDocumentClose;
		/** An event triggered when a Document is added to the Player's collection */
		public static event Delegate_HandleDocument OnDocumentAdd;
		/** An event triggered when a Document is removed from the Player's collection */
		public static event Delegate_HandleDocument OnDocumentRemove;


		/**
		 * <summary>Triggers either the OnDocumentOpen or OnDocumentClose events.</summary>
		 * <param name = "documentInstance">The Document instance that is affected</param>
		 * <param name = "isOpening">If True, the Document was opened and OnDocumentOpen is triggered.  Otherwise, OnDocumentClose is triggered.</param>
		 */
		public void Call_OnHandleDocument (DocumentInstance documentInstance, bool isOpening)
		{
			if (isOpening)
			{
				if (OnDocumentOpen != null)
				{
					OnDocumentOpen (documentInstance);
				}
			}
			else
			{
				if (OnDocumentClose != null)
				{
					OnDocumentClose (documentInstance);
				}
			}
		}


		/**
		 * <summary>Triggers either the OnDocumentAdd or OnCloseDocument events.</summary>
		 * <param name = "documentInstance">The Document instance that is affected</param>
		 * <param name = "isOpening">If True, the Document was opened and OnDocumentAdd is triggered.  Otherwise, OnDocumentRemove is triggered.</param>
		 */
		public void Call_OnAddRemoveDocument (DocumentInstance documentInstance, bool isAdded)
		{
			if (isAdded)
			{
				if (OnDocumentAdd != null)
				{
					OnDocumentAdd (documentInstance);
				}
			}
			else
			{
				if (OnDocumentRemove != null)
				{
					OnDocumentRemove (documentInstance);
				}
			}
		}

		#endregion


		#region Objectives

		/** A delegate for the OnObjectiveUpdate and OnObjectiveSelect events */
		public delegate void Delegate_HandleObjective (Objective objective, ObjectiveState state);
		/** An event triggered when a Objective's state is changed */
		public static event Delegate_HandleObjective OnObjectiveUpdate;
		/** An event triggered when a Objective is selected */
		public static event Delegate_HandleObjective OnObjectiveSelect;


		/**
		 * <summary>Triggers the OnUpdateObjective event</summary>
		 * <param name = "objectiveInstance">The instance of the updated Objective</param>
		 */
		public void Call_OnObjectiveUpdate (ObjectiveInstance objectiveInstance)
		{
			if (OnObjectiveUpdate != null)
			{
				OnObjectiveUpdate (objectiveInstance.Objective, objectiveInstance.CurrentState);
			}
		}


		/**
		 * <summary>Triggers the OnObjectiveSelect event</summary>
		 * <param name = "objectiveInstance">The instance of the selected Objective</param>
		 */
		public void Call_OnObjectiveSelect (ObjectiveInstance objectiveInstance)
		{
			if (OnObjectiveSelect != null)
			{
				OnObjectiveSelect (objectiveInstance.Objective, objectiveInstance.CurrentState);
			}
		}

		#endregion


		#region Sound

		/** A delegate for the OnPlayMusic and OnPlayAmbience events */
		public delegate void Delegate_OnPlaySoundtrack (int trackID, bool loop, float fadeTime, int startingSample);
		/** A delegate for the OnStopMusic and OnStopAmbience events */
		public delegate void Delegate_OnStopSoundtrack (float fadeTime);
		/** An event triggered when a Music track plays */
		public static event Delegate_OnPlaySoundtrack OnPlayMusic;
		/** An event triggered when an Ambience track plays */
		public static event Delegate_OnPlaySoundtrack OnPlayAmbience;
		/** An event triggered when the Music stops */
		public static event Delegate_OnStopSoundtrack OnStopMusic;
		/** An event triggered when the Ambience stops */
		public static event Delegate_OnStopSoundtrack OnStopAmbience;
		/** A delegate for the OnPlayFootstepSound event */
		public delegate void Delegate_PlayFootstepSound (AC.Char character, FootstepSounds footstepSounds, bool isWalkingSound, AudioSource audioSource, AudioClip audioClip);
		/** An event triggered whenever the FootstepSounds component plays an AudioClip */
		public static Delegate_PlayFootstepSound OnPlayFootstepSound;
		/** A delegate for the OnPlaySound and OnStopSound events */
		public delegate void Delegate_OnHandleSound (Sound sound, AudioSource audioSource, AudioClip audioClip, float fadeTime);
		/** An event triggered when a Sound component plays audio */
		public static event Delegate_OnHandleSound OnPlaySound;
		/** An event triggered when a Sound component stops playing audio */
		public static event Delegate_OnHandleSound OnStopSound;
		/** A delegate for the OnRequestFootstepSounds event */
		public delegate void Delegate_FootstepSounds (FootstepSounds footstepSounds);
		/** An event triggered when a FootstepSounds component wants to know which sounds to play */
		public static event Delegate_FootstepSounds OnRequestFootstepSounds;


		/**
		 * <summary>Triggers either the OnPlayMusic or OnPlayAmbience events</summary>
		 * <param name = "trackID">The ID of the Music or Ambience track that is being played</param>
		 * <param name = "isMusic">If True, the track is Music.  If False, it is Ambience</param>
		 * <param name = "loop">If True, the audio is looping</param>
		 * <param name = "fadeTime">The fade duration, in seconds</param>
		 * <param name = "startingSample">The point at which to start the new track</param>
		 */
		public void Call_OnPlaySoundtrack (int trackID, bool isMusic, bool loop, float fadeTime, int startingSample)
		{
			if (fadeTime <= 0f) fadeTime = 0f;

			if (isMusic)
			{
				if (OnPlayMusic != null)
				{
					OnPlayMusic (trackID, loop, fadeTime, startingSample);
				}
			}
			else
			{
				if (OnPlayAmbience != null)
				{
					OnPlayAmbience (trackID, loop, fadeTime, startingSample);
				}
			}
		}


		/**
		 * <summary>Triggers either the OnStopMusic or OnStopAmbience events</summary>
		 * <param name = "isMusic">If True, the track is Music.  If False, it is Ambience</param>
		 * <param name = "fadeTime">The fade duration, in seconds</param>
		 */
		public void Call_OnStopSoundtrack (bool isMusic, float fadeTime)
		{
			if (fadeTime <= 0f) fadeTime = 0f;

			if (isMusic)
			{
				if (OnStopMusic != null)
				{
					OnStopMusic (fadeTime);
				}
			}
			else
			{
				if (OnStopAmbience != null)
				{
					OnStopAmbience (fadeTime);
				}
			}
		}


		/**
		 * <summary>Triggers the OnPlayFootstepSound event.</summary>
		 * <param name = "character">The moving character</param>
		 * <param name = "footstepSounds">The FootstepSounds component (if used) that triggered the audio</param>
		 * <param name = "isWalkingSound">If True, the character is walking.  If False, the character is running</param>
		 * <param name = "audioSource">The AudioSource component playing the audio</param>
		 * <param name = "audioClip">The audio being played</param>
		 */
		public void Call_OnPlayFootstepSound (AC.Char character, FootstepSounds footstepSounds, bool isWalkingSound, AudioSource audioSource, AudioClip audioClip)
		{
			if (OnPlayFootstepSound != null)
			{
				OnPlayFootstepSound (character, footstepSounds, isWalkingSound, audioSource, audioClip);
			}
		}


		/**
		 * <summary>Triggers the OnPlaySound event</summary>
		 * <param name = "sound">The Sound that triggered the audio</param>
		 * <param name = "_audioSource">The AudioSource componet that is playing the audio</param>
		 * <param name = "audioClip">The clip being played</param>
		 * <param name = "fadeInTime">The duration of the fade-in effect</param>
		 */
		public void Call_OnPlaySound (Sound sound, AudioSource _audioSource, AudioClip audioClip, float fadeInTime)
		{
			if (OnPlaySound != null)
			{
				OnPlaySound (sound, _audioSource, audioClip, fadeInTime);
			}
		}


		/**
		 * <summary>Triggers the OnStopSound event</summary>
		 * <param name = "sound">The Sound that triggered the audio</param>
		 * <param name = "_audioSource">The AudioSource componet that is playing the audio</param>
		 * <param name = "audioClip">The clip being stopped</param>
		 * <param name = "fadeInTime">The duration of the fade-out effect. The audio will finish playing after this time</param>
		 */
		public void Call_OnStopSound (Sound sound, AudioSource _audioSource, AudioClip audioClip, float fadeOutTime)
		{
			if (OnStopSound != null)
			{
				OnStopSound (sound, _audioSource, audioClip, fadeOutTime);
			}
		}


		/**
		 * <summary>Triggers the OnRequestFootstepSounds event</summary>
		 * <param name = "footstepSounds">The FootstepSounds component</param>
		 */
		public void Call_OnRequestFootstepSounds (FootstepSounds footstepSounds)
		{
			if (OnRequestFootstepSounds != null)
			{
				OnRequestFootstepSounds (footstepSounds);
			}
		}

		#endregion


		#region ActionLists

		/** A delegate for the OnBeginActionList event */
		public delegate void Delegate_OnBeginActionList (ActionList actionList, ActionListAsset actionListAsset, int startingIndex, bool isSkipping);
		/** An event triggered when an ActionList is run */
		public static event Delegate_OnBeginActionList OnBeginActionList;

		/** A delegate for the OnEndActionList event */
		public delegate void Delegate_OnEndActionList (ActionList actionList, ActionListAsset actionListAsset, bool isSkipping);
		/** An event triggered when an ActionList is ended */
		public static event Delegate_OnEndActionList OnEndActionList;

		/** A delegate for the OnPauseActionList and OnResumeActionList events */
		public delegate void Delegate_OnPauseActionList (ActionList actionList);
		/** An event triggered when an ActionList is paused */
		public static event Delegate_OnPauseActionList OnPauseActionList;
		/** An event triggered when an ActionList is resumed */
		public static event Delegate_OnPauseActionList OnResumeActionList;
		/** An event triggered when skipping a cutscene */
		public static event Delegate_Generic OnSkipCutscene;


		/**
		 * <summary>Triggers the OnBeginActionList event.</summary>
		 * <param name = "actionList">The ActionList that is running</param>
		 * <param name = "actionListAsset">The ActionListAsset that the Actions come from, if an asset.  If this is not null, then actionList is an instance of RuntimeActionList made specifically for the asset running at this moment</param>
		 * <param name = "startingIndex">The index number to start from, out of the List of Actions. If zero, the ActionList will start from the beginning</param>
		 * <param name = "isSkipping">If True, then the ActionList is being skipped, and will run instantly</param>
		 */
		public void Call_OnBeginActionList (ActionList actionList, ActionListAsset actionListAsset, int startingIndex, bool isSkipping)
		{
			if (OnBeginActionList != null)
			{
				OnBeginActionList (actionList, actionListAsset, startingIndex, isSkipping);
			}
		}


		/**
		 * <summary>Triggers the OnEndActionList event.</summary>
		 * <param name = "actionList">The ActionList that is ending</param>
		 * <param name = "actionListAsset">The ActionListAsset that the Actions come from, if an asset.  If this is not null, then actionList is an instance of RuntimeActionList made specifically for the asset running at this moment</param>
		 * <param name = "isSkipping">If True, then the ActionList was skipped, and run instantly</param>
		 */
		public void Call_OnEndActionList (ActionList actionList, ActionListAsset actionListAsset, bool isSkipping)
		{
			if (OnEndActionList != null)
			{
				OnEndActionList (actionList, actionListAsset, isSkipping);
			}
		}


		/**
		 * <summary>Triggers the OnPauseActionList event.</summary>
		 * <param name = "actionList">The ActionList that being paused</param>
		 */
		public void Call_OnPauseActionList (ActionList actionList)
		{
			if (OnPauseActionList != null)
			{
				OnPauseActionList (actionList);
			}
		}


		/**
		 * <summary>Triggers the OnResumeActionList event.</summary>
		 * <param name = "actionList">The ActionList that being resumed</param>
		 */
		public void Call_OnResumeActionList (ActionList actionList)
		{
			if (OnResumeActionList != null)
			{
				OnResumeActionList (actionList);
			}
		}


		/** Triggers the OnSkipCutscene event */
		public void Call_OnSkipCutscene ()
		{
			if (OnSkipCutscene != null)
			{
				OnSkipCutscene ();
			}
		}

		#endregion


		#region Input

		/** A delegate for the OnQTEBegin event */
		public delegate void Delegate_OnQTEBegin (QTEType qteType, string inputName, float duration);
		/** An event triggered when quick-time event is begun */
		public static event Delegate_OnQTEBegin OnQTEBegin;
		/** A delegate for the OnQTEWin and OnQTELose events */
		public delegate void Delegate_OnQTEWinLose (QTEType qteType);
		/** An event triggered when a quick-time event is won */
		public static event Delegate_OnQTEWinLose OnQTEWin;
		/** An event triggered when a quick-time event is lost */
		public static event Delegate_OnQTEWinLose OnQTELose;


		/**
		 * <summary>Triggers the OnQTEBegin event</summary>
		 * <param name = "qteType">The type of QTE that began</param>
		 * <param name = "inputName">The name of the input axis used to complete the QTE</param>
		 * <param name = "duration">The duration, in seconds</param>
		 */
		public void Call_OnQTEBegin (QTEType qteType, string inputName, float duration)
		{
			if (OnQTEBegin != null)
			{
				OnQTEBegin (qteType, inputName, duration);
			}
		}


		/**
		 * <summary>Triggers either the OnQTEWin or OnQTELose events</summary>
		 * <param name = "qteType">The type of QTE that ended</param>
		 * <param name = "wasWon">If True, OnQTEWin will be triggered. Otherwise, OnQTELose will be triggered.</param>
		 */
		public void Call_OnQTEEnd (QTEType qteType, bool wasWon)
		{
			if (wasWon)
			{
				if (OnQTEWin != null)
				{
					OnQTEWin (qteType);
				}
			}
			else
			{
				if (OnQTELose != null)
				{
					OnQTELose (qteType);
				}
			}
		}


		/** A delegate for OnActiveInputFire event */
		public delegate void Delegate_ActiveInput (ActiveInput activeInput);
		/** An event triggered when an Active Input is fired */
		public static event Delegate_ActiveInput OnActiveInputFire;

		/** Triggers the OnActiveInputFire event */
		public void Call_OnActiveInputFire (ActiveInput activeInput)
		{
			if (OnActiveInputFire != null)
			{
				OnActiveInputFire (activeInput);
			}
		}

		#endregion

	}

}