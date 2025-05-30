﻿using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/** A subclass of _Camera that allows for the cursor position to tweak the camera's rotation */
	public abstract class CursorInfluenceCamera : _Camera
	{

		#region Variables

		/** If True, then the camera will rotate towards the cursor's position on-screen */
		public bool followCursor = false;
		/** The influence that the cursor's position has on rotation, if followCursor = True */
		public Vector2 cursorInfluence = new Vector2 (0.3f, 0.1f);
		/** If True, and followCursor = True, then camera rotation according to the cursor's X position will be limited */
		public bool constrainCursorInfluenceX = false;
		/** The lower and upper limits, if constrainCursorInfluenceX = True */
		public Vector2 limitCursorInfluenceX;
		/** If True, and followCursor = True, then camera rotation according to the cursor's Y position will be limited */
		public bool constrainCursorInfluenceY = false;
		/** The lower and upper limits, if constrainCursorInfluenceY = True */
		public Vector2 limitCursorInfluenceY;
		/** The speed at which the camera follows the cursor */
		public float followCursorSpeed = 3f;
		[SerializeField] private bool cursorInfluenceDuringCutscenes = false;
		/** The influence of the cursor position on the camera's rotation during cutscenes (Freeze, Allow, Reset) */
		public CutsceneBehaviour cutsceneBehaviour = CutsceneBehaviour.Freeze;

		/** The influence of the cursor position on the camera's rotation during conversations (Freeze, Allow, Reset) */
		public CutsceneBehaviour conversationBehaviour = CutsceneBehaviour.Freeze;

		public enum CutsceneBehaviour { Freeze, Allow, Reset };

		private bool forceFreeze;

		protected Vector2 actualCursorOffset;

		#endregion


		#region PublicFunctions

		public override Vector2 CreateRotationOffset ()
		{
			#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				return Vector2.zero;
			}
			#endif

			if (followCursor)
			{
				if (forceFreeze)
				{
					return actualCursorOffset;
				}

				if (cursorInfluenceDuringCutscenes)
				{
					cursorInfluenceDuringCutscenes = false;
					cutsceneBehaviour = CutsceneBehaviour.Allow;
				}

				bool isInCutsene = KickStarter.stateHandler.IsInCutscene ();
				bool isInConversation = KickStarter.stateHandler.gameState == GameState.DialogOptions;

				if (KickStarter.stateHandler.IsInGameplay () || (conversationBehaviour == CutsceneBehaviour.Allow && isInConversation) || (cutsceneBehaviour == CutsceneBehaviour.Allow && isInCutsene) || (KickStarter.stateHandler.IsPaused () && updateWhilePaused))
				{
					Vector2 mousePosition = KickStarter.playerInput.GetMousePosition ();
					Vector2 mouseOffset = new Vector2 (mousePosition.x / ( ACScreen.width / 2) - 1, mousePosition.y / ( ACScreen.height / 2) - 1);
					float distFromCentre = mouseOffset.sqrMagnitude;

					if (distFromCentre < 1.96f)
					{
						if (constrainCursorInfluenceX)
						{
							mouseOffset.x = Mathf.Clamp (mouseOffset.x, limitCursorInfluenceX[0], limitCursorInfluenceX[1]);
						}
						if (constrainCursorInfluenceY)
						{
							mouseOffset.y = Mathf.Clamp (mouseOffset.y, limitCursorInfluenceY[0], limitCursorInfluenceY[1]);
						}
					}

					Vector2 targetCursorOffset = new Vector2 (mouseOffset.x * cursorInfluence.x, mouseOffset.y * cursorInfluence.y);
					actualCursorOffset = Vector2.Lerp (actualCursorOffset, targetCursorOffset, LerpSpeed);
				}
				else if ((isInCutsene && cutsceneBehaviour == CutsceneBehaviour.Reset) || (isInConversation && conversationBehaviour == CutsceneBehaviour.Reset))
				{
					actualCursorOffset = Vector2.Lerp (actualCursorOffset, Vector2.zero, LerpSpeed);
				}

				return actualCursorOffset;
			}
			return Vector2.zero;
		}

		#endregion


		#if UNITY_EDITOR

		public void ShowCursorInfluenceGUI ()
		{
			CustomGUILayout.Header ("Cursor influence");
			CustomGUILayout.BeginVertical ();
			followCursor = CustomGUILayout.Toggle ("Follow cursor?", followCursor, string.Empty, "If True, then the camera will rotate towards the cursor's position on-screen");
			if (followCursor)
			{
				cursorInfluence = CustomGUILayout.Vector2Field ("Panning factor:", cursorInfluence, string.Empty, "The influence that the cursor's position has on rotation");
				followCursorSpeed = CustomGUILayout.Slider ("Follow speed:", followCursorSpeed, 0.01f, 1f, string.Empty, "The speed at which the camera follows the cursor.");

				if (cursorInfluenceDuringCutscenes)
				{
					cursorInfluenceDuringCutscenes = false;
					cutsceneBehaviour = CutsceneBehaviour.Allow;
				}

				cutsceneBehaviour = (CutsceneBehaviour) CustomGUILayout.EnumPopup ("Cutscene behaviour:", cutsceneBehaviour, string.Empty, "The influence of the cursor position on the camera's rotation during cutscenes");
				conversationBehaviour = (CutsceneBehaviour) CustomGUILayout.EnumPopup ("Conversation behaviour:", conversationBehaviour, string.Empty, "The influence of the cursor position on the camera's rotation during conversations");

				constrainCursorInfluenceX = CustomGUILayout.Toggle ("Constrain in X direction?", constrainCursorInfluenceX, string.Empty, "If True, then camera rotation according to the cursor's X position will be limited");
				if (constrainCursorInfluenceX)
				{
					limitCursorInfluenceX[0] = CustomGUILayout.Slider ("Minimum X constraint:", limitCursorInfluenceX[0], -1.4f, 0f, string.Empty, "The cursor influence's lower limit in the X-direction");
					limitCursorInfluenceX[1] = CustomGUILayout.Slider ("Maximum X constraint:", limitCursorInfluenceX[1], 0f, 1.4f, string.Empty, "The cursor influence's upper limit in the X-direction");
				}
				constrainCursorInfluenceY = CustomGUILayout.Toggle ("Constrain in Y direction?", constrainCursorInfluenceY, string.Empty, "If True, then camera rotation according to the cursor's Y position will be limited");
				if (constrainCursorInfluenceY)
				{
					limitCursorInfluenceY[0] = CustomGUILayout.Slider ("Minimum Y constraint:", limitCursorInfluenceY[0], -1.4f, 0f, string.Empty, "The cursor influence's lower limit in the Y-direction");
					limitCursorInfluenceY[1] = CustomGUILayout.Slider ("Maximum Y constraint:", limitCursorInfluenceY[1], 0f, 1.4f, string.Empty, "The cursor influence's upper limit in the Y-direction");
				}

				updateWhilePaused = CustomGUILayout.Toggle ("Update while paused?", updateWhilePaused, "", "If True, the camera will not be frozen while the game is paused");

				if (Application.isPlaying && KickStarter.mainCamera && KickStarter.mainCamera.attachedCamera == this)
				{
					EditorGUILayout.HelpBox ("Changes made to this panel will not be felt until the MainCamera switches to this camera again.", MessageType.Info);
				}
			}
			CustomGUILayout.EndVertical ();
		}

		#endif


		#region GetSet

		private float LerpSpeed { get { return (1f - Mathf.Pow (1f - Mathf.Clamp01 (followCursorSpeed), updateWhilePaused ? Time.unscaledDeltaTime : Time.deltaTime)); } }

		/** If True, the cursor's influence over the camera will be frozen */
		public bool FreezeCursorInfluence
		{
			get
			{
				return forceFreeze;
			}
			set
			{
				forceFreeze = value;
			}
		}

		#endregion

	}

}