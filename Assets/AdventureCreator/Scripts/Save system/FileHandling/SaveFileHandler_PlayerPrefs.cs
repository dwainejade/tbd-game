﻿#if !(UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII || UNITY_PS4 || UNITY_SWITCH || UNITY_WSA)
#define CAN_HANDLE_SCREENSHOTS
#endif

using UnityEngine;
using System;
using System.Collections.Generic;

namespace AC
{

	/** A save-file handler that stores save-games as separate keys in the PlayerPrefs */
	public class SaveFileHandler_PlayerPrefs : iSaveFileHandler
	{

		#region Variables

		protected const string screenshotKey = "_screenshot";

		#endregion


		#region PublicFunctions

		public virtual string GetDefaultSaveLabel (int saveID)
		{
			string label = (saveID == 0)
							? SaveSystem.AutosaveLabel
							: (SaveSystem.SaveLabel + " " + saveID.ToString ());

			label += GetTimeString (System.DateTime.Now);
			return label;
		}


		public virtual void DeleteAll (int profileID)
		{
			GatherSaveFiles (profileID, OnGatherFilesForDeletion);
		}


		public virtual void Delete (SaveFile saveFile, System.Action<bool> callback = null)
		{
			string filename = saveFile.fileName;

			if (PlayerPrefs.HasKey (filename))
			{
				PlayerPrefs.DeleteKey (filename);
				ACDebug.Log ("PlayerPrefs key deleted: " + filename);

				if (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.Always || (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.ExceptWhenAutosaving && !saveFile.IsAutoSave))
				{
					if (PlayerPrefs.HasKey (filename + screenshotKey))
					{
						PlayerPrefs.DeleteKey (filename + screenshotKey);

						if (callback != null) callback.Invoke (true);
						return;
					}
				}
			}

			if (callback != null) callback.Invoke (false);
		}


