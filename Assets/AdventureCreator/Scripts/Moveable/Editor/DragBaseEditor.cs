﻿#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(DragBase))]
	public class DragBaseEditor : Editor
	{

		protected CursorManager cursorManager;
		private DragBase baseTarget;


		protected void GetReferences ()
		{
			if (KickStarter.cursorManager)
			{
				cursorManager = KickStarter.cursorManager;
			}
		}


		private InteractiveBoundary OnAutoCreateInteractiveBoundary ()
		{
			string prefabName = "InteractiveBoundary";
			if (SceneSettings.IsUnity2D ())
			{
				prefabName += "2D";
			}
			InteractiveBoundary newInteractiveBoundary = SceneManager.AddPrefab ("Logic", prefabName, true, false, true).GetComponent <InteractiveBoundary>();
			newInteractiveBoundary.gameObject.name += (": " + baseTarget.gameObject.name);
			newInteractiveBoundary.transform.position = baseTarget.transform.position;

			UnityVersionHandler.PutInFolder (newInteractiveBoundary.gameObject, "_Hotspots");
			return newInteractiveBoundary;
		}


		protected void SharedGUI (DragBase _target, bool isOnHinge)
		{
			baseTarget = _target;

			CustomGUILayout.Header ("Collision");
			CustomGUILayout.BeginVertical ();
			_target.ignorePlayerCollider = CustomGUILayout.Toggle ("Ignore Player's collider?", _target.ignorePlayerCollider, "", "If True, then the Physics system will ignore collisions between this object and the player");
			_target.ignoreMoveableRigidbodies = CustomGUILayout.Toggle ("Ignore Moveable Rigidbodies?", _target.ignoreMoveableRigidbodies, "", " If True, then the Physics system will ignore collisions between this object and the boundary colliders of any DragTrack that this is not locked to");
			_target.childrenShareLayer = CustomGUILayout.Toggle ("Place children on same layer?", _target.childrenShareLayer, "", "If True, then this object's children will be placed on the same layer");

			_target.interactiveBoundary = (InteractiveBoundary) CustomGUILayout.AutoCreateField ("Interactive boundary:", _target.interactiveBoundary, OnAutoCreateInteractiveBoundary, "", "If assigned, then the draggable will only be interactive when the player is within this Trigger Collider's boundary");

			_target.limitToCamera = (_Camera) CustomGUILayout.ObjectField <_Camera> ("Limit to camera:", _target.limitToCamera, true, "", "If assigned, then the draggable  will only be interactive when the assigned _Camera is active");

			CustomGUILayout.EndVertical ();

			CustomGUILayout.Header ("Icon");
			CustomGUILayout.BeginVertical ();
			_target.showIcon = CustomGUILayout.Toggle ("Icon at contact point?", _target.showIcon, "", "If True, then an icon will be displayed at the 'grab point' when the object is held");
			if (_target.showIcon)
			{
				if (cursorManager && cursorManager.cursorIcons.Count > 0)
				{
					int cursorInt = cursorManager.GetIntFromID (_target.iconID);
					cursorInt = CustomGUILayout.Popup ("Cursor icon:", cursorInt, cursorManager.GetLabelsArray (), "", "The cursor that gets shown when held");
					_target.iconID = cursorManager.cursorIcons [cursorInt].id;
				}
				else
				{
					_target.iconID = -1;
				}
			}		
			CustomGUILayout.EndVertical ();

			CustomGUILayout.Header ("Sound");
			CustomGUILayout.BeginVertical ();
			_target.moveSoundClip = (AudioClip) CustomGUILayout.ObjectField <AudioClip> ("Move sound:", _target.moveSoundClip, false, "", "The sound to play when the object is moved");
			_target.moveSound = (Sound) CustomGUILayout.ObjectField <Sound> ("Move Sound object:", _target.moveSound, true, "", "The Sound component to play move sounds from");
			_target.slideSoundThreshold = CustomGUILayout.FloatField ("Min. move speed:", _target.slideSoundThreshold, "", "The minimum speed that the object must be moving by for sound to play");
			_target.slidePitchFactor = CustomGUILayout.FloatField ("Pitch factor:", _target.slidePitchFactor, "", "The factor by which the movement sound's pitch is adjusted in relation to speed");
		
			_target.collideSoundClip = (AudioClip) CustomGUILayout.ObjectField <AudioClip> ("Collide sound:", _target.collideSoundClip, false, "", "The sound to play when the object has a collision");
			if (isOnHinge)
			{
				_target.onlyPlayLowerCollisionSound = CustomGUILayout.Toggle ("Only on lower boundary?", _target.onlyPlayLowerCollisionSound, "", "If True, then the collision sound will only play when the object collides with its lower boundary collider");
			}
			CustomGUILayout.EndVertical ();
		}

	}

}

#endif