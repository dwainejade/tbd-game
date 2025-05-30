/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2024
 *	
 *	"ActionTransform.cs"
 * 
 *	This action modifies a GameObject position, rotation or scale over a set time.
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
	public class ActionTransform : Action
	{

		public bool isPlayer;
		public int playerID = -1;

		public int markerParameterID = -1;
		public int markerID = 0;
		public Marker marker;
		protected Marker runtimeMarker;

		public bool scaleDuration = false;

		public bool doEulerRotation = false;
		public bool clearExisting = true;
		public bool inWorldSpace = false;

		public bool copyMarkerPosition = true;
		public bool copyMarkerRotation = true;
		public bool copyMarkerScale = true;
		
		public AnimationCurve timeCurve = new AnimationCurve (new Keyframe(0, 0), new Keyframe(1, 1));
		
		public int parameterID = -1;
		public int constantID = 0;
		public Moveable linkedProp;
		protected Moveable runtimeLinkedProp;

		public enum SetVectorMethod { EnteredHere, FromVector3Variable };
		public SetVectorMethod setVectorMethod = SetVectorMethod.EnteredHere;

		public int newVectorParameterID = -1;

		[ContextMenuItem ("Copy from Moveable", "CopyFromMoveable")]
		public Vector3 newVector;

		public int vectorVarParameterID = -1;
		public int vectorVarID;
		public VariableLocation variableLocation = VariableLocation.Global;

		public float transitionTime;
		public int transitionTimeParameterID = -1;
		
		public TransformType transformType;
		public MoveMethod moveMethod;
		
		public enum ToBy { To, By };
		public ToBy toBy;

		protected Vector3 nonSkipTargetVector = Vector3.zero;

		public Variables variables;
		public int variablesConstantID = 0;

		protected GVar runtimeVariable;
		protected LocalVariables localVariables;


		public override ActionCategory Category { get { return ActionCategory.Object; }}
		public override string Title { get { return "Transform"; }}
		public override string Description { get { return "Transforms a GameObject over time, by or to a given amount, or towards a Marker in the scene. The GameObject must have a Moveable script attached."; }}
		
		
		public override void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				Player player = AssignPlayer (playerID, parameters, parameterID);
				runtimeLinkedProp = (player != null) ? player.GetComponent<Moveable> () : null;
			}
			else
			{
				runtimeLinkedProp = AssignFile <Moveable> (parameters, parameterID, constantID, linkedProp);
			}

			runtimeMarker = AssignFile <Marker> (parameters, markerParameterID, markerID, marker);
			transitionTime = AssignFloat (parameters, transitionTimeParameterID, transitionTime);
			newVector = AssignVector3 (parameters, newVectorParameterID, newVector);

			if (!(transformType == TransformType.CopyMarker ||
				(transformType == TransformType.Translate && toBy == ToBy.To) ||
				(transformType == TransformType.Rotate && toBy == ToBy.To)))
			{
				inWorldSpace = false;
			}

			runtimeVariable = null;
			if (transformType != TransformType.CopyMarker && setVectorMethod == SetVectorMethod.FromVector3Variable)
			{
				switch (variableLocation)
				{
					case VariableLocation.Global:
						vectorVarID = AssignVariableID (parameters, vectorVarParameterID, vectorVarID);
						runtimeVariable = GlobalVariables.GetVariable (vectorVarID, true);
						break;

					case VariableLocation.Local:
						if (!isAssetFile)
						{
							vectorVarID = AssignVariableID (parameters, vectorVarParameterID, vectorVarID);
							runtimeVariable = LocalVariables.GetVariable (vectorVarID, localVariables);
						}
						break;

					case VariableLocation.Component:
						Variables runtimeVariables = AssignFile <Variables> (variablesConstantID, variables);
						if (runtimeVariables != null)
						{
							runtimeVariable = runtimeVariables.GetVariable (vectorVarID);
						}
						runtimeVariable = AssignVariable (parameters, vectorVarParameterID, runtimeVariable);
						break;
				}
			}
		}


		public override void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject (actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}

			base.AssignParentList (actionList);
		}
		
		
		public override float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				
				if (runtimeLinkedProp != null)
				{
					float _transitionTime = Mathf.Max (transitionTime, 0f);
					RunToTime (_transitionTime, false);
					
					if (willWait && _transitionTime > 0f)
					{
						return defaultPauseTime;
					}
				}
				else
				{
					if (isPlayer && KickStarter.player != null)
					{
						LogWarning ("The player " + KickStarter.player + " requires a Moveable component to be moved.", KickStarter.player);
					}
				}
			}
			else
			{
				if (runtimeLinkedProp != null)
				{
					if (!runtimeLinkedProp.IsMoving (transformType))
					{
						isRunning = false;
					}
					else
					{
						return defaultPauseTime;
					}
				}
			}
			
			return 0f;
		}
		
		
		public override void Skip ()
		{
			if (runtimeLinkedProp)
			{
				RunToTime (0f, true);
			}
		}
		

		protected void RunToTime (float _time, bool isSkipping)
		{
			if (transformType == TransformType.CopyMarker)
			{
				if (runtimeMarker)
				{
					runtimeLinkedProp.Move (runtimeMarker, moveMethod, inWorldSpace, _time, timeCurve, copyMarkerPosition, copyMarkerRotation, copyMarkerScale);
				}
			}
			else
			{
				Vector3 targetVector = Vector3.zero;
				float speedScaler = 1f;

				if (setVectorMethod == SetVectorMethod.FromVector3Variable)
				{
					if (runtimeVariable != null)
					{
						targetVector = runtimeVariable.Vector3Value;
					}
				}
				else if (setVectorMethod == SetVectorMethod.EnteredHere)
				{
					targetVector = newVector;
				}

				if (transformType == TransformType.Translate)
				{
					if (toBy == ToBy.By)
					{
						targetVector = SetRelativeTarget (targetVector, isSkipping, runtimeLinkedProp.transform.localPosition);
						speedScaler = targetVector.magnitude;
					}
					else
					{
						speedScaler = Vector3.Distance (runtimeLinkedProp.transform.localPosition, targetVector);
					}

					if (scaleDuration)
					{
						_time *= speedScaler * 0.2f;
					}
				}
				else if (transformType == TransformType.Rotate)
				{
					if (toBy == ToBy.By)
					{
						int numZeros = 0;
						if (Mathf.Approximately (targetVector.x, 0f)) numZeros ++;
						if (Mathf.Approximately (targetVector.y, 0f)) numZeros ++;
						if (Mathf.Approximately (targetVector.z, 0f)) numZeros ++;

						if (numZeros == 3)
						{
							targetVector = SetRelativeTarget (targetVector, isSkipping, runtimeLinkedProp.transform.eulerAngles);
						}
						else
						{
							Quaternion currentRotation = runtimeLinkedProp.transform.localRotation;
							runtimeLinkedProp.transform.Rotate (targetVector, Space.World);
							targetVector = runtimeLinkedProp.transform.localEulerAngles;
							runtimeLinkedProp.transform.localRotation = currentRotation;
						}

						speedScaler = targetVector.magnitude;
					}
					else
					{
						speedScaler = Vector3.Distance (runtimeLinkedProp.transform.eulerAngles, targetVector);
					}

					if (scaleDuration)
					{
						_time *= speedScaler * 0.01f;
					}
				}
				else if (transformType == TransformType.Scale)
				{
					if (toBy == ToBy.By)
					{
						targetVector = SetRelativeTarget (targetVector, isSkipping, runtimeLinkedProp.transform.localScale);
						speedScaler = targetVector.magnitude;
					}
					else
					{
						speedScaler = Vector3.Distance (runtimeLinkedProp.transform.localScale, targetVector);
					}

					if (scaleDuration)
					{
						_time *= speedScaler * 0.2f;
					}
				}
				
				if (transformType == TransformType.Rotate)
				{
					runtimeLinkedProp.Move (targetVector, moveMethod, inWorldSpace, _time, transformType, doEulerRotation, timeCurve, clearExisting);
				}
				else
				{
					runtimeLinkedProp.Move (targetVector, moveMethod, inWorldSpace, _time, transformType, false, timeCurve, clearExisting);
				}
			}
		}


		protected Vector3 SetRelativeTarget (Vector3 _targetVector, bool isSkipping, Vector3 normalAddition)
		{
			if (isSkipping && nonSkipTargetVector != Vector3.zero)
			{
				_targetVector = nonSkipTargetVector;
			}
			else
			{
				_targetVector += normalAddition;
				nonSkipTargetVector = _targetVector;
			}
			return _targetVector;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Move Player?", isPlayer);
			if (isPlayer)
			{
				PlayerField (ref playerID, parameters, ref parameterID);
			}
			else
			{
				ComponentField ("Moveable object:", ref linkedProp, ref constantID, parameters, ref parameterID);
			}

			EditorGUILayout.BeginHorizontal ();
			transformType = (TransformType) EditorGUILayout.EnumPopup (transformType);
			if (transformType != TransformType.CopyMarker)
			{
				toBy = (ToBy) EditorGUILayout.EnumPopup (toBy);
			}
			EditorGUILayout.EndHorizontal ();
			
			if (transformType == TransformType.CopyMarker)
			{
				ComponentField ("Marker:", ref marker, ref markerID, parameters, ref markerParameterID);
				copyMarkerPosition = EditorGUILayout.Toggle ("Copy position?", copyMarkerPosition);
				copyMarkerRotation = EditorGUILayout.Toggle ("Copy rotation?", copyMarkerRotation);
				copyMarkerScale = EditorGUILayout.Toggle ("Copy scale?", copyMarkerScale);
			}
			else
			{
				setVectorMethod = (SetVectorMethod) EditorGUILayout.EnumPopup ("Vector is: ", setVectorMethod);
				if (setVectorMethod == SetVectorMethod.EnteredHere)
				{
					Vector3Field ("Value:", ref newVector, parameters, ref newVectorParameterID);
				}
				else if (setVectorMethod == SetVectorMethod.FromVector3Variable)
				{
					variableLocation = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", variableLocation);

					switch (variableLocation)
					{
						case VariableLocation.Global:
							GlobalVariableField ("Vector3 variable:", ref vectorVarID, VariableType.Vector3, parameters, ref vectorVarParameterID);
							break;

						case VariableLocation.Local:
							if (!isAssetFile)
							{
								LocalVariableField ("Vector3 variable:", ref vectorVarID, VariableType.Vector3, parameters, ref vectorVarParameterID);
							}
							else
							{
								EditorGUILayout.HelpBox ("Local variables cannot be accessed in ActionList assets.", MessageType.Info);
							}
							break;

						case VariableLocation.Component:
							ComponentVariableField ("Vector3 variable:", ref variables, ref variablesConstantID, ref vectorVarID, VariableType.Vector3, parameters, ref vectorVarParameterID);
							break;
					}
				}

				clearExisting = EditorGUILayout.Toggle ("Stop existing transforms?", clearExisting);
			}

			if (transformType == TransformType.CopyMarker ||
				(transformType == TransformType.Translate && toBy == ToBy.To) ||
				(transformType == TransformType.Rotate && toBy == ToBy.To))
			{
				inWorldSpace = EditorGUILayout.Toggle ("Act in world-space?", inWorldSpace);

				if (inWorldSpace && transformType == TransformType.CopyMarker)
				{
					EditorGUILayout.HelpBox ("The moveable object's scale will be changed in local space.", MessageType.Info);
				}
			}

			FloatField ("Transition time (s):", ref transitionTime, parameters, ref transitionTimeParameterID);

			if (transitionTime > 0f)
			{
				if (transformType != TransformType.CopyMarker)
				{
					scaleDuration = EditorGUILayout.Toggle ("Scale time with distance?", scaleDuration);
				}

				if (transformType == TransformType.Rotate)
				{
					doEulerRotation = EditorGUILayout.Toggle ("Euler rotation?", doEulerRotation);
				}
				moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = EditorGUILayout.CurveField ("Time curve:", timeCurve);
				}
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
		}


		public override void AssignConstantIDs (bool saveScriptsToo, bool fromAssetFile)
		{
			if (saveScriptsToo)
			{
				if (!isPlayer)
				{
					AddSaveScript<RememberMoveable> (linkedProp);
				}
			}
			constantID = AssignConstantID<Moveable> (linkedProp, constantID, parameterID);
			markerID = AssignConstantID<Marker> (marker, markerID, markerParameterID);

			if (transformType != TransformType.CopyMarker &&
				setVectorMethod == SetVectorMethod.FromVector3Variable &&
				variableLocation == VariableLocation.Component)
			{
				variablesConstantID = AssignConstantID<Variables> (variables, variablesConstantID, vectorVarParameterID);
			}
		}


		public override string SetLabel ()
		{
			if (linkedProp != null)
			{
				return linkedProp.name;
			}
			return string.Empty;
		}


		public override bool ReferencesObjectOrID (GameObject gameObject, int id)
		{
			if (!isPlayer && parameterID < 0)
			{
				if (linkedProp && linkedProp.gameObject == gameObject) return true;
				if (constantID == id && id != 0) return true;
			}
			if (isPlayer && gameObject && gameObject.GetComponent<Player> ()) return true;
			if (transformType != TransformType.CopyMarker && setVectorMethod == SetVectorMethod.FromVector3Variable && variableLocation == VariableLocation.Component && vectorVarParameterID < 0)
			{
				if (variables && variables.gameObject == gameObject) return true;
				if (variablesConstantID == id && id != 0) return true;
			}
			return base.ReferencesObjectOrID (gameObject, id);
		}


		public override bool ReferencesPlayer (int _playerID = -1)
		{
			if (!isPlayer) return false;
			if (_playerID < 0) return true;
			if (playerID < 0 && parameterID < 0) return true;
			return (parameterID < 0 && playerID == _playerID);
		}

		private void CopyFromMoveable ()
		{
			if (linkedProp == null) return;

			switch (transformType)
			{
				case TransformType.Translate:
					newVector = linkedProp.transform.position;
					break;

				case TransformType.Rotate:
					newVector = linkedProp.transform.eulerAngles;
					break;

				case TransformType.Scale:
					newVector = linkedProp.transform.localScale;
					break;

				default:
					break;
			}
		}

		#endif


		/**
		 * <summary>Creates a new instance of the 'Object: Transform' Action</summary>
		 * <param name = "objectToMove">The Moveable object to move</param>
		 * <param name = "markerToMoveTo">The Marker to move towards</param>
		 * <param name = "inWorldSpace">If True, the Marker's transform values will be read in world space</param>
		 * <param name = "transitionTime">The time, in seconds, to take when moving to the Marker</param>
		 * <param name = "waitUntilFinish">If True, then the Action will wait until the transition is complete</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionTransform CreateNew (Moveable objectToMove, Marker markerToMoveTo, bool inWorldSpace = true, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.CopyMarker;
			newAction.marker = markerToMoveTo;
			newAction.inWorldSpace = inWorldSpace;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}


		public static ActionTransform CreateNew_TranslateBy (Moveable objectToMove, Vector3 moveAmount, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.Translate;
			newAction.newVector = moveAmount;
			newAction.toBy = ToBy.By;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}


		public static ActionTransform CreateNew_TranslateTo (Moveable objectToMove, Vector3 moveAmount, bool inWorldSpace = true, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.Translate;
			newAction.newVector = moveAmount;
			newAction.toBy = ToBy.To;
			newAction.inWorldSpace = inWorldSpace;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}


		public static ActionTransform CreateNew_RotateBy (Moveable objectToMove, Vector3 eulerAngles, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.Rotate;
			newAction.newVector = eulerAngles;
			newAction.toBy = ToBy.By;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}


		public static ActionTransform CreateNew_RotateTo (Moveable objectToMove, Vector3 eulerAngles, bool inWorldSpace = true, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.Rotate;
			newAction.newVector = eulerAngles;
			newAction.toBy = ToBy.To;
			newAction.inWorldSpace = inWorldSpace;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}


		public static ActionTransform CreateNew_ScaleBy (Moveable objectToMove, Vector3 scaleVector, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.Scale;
			newAction.newVector = scaleVector;
			newAction.toBy = ToBy.By;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}


		public static ActionTransform CreateNew_ScaleTo (Moveable objectToMove, Vector3 scaleVector, float transitionTime = 1f, MoveMethod moveMethod = MoveMethod.Smooth, AnimationCurve timeCurve = null, bool waitUntilFinish = false)
		{
			ActionTransform newAction = CreateNew<ActionTransform> ();
			newAction.linkedProp = objectToMove;
			newAction.TryAssignConstantID (newAction.linkedProp, ref newAction.constantID);
			newAction.transformType = TransformType.Scale;
			newAction.newVector = scaleVector;
			newAction.toBy = ToBy.To;
			newAction.transitionTime = transitionTime;
			newAction.moveMethod = moveMethod;
			newAction.timeCurve = timeCurve;
			newAction.willWait = waitUntilFinish;
			return newAction;
		}

	}

}