		public virtual void Save (SaveFile saveFile, string dataToSave, System.Action<bool> callback)
		{
			string fullFilename = GetSaveFilename (saveFile.saveID, saveFile.profileID);
			bool isSuccessful = false;

			try
			{
				PlayerPrefs.SetString (fullFilename, dataToSave);
				#if UNITY_PS4 || UNITY_SWITCH || UNITY_WEBGL
				PlayerPrefs.Save ();
				#endif
				ACDebug.Log ("PlayerPrefs key written: " + fullFilename);
				isSuccessful = true;
			}
			catch (Exception e)
 			{
				ACDebug.LogWarning ("Could not save PlayerPrefs data under key " + fullFilename + ". Exception: " + e);
 			}

 			if (isSuccessful)
 			{
 				string dateKey = fullFilename + "_timestamp";

	 			try
	 			{
					DateTime startDate = new DateTime (2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

					int secs = (int) (DateTime.UtcNow - startDate).TotalSeconds;
					string timestampData = secs.ToString ();

					PlayerPrefs.SetString (dateKey, timestampData);
	 				#if UNITY_PS4 || UNITY_SWITCH
					PlayerPrefs.Save ();
					#endif
	 			}
				catch (Exception e)
	 			{
					ACDebug.LogWarning ("Could not save PlayerPrefs data under key " + dateKey + ". Exception: " + e);
	 			}
	 		}

			callback.Invoke (isSuccessful);
		}


		public virtual void Load (SaveFile saveFile, bool doLog, System.Action<SaveFile, string> callback)
		{
			string filename = saveFile.fileName;
			string _data = PlayerPrefs.GetString (filename, string.Empty);
			
			if (doLog && !string.IsNullOrEmpty (_data))
			{
				ACDebug.Log ("PlayerPrefs key read: " + filename);
			}

			callback.Invoke (saveFile, _data);
		}


		public virtual bool SupportsSaveThreading ()
		{
			return false;
		}


		public virtual void GatherSaveFiles (int profileID, System.Action<List<SaveFile>> callback)
		{
			GatherSaveFiles (profileID, false, -1, string.Empty, callback);
		}


		public virtual SaveFile GetSaveFile (int saveID, int profileID)
		{
			return GetSaveFile (saveID, profileID, false, -1, string.Empty);
		}


		public virtual void GatherImportFiles (int profileID, int boolID, string separateProductName, string separateFilePrefix, System.Action<List<SaveFile>> callback)
		{
			if (!string.IsNullOrEmpty (separateProductName) && !string.IsNullOrEmpty (separateFilePrefix))
			{
				GatherSaveFiles (profileID, true, boolID, separateFilePrefix, callback);
			}
		}


		public virtual void SaveScreenshot (SaveFile saveFile)
		{
			string fullFilename = GetSaveFilename (saveFile.saveID, saveFile.profileID) + screenshotKey;

			try
			{
				byte[] bytes = saveFile.screenShot.EncodeToJPG ();
				string dataToSave = Convert.ToBase64String (bytes);

				PlayerPrefs.SetString (fullFilename, dataToSave);
				#if UNITY_PS4 || UNITY_SWITCH
				PlayerPrefs.Save ();
				#endif
				ACDebug.Log ("PlayerPrefs key written: " + fullFilename);
			}
			catch (Exception e)
 			{
				ACDebug.LogWarning ("Could not save PlayerPrefs data under key " + fullFilename + ". Exception: " + e);
 			}
		}

		#endregion


		#region ProtectedFunctions

		protected virtual void GatherSaveFiles (int profileID, bool isImport, int boolID, string separateFilePrefix, System.Action<List<SaveFile>> callback)
		{
			List<SaveFile> gatheredSaveFiles = new List<SaveFile>();

			for (int i = 0; i < MaxSaves; i++)
			{
				SaveFile saveFile = GetSaveFile (i, profileID, isImport, boolID, separateFilePrefix);
				if (saveFile != null)
				{
					gatheredSaveFiles.Add (saveFile);
				}
			}

			callback?.Invoke (gatheredSaveFiles);
		}


		protected virtual SaveFile GetSaveFile (int saveID, int profileID, bool isImport, int boolID, string separateFilePrefix)
		{
			bool isAutoSave = (saveID == 0);
			string filename = (isImport) ? GetImportFilename (saveID, separateFilePrefix, profileID) : GetSaveFilename (saveID, profileID);

			if (PlayerPrefs.HasKey (filename))
			{
				string label = isAutoSave
								? SaveSystem.AutosaveLabel
								: SaveSystem.SaveLabel + " " + saveID.ToString ();

				Texture2D screenShot = null;
				if (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.Always || (KickStarter.settingsManager.saveScreenshots == SaveScreenshots.ExceptWhenAutosaving && !isAutoSave))
				{
					if (PlayerPrefs.HasKey (filename + screenshotKey) && KickStarter.saveSystem)
					{
						try
						{
							string screenshotData = PlayerPrefs.GetString (filename + screenshotKey);
							if (!string.IsNullOrEmpty (screenshotData))
							{
								byte[] result = Convert.FromBase64String (screenshotData);
								if (result != null)
								{
									screenShot = new Texture2D (KickStarter.saveSystem.ScreenshotWidth, KickStarter.saveSystem.ScreenshotHeight, TextureFormat.RGB24, false, KickStarter.settingsManager.linearColorTextures);
									screenShot.LoadImage (result);
									screenShot.Apply ();
								}
							}
						}
						catch (Exception e)
						{
							ACDebug.LogWarning ("Could not load PlayerPrefs data from key " + filename + screenshotKey + ". Exception: " + e);
						}
					}
				}

				int updateTime = 0;

				string dateKey = filename + "_timestamp";
				if (PlayerPrefs.HasKey (dateKey))
				{
					string timestampData = PlayerPrefs.GetString (dateKey);
					if (!string.IsNullOrEmpty (timestampData))
					{
						if (int.TryParse (timestampData, out updateTime))
						{
							DateTime startDate = new DateTime (2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
							DateTime saveDate = startDate.AddSeconds (updateTime);
							DateTime localDate = saveDate.ToLocalTime ();

							if (KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
							{
								label += GetTimeString (localDate);
							}
						}
					}
				}

				return new SaveFile (saveID, profileID, label, filename, screenShot, string.Empty, updateTime);
			}

			return null;
		}


		protected virtual string GetSaveFilename (int saveID, int profileID = -1)
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID ();
			}

			return KickStarter.settingsManager.SavePrefix + SaveSystem.GenerateSaveSuffix (saveID, profileID);
		}


		protected virtual string GetImportFilename (int saveID, string filePrefix, int profileID = -1)
		{
			if (profileID == -1)
			{
				profileID = Options.GetActiveProfileID ();
			}

			return filePrefix + SaveSystem.GenerateSaveSuffix (saveID, profileID);
		}


		protected virtual string GetTimeString (DateTime dateTime)
		{
			if (KickStarter.settingsManager.saveTimeDisplay != SaveTimeDisplay.None)
			{
				if (KickStarter.settingsManager.saveTimeDisplay == SaveTimeDisplay.CustomFormat)
				{
					string creationTime = dateTime.ToString (KickStarter.settingsManager.customSaveFormat);
					return " (" + creationTime + ")";
				}
				else
				{
					string creationTime = dateTime.ToShortDateString ();
					if (KickStarter.settingsManager.saveTimeDisplay == SaveTimeDisplay.TimeAndDate)
					{
						creationTime += " " + dateTime.ToShortTimeString ();
					}
					return " (" + creationTime + ")";
				}
			}

			return string.Empty;
		}

		#endregion


		#region PrivateFunctions

		private void OnGatherFilesForDeletion (List<SaveFile> saveFiles)
		{
			foreach (SaveFile saveFile in saveFiles)
			{
				Delete (saveFile);
			}
		}

		#endregion


		#region GetSet

		protected virtual int MaxSaves { get { return 50; } }

		#endregion
	}

